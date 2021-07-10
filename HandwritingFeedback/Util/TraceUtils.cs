using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.Config;
using HandwritingFeedback.StylusPlugins.Strokes;

namespace HandwritingFeedback.Util
{
    /// <summary>
    /// Contains methods for manipulating and analyzing data of a StrokeCollection (which we call a 'trace').
    /// </summary>
    public class TraceUtils
    {
        /// <summary>
        /// The trace to use in calculations.
        /// </summary>
        public readonly StrokeCollection Trace;

        /// <summary>
        /// Split version of Trace, can be used for faster hit testing.
        /// Segment size that was used to compute it is also stored here.
        /// </summary>
        private (StrokeCollection trace, int segmentSize) _splitTrace;

        /// <summary>
        /// Cache that stores the closest expert point and its distance, given a point. <br />
        /// This eliminates the need to recalculate the closest point when GetClosestPoint <br />
        /// is called multiple times with the same point, for example when called by multiple feedback components.
        /// </summary>
        private readonly IDictionary<Point, (StylusPoint, double)> _closestPointsCache
            = new Dictionary<Point, (StylusPoint, double)>();

        /// <summary>
        /// Dictionary that maps (strokeIndex, pointIndex) for all points in splitTrace to <br />
        /// (strokeIndex, pointIndex) for all points in trace. <br />
        /// This allows us to find the corresponding indices for a point in trace <br />
        /// given the indices for a point in splitTrace. <br />
        /// Used when finding the neighbors of the closest point in GetClosestPoint.
        /// </summary>
        private readonly IDictionary<(int, int), (int, int)> _splitTraceIndices 
            = new Dictionary<(int, int), (int, int)>();

        private (bool isCalculated, bool result) _validTemporalCache;
        
        /// <summary>
        /// Constructor that sets the trace.
        /// </summary>
        /// <param name="trace">The trace that will be used for calculations</param>
        public TraceUtils(StrokeCollection trace)
        {
            Trace = trace;
        }

        /// <summary>
        /// Splits each stroke of Trace with stylus point count > segmentSize <br />
        /// into multiple strokes of stylus point count &lt;= segmentSize. <br />
        /// Splitting strokes can improve performance for hit testing, but the segmentSize needs to be chosen carefully.
        /// When rendered the resulting collection is visually equivalent to the original collection.
        /// </summary>
        /// <param name="segmentSize">The maximum number of StylusPoints in a stroke before the stroke is split. <br />
        /// 100 was found to be a good value in manual testing</param>
        /// <returns>New stroke collection where the strokes of strokeCollection <br />
        /// have been segmented into strokes of size segmentSize. <br />
        /// When rendered this collection is visually equivalent to the original collection</returns>
        public StrokeCollection Split(int segmentSize = 100)
        {
            // Return if the calculation has been performed before
            if (_splitTrace.trace != null && _splitTrace.segmentSize == segmentSize) 
                return _splitTrace.trace;

            _splitTrace = (new StrokeCollection(), segmentSize);
            
            // This thread needs exclusive access for using the Dictionary
            Monitor.Enter(_splitTraceIndices);

            for (int i = 0; i < Trace.Count; i++)
            {
                Stroke stroke = Trace[i];
                
                // The number of StylusPoints in the current stroke
                int count = stroke.StylusPoints.Count;
                
                // The number of points taken from the current stroke
                int taken = 0;

                // We get the common description between the expert trace stylus points and the configuration to ensure
                // that we don't get any mismatch errors that might be cause by using one type of input device for the
                // expert trace and another type for the student trace
                StylusPointDescription description = StylusPointDescription.GetCommonDescription(
                    stroke.StylusPoints.Description,
                    ApplicationConfig.Instance.StylusPointDescription);

                // We need to reformat the points to ensure the description is the same
                // as the common description we calculated above
                stroke = new Stroke(stroke.StylusPoints.Reformat(description));
                
                while (taken < count)
                {
                    StylusPointCollection segmentPoints = new StylusPointCollection(description);

                    // Add a selection of StylusPoints to the current segment
                    for (int j = 0; j < segmentSize && taken + j < count; j++)
                    {
                        // Include the last point of the previous segment in the new segment
                        // This ensures that the segments are connected
                        if (taken > 0 && j == 0)
                        {
                            _splitTraceIndices.Add((_splitTrace.trace.Count, segmentPoints.Count), (i, taken - 1));
                            segmentPoints.Add(stroke.StylusPoints[taken - 1]);
                        }
                        // Cache index of stroke which the point belongs to in the original trace
                        _splitTraceIndices.Add((_splitTrace.trace.Count, segmentPoints.Count), (i, taken + j));
                        
                        segmentPoints.Add(stroke.StylusPoints[taken + j]);
                    }

                    // Add new stroke containing points in the segment to the result
                    _splitTrace.trace.Add(new Stroke(segmentPoints));
                    taken += segmentSize;
                }
            }

            Monitor.Exit(_splitTraceIndices);

            return _splitTrace.trace;
        }

        /// <summary>
        /// Finds the interpolated point in Trace closest to the given point within the given <br />
        /// scanDiameter, and the distance to that point. <br />
        /// The closest interpolated point is found by projecting point onto the
        /// line segment between the two closest expert points in Trace. <br />
        /// The pressure value is linearly interpolated between these points.
        /// </summary>
        /// <param name="point">The point for which to find closest neighbor</param>
        /// <returns>The closest interpolated point within scanDiameter <br />
        /// and its distance to point, or a point with coordinates (double.NegativeInfinity, double.NegativeInfinity) <br />
        /// if no point was found within scanDiameter and distance infinity</returns>
        public (StylusPoint stylusPoint, double distance) GetClosestPoint(Point point)
        {
            // This thread needs exclusive access for using the cache
            Monitor.Enter(_closestPointsCache);
            
            // We round the X and Y coordinates, because when using a stylus
            // these coordinates are very precise, and therefore almost never repeat.
            // Without rounding, a cache hit would be extremely rare
            Point roundedPoint = new Point((int)point.X, (int)point.Y);

            // The scan diameter is at least 100 (good value found during functional testing)
            // and can be greater if the MaxDeviationRadius requires it.
            // 2d * MaxDeviationRadius makes the dashed line start exactly where the trace changes its
            // color to red. Multiplying the diameter by 2 adds some space between the red and the
            // dashed line.
            double scanDiameter = Math.Max(ApplicationConfig.Instance.ClosestPointScanDiameter,
                4d * ApplicationConfig.Instance.MaxDeviationRadius);

            // If this point was calculated earlier, return the result
            if (_closestPointsCache.ContainsKey(roundedPoint))
            {
                var cached = _closestPointsCache[roundedPoint];
                Monitor.Exit(_closestPointsCache);
                return cached;
            }

            double minDistance = double.PositiveInfinity;
            // Indices of the closest point in the split trace
            (int strokeIndex, int pointIndex) closestPointIndices = (-1, -1);

            for (int i = 0; i < Split().Count; i++)
            {
                Stroke strokeSegment = Split()[i];
                
                // Only consider the points in the stroke segment if it is within the scan diameter
                if (!strokeSegment.HitTest(point, scanDiameter)) continue;

                // Find the closest point in the stroke segment
                for (int j = 0; j < strokeSegment.StylusPoints.Count; j++)
                {
                    StylusPoint candidate = strokeSegment.StylusPoints[j];
                    Vector v = Point.Subtract(point, candidate.ToPoint());
                    double distance = v.Length;
                    if (distance < minDistance)
                    {
                        closestPointIndices = (i, j);
                        minDistance = distance;
                    }
                }
            }

            // If no closest point was found return
            if (double.IsInfinity(minDistance))
            {
                Monitor.Exit(_closestPointsCache);
                return (new StylusPoint(double.NegativeInfinity, double.NegativeInfinity), minDistance);
            }

            // Get indices of closest point in the original trace
            (int originalStrokeIndex, int originalPointIndex) = _splitTraceIndices[closestPointIndices];
            
            StylusPoint closestPoint = Trace[originalStrokeIndex].StylusPoints[originalPointIndex];
            
            // Get point before closest if it exists, else use closestPoint
            StylusPoint leftNeighbor = originalPointIndex - 1 >= 0
                ? Trace[originalStrokeIndex].StylusPoints[originalPointIndex - 1]
                : closestPoint;

            // Get point after closest if it exists, else use closestPoint
            StylusPoint rightNeighbor = originalPointIndex + 1 < Trace[originalStrokeIndex].StylusPoints.Count
                ? Trace[originalStrokeIndex].StylusPoints[originalPointIndex + 1]
                : closestPoint;
            
            // Get projections of point on the line segments, and corresponding distances
            var leftResult = ProjectOnLineSegment(point, closestPoint, leftNeighbor);
            var rightResult = ProjectOnLineSegment(point, closestPoint, rightNeighbor);

            var result = leftResult.distance < rightResult.distance ? leftResult : rightResult;

            // Cache the result for future requests
            _closestPointsCache.Add(roundedPoint, result);
            
            Monitor.Exit(_closestPointsCache);

            // Return the closestPoint together with the minDistance
            // The minDistance is returned, such that it won't have to
            // be recalculated by the calling method.
            return result; 
        }

        /// <summary>
        /// Projects point onto the line segment between lineFrom and lineTo.
        /// </summary>
        /// <param name="point">Point to project</param>
        /// <param name="lineFrom">Start of the line segment</param>
        /// <param name="lineTo">End of the line segment</param>
        /// <returns>If projection is possible: projection of point and the distance between point and its projection, <br />
        /// else: lineFrom and the distance between point and lineFrom</returns>
        private static (StylusPoint stylusPoint, double distance) ProjectOnLineSegment(Point point, StylusPoint lineFrom, StylusPoint lineTo)
        {
            // Vector representing the line segment
            var lineSegment = Point.Subtract(lineTo.ToPoint(), lineFrom.ToPoint());
            if (lineSegment.Length == 0d) return (lineFrom, Point.Subtract(lineFrom.ToPoint(), point).Length);
            // Vector from lineFrom to point
            var pointVector = Point.Subtract(point, lineFrom.ToPoint());

            // The vector that we are projecting onto (lineSegment) will be multiplied by this scalar
            double scalar = DotProduct(pointVector, lineSegment)
                            / DotProduct(lineSegment, lineSegment);
            // Scalar must be between 0 and 1, otherwise the projection will not land on the line segment
            if (scalar < 0f - double.Epsilon || scalar > 1f + double.Epsilon)
                return (lineFrom, Point.Subtract(lineFrom.ToPoint(), point).Length);
            
            // Projected point on the line segment
            StylusPoint projectedPoint = InterpolatePoint(lineFrom, lineTo, scalar);
            
            double distance = Point.Subtract(projectedPoint.ToPoint(), point).Length;
            
            return (projectedPoint, distance);
        }

        // Helper method that calculates the dot product between two vectors.
        private static double DotProduct(Vector l, Vector r)
        {
            return l.X * r.X + l.Y * r.Y;
        }

        /// <summary>
        /// Interpolates all attributes of two StylusPoints, based on ratio.
        /// </summary>
        /// <param name="from">Point to start from</param>
        /// <param name="to">Point to go to</param>
        /// <param name="ratio">How far to go between from and to, 0 means stay at from, 1 means go to to</param>
        /// <returns>New StylusPoint with attributes interpolated between from and to</returns>
        public static StylusPoint InterpolatePoint(StylusPoint from, StylusPoint to, double ratio)
        {
            double x = from.X + ratio * (to.X - from.X);
            double y = from.Y + ratio * (to.Y - from.Y);
            
            // Interpolate all attributes based on the ratio between from and to
            float pressure = (float)((1d - ratio) * from.PressureFactor + ratio * to.PressureFactor);
            pressure = Math.Clamp(pressure, 0f, 1f);
            
            // By default, a stylus point has 3 properties, X coordinate, Y coordinate and pressure
            // We will use this variable to keep track of all the extra properties added
            const int defaultPropertyCount = 3;
            var additionalPropertyValues = new List<int>();
            
            // We get the common description between the expert trace stylus points and the configuration to ensure
            // that we don't get any mismatch errors that might be cause by using one type of input device for the
            // expert trace and another type for the student trace
            StylusPointDescription originalPointsDescription  =
                StylusPointDescription.GetCommonDescription(from.Description, to.Description);

            // We get all of the properties and then only keep the non-default ones
            ReadOnlyCollection<StylusPointPropertyInfo> stylusPointProperties = originalPointsDescription.GetStylusPointProperties();
            List<StylusPointPropertyInfo> additionalStylusPointProperties =
                stylusPointProperties.TakeLast(stylusPointProperties.Count - defaultPropertyCount).ToList();

            // We don't need to check if the points given for interpolation contain the property
            // since we used the common description to get these non-default properties
            // For each property, we calculate its interpolated value
            foreach (var pointProperty in additionalStylusPointProperties)
            {
                var additionalValue = (int) ((1d - ratio) * from.GetPropertyValue(pointProperty) +
                                             ratio * to.GetPropertyValue(pointProperty));
                additionalValue = Math.Clamp(additionalValue, pointProperty.Minimum, pointProperty.Maximum);
                additionalPropertyValues.Add(additionalValue);
            }

            // We check that all the non default properties were calculated. If not, we return a default
            // stylus point. We also check if we added at least one non default property because otherwise
            // we need to return the default stylus point
            return additionalPropertyValues.Count > 0 && defaultPropertyCount + 
                additionalPropertyValues.Count == originalPointsDescription.PropertyCount
                ? new StylusPoint(x, y, pressure, ApplicationConfig.Instance.StylusPointDescription,
                    additionalPropertyValues.ToArray())
                : new StylusPoint(x, y, pressure);
        }
        
        /// <summary>
        /// Adds all StylusPoints in Trace to 1 StylusPointCollection. 
        /// </summary>
        /// <returns>A StylusPointCollection with all StylusPoints from Trace</returns>
        public StylusPointCollection GetAllPoints()
        {
            // We get the common description between the expert trace stylus points and the configuration to ensure
            // that we don't get any mismatch errors that might be cause by using a mouse for the expert trace
            // and a stylus for practice mode (or the other way around)
            StylusPointDescription description = Trace.Aggregate(ApplicationConfig.Instance.StylusPointDescription, 
                (current, stroke) => StylusPointDescription.GetCommonDescription(current, stroke.StylusPoints.Description));
            
            var allPoints = new StylusPointCollection(description);
            foreach (Stroke stroke in Trace)
            {
                // We need to reformat the points to ensure the description is the same
                // as the common description of all stylus Points
                StylusPointCollection reformattedStylusPoints = stroke.StylusPoints.Reformat(description);
                allPoints.Add(reformattedStylusPoints);
            }

            return allPoints;
        }
        
        /// <summary>
        /// Calculates the number of StylusPoints in Trace.
        /// </summary>
        /// <returns>The number of StylusPoints in Trace</returns>
        public int GetNumberOfStylusPoints()
        {
            return Trace.Sum(stroke => stroke.StylusPoints.Count);
        }
        
        /// <summary>
        /// Calculates the total length of Trace and saves the distance to each point in a list.
        /// The distance between the end of a stroke and the beginning of the next stroke is not incorporated in the calculation.
        /// </summary>
        /// <param name="distances">The list to store the distances to each point in</param>
        /// <returns>The total lenght of Trace</returns>
        public double CalculateTraceLengths(List<double> distances)
        {
            double length = 0;
            foreach (var stroke in Trace)
            {
                var previousPoint = stroke.StylusPoints[0].ToPoint();
                foreach (var point in stroke.StylusPoints)
                {
                    length += Point.Subtract(previousPoint, point.ToPoint()).Length;
                    distances?.Add(length); // The ? ensures that we only add to the list when it isn't null
                    previousPoint = point.ToPoint();
                }
            }
            
            return length;
        }
        
        /// <summary>
        /// Checks if the internal stroke collection contains temporal cache data.
        /// The cached result is returned if the computation has previously been performed.
        /// </summary>
        /// <returns>Boolean indicating whether trace contains valid temporal caches</returns>
        public bool ContainsValidTemporalCaches()
        {
            if (_validTemporalCache.isCalculated) return _validTemporalCache.result;

            var result = true;
            foreach (var stroke in Trace)
            {
                if (!stroke.GetType().IsSubclassOf(typeof(GeneralStroke))
                    && stroke.GetType() != typeof(GeneralStroke))
                {
                    result = false;
                    break;
                }

                var current = (GeneralStroke) stroke;

                if (current.TemporalCacheSnapshot is null || current.TemporalCacheSnapshot.Length == 0)
                {
                    result = false;
                    break;
                }
                
            }
            
            _validTemporalCache = (isCalculated: true, result);
            return result;
        }
    }
}