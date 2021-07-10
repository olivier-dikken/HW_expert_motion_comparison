using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Config;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.Util;
using OxyPlot;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.SpeedComponents
{
    /// <summary>
    /// Computes user's speed compared to trace completion, in comparison with the expert model.
    /// </summary>
    public class SpeedOverProgress : BFCanvasComponent
    {
        /// <summary>
        /// Constructor to compute user speed over progress.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at a exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        public SpeedOverProgress(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils) : base(studentTraceUtils,
            expertTraceUtils)
        {
            Synthesis = new LineGraph
            {
                Title = "Speed of Student's Trace Compared to Expert's",
                XAxisLabel = "Student Trace Completion (%)",
                YAxisLabel = "Speed (pixels/s)",
                CurvedLine = false
            };
        }

        /// <summary>
        /// Computes and constructs a line graph comparing stroke speed of student's compared <br />
        /// to expert's, given that expert data on time is available.
        /// If expert temporal data is unavailable due to an out-dated .ISF file or file created in <br />
        /// external application, only student speed is calculated.
        /// </summary>
        /// <returns> Line graph comparing speed of student compared to expert </returns>
        public override Synthesis Synthesize()
        {
            var result = (LineGraph) Synthesis;

            // Check if expert trace contains temporal data
            // This may be unavailable due to an out-dated .ISF file or file created in external application
            bool expertTraceCompatible = ExpertTraceUtils.ContainsValidTemporalCaches();

            // Create mappings for temporal data for both traces in parallel
            // Expert mapping will only be constructed given that temporal data is available
            var studentMapping = Task.Factory.StartNew(() 
                => CreateTemporalMapping(StudentTraceUtils.Trace));
            var expertMapping = Task.Factory.StartNew(() 
                => expertTraceCompatible ? CreateTemporalMapping(ExpertTraceUtils.Trace) : null);

            // Wait for construction of mappings to be completed
            Task.WaitAll(new Task[] { studentMapping, expertMapping });

            // Generate points for both traces in parallel
            // Expert points will only be generated given that temporal data is available
            var studentPoints = Task.Factory.StartNew(() 
                => GeneratePoints(StudentTraceUtils.Trace, studentMapping.Result));
            var expertPoints = Task.Factory.StartNew(()
                => expertTraceCompatible ? GeneratePoints(ExpertTraceUtils.Trace, expertMapping.Result) : (null, 0));

            // Wait for generation of both student and expert points
            Task.WaitAll(studentPoints, expertPoints);

            // Set the longest trace length to be the length of the longest trace from the student and expert
            // Longest trace length will be used to constrain the x-axis between 0 to 100%
            var longestTraceLength = studentPoints.Result.traceLength;
            if (expertTraceCompatible && longestTraceLength < expertPoints.Result.traceLength)
            {
                // If expert trace is longer, x-axis will be based on expert trace completion
                result.XAxisLabel = "Expert Trace Completion (%)";
                longestTraceLength = expertPoints.Result.traceLength;
            }

            // Check is expert trace contains temporal data
            if (expertTraceCompatible)
            {
                // If temporal data is available for expert trace, normalize both student and expert data points
                // in parallel
                var studentNormalization = Task.Factory.StartNew(()
                    => this.NormalizeAndAverageDataPoints(studentPoints.Result.dataPoints, longestTraceLength));
                var expertNormalization = Task.Factory.StartNew(() 
                    => this.NormalizeAndAverageDataPoints(expertPoints.Result.dataPoints, longestTraceLength));;

                // Wait for normalization of both student and expert data points to be complete
                Task.WaitAll(studentNormalization, expertNormalization);
                
                // Prevent the graph from throwing an exception when not enough points are available
                if (studentPoints.Result.dataPoints.Count == 1 && expertPoints.Result.dataPoints.Count == 1)
                {
                    studentPoints.Result.dataPoints.Add(new DataPoint(100, 0));
                    expertPoints.Result.dataPoints.Add(new DataPoint(100, 0));
                }
            
                // Construct line series for normalized student and expert data points, then add to line graph
                result.AddSeries(LineGraph
                    .CreateSeries(expertPoints.Result.dataPoints, OxyColors.Red, "Expert Trace"));
                result.AddSeries(LineGraph
                    .CreateSeries(studentPoints.Result.dataPoints, OxyColors.Blue, "Student Trace"));
            } else
            {
                // If temporal data is unavailable for expert trace, normalize only student data points
                this.NormalizeAndAverageDataPoints(studentPoints.Result.dataPoints, longestTraceLength);

                // Prevent the graph from throwing an exception when not enough points are available
                if (studentPoints.Result.dataPoints.Count == 1)
                {
                    studentPoints.Result.dataPoints.Add(new DataPoint(100, 0));
                }
                
                // Construct line series for normalized student data points, then add to line graph
                result.AddSeries(LineGraph
                    .CreateSeries(studentPoints.Result.dataPoints, OxyColors.Blue, "Student Trace"));
            }

            return result;
        }

        /// <summary>
        /// Creates a temporal mapping using the temporal cache of a trace, which will be used to compute <br />
        /// speed over progress.
        /// This provides quick access to timestamps identified by points, in sequential order.
        /// A mapping is necessary due to the unordered stylus point collections in events - if we were to compute <br />
        /// distance covered in events without using the natural ordering, total distance would be inaccurate and amplified.
        /// </summary>
        /// <param name="trace"> Trace with associated temporal cache for which mapping must be created </param>
        /// <returns> Mapping for temporal cache, where timestamps are identified in sequential order by the <br />
        /// co-ordinates of their stylus points</returns>
        public static Dictionary<Point, Queue<double>> CreateTemporalMapping(StrokeCollection trace)
        {
            var mapping = new Dictionary<Point, Queue<double>>();

            // Iterate through every stroke in the given trace
            foreach (var currentStroke in trace)
            {
                // Cast current stroke to an instance of GeneralStroke in order to access temporal cache
                var stroke = (GeneralStroke) currentStroke;

                // Iterate through every event in current temporal cache snapshot,
                // in order to store them in the new mapping
                foreach (var (stylusPoints, timestamp) in stroke.TemporalCacheSnapshot)
                {
                    // Iterate through every stylus point associated with the current event,
                    // in order to store them separately in the new mapping
                    foreach (var point in stylusPoints)
                    {
                        Point roundedPoint = new Point(Math.Round(point.X, 0), Math.Round(point.Y, 0));
                        
                        // Check if current rounded stylus point has already been encountered and
                        // extract queue of timestamps if true
                        if (mapping.TryGetValue(roundedPoint, out Queue<double> timestamps))
                        {
                            // If key for current stylus point exists, add new timestamp to end of queue
                            // such that it will be accessed last
                            timestamps.Enqueue(timestamp);
                        }
                        else
                        {
                            // If key for current stylus point does not exist, construct new queue of timestamps
                            // in order to store timestamps per stylus point sequentially and add the new
                            // timestamp to the head of the new queue
                            timestamps = new Queue<double>();
                            timestamps.Enqueue(timestamp);
                            mapping.Add(roundedPoint, timestamps);
                        }
                    }
                }
            }

            // Return newly constructed mapping, providing easy access to timestamps via stylus point coordinates
            return mapping;
        }

        /// <summary>
        /// Generates data points where each x-coordinate is the distance covered
        /// between the start and end of the trace, mapped to one straight line.
        /// The y-coordinate is the speed in pixels per second at the given distance.
        /// It returns these data points along with the total length of the trace.
        /// </summary>
        /// <param name="trace">The trace to generate data points from.</param>
        /// <param name="temporalMapping"></param>
        /// <returns>Tuple of the data points and the total length of the trace</returns>
        public (List<DataPoint> dataPoints, double traceLength) GeneratePoints(
            StrokeCollection trace, Dictionary<Point, Queue<double>> temporalMapping)
        {
            // Construct new list of data points and add stationary speed to timestamp zero
            // This represents stationary nature of hand prior to start of writing
            List<DataPoint> dataPoints = new List<DataPoint>() { new DataPoint(0, 0) };

            // Initialize distance of a single segment
            // Speed is calculated over a single segment in order to simplify calculation
            double segmentDistance = 0;

            // Initialize total distance to zero and previous timestamp (i.e. timestamp of previous segment)
            // to the timestamp in the first stylus point of the provided trace.
            // Timestamp is stored as the number of milliseconds relative to system boot-up.
            double totalDistance = 0;
            this.TryGetPointTimestamp(trace[0].StylusPoints[0].ToPoint(), temporalMapping, 
                out var prevTimestamp, peek: true);
            
            // Iterate through every stroke in the provided trace
            foreach (Stroke stroke in trace)
            {
                // Set the previous point to the first stylus point in the current stroke
                Point prevPoint = stroke.StylusPoints[0].ToPoint();

                // Iterate through every stylus point in the current stroke
                // These stylus points are stored sequentially in correct order of input, such
                // that distance between sequential stylus points is reflective of distance covered by
                // the user's stylus on the digitizer
                foreach (StylusPoint currPoint in stroke.StylusPoints)
                {
                    // Compute the distance covered between the current point and previous point
                    Point point = currPoint.ToPoint();
                    segmentDistance += Point.Subtract(prevPoint, point).Length;

                    // Check if the current point exists in the given temporal mapping
                    // Points may not exist due to minor rounding errors caused by the creation of
                    // stroke collections; we use this boolean to tackle these errors.
                    // We do not account for errors caused by rounding explicitly as the line series
                    // does not have to be completely accurate as they will only result in minor deviations
                    // from the actual speed over progress line series, imperceptible to the human eye.
                    bool pointExists = this.TryGetPointTimestamp(point, temporalMapping, 
                        out double segmentTimestamp);


                    // Compute total duration of segment as difference between current timestamp and time elapsed
                    double segmentTime = segmentTimestamp - prevTimestamp;

                    // Execute conditional if point exists in mapping and total segment duration exceeded 10ms
                    // 10ms segments were chosen due to positive results during functional testing
                    if (pointExists && segmentTime >= 10)
                    {
                        // Update total distance and calculate segment speed
                        totalDistance += segmentDistance;
                        double speed = this.CalculateSpeed(segmentDistance, segmentTime);

                        // Add new data point with segment speed and distance covered
                        dataPoints.Add(new DataPoint(totalDistance, speed));

                        // Reset segment distance and update previous timestamp
                        segmentDistance = 0;
                        prevTimestamp = segmentTimestamp;
                    }
                    
                    // If the previous point was not found in the temporal mapping,
                    // the previous timestamp still needs to be updated.
                    if (double.IsInfinity(prevTimestamp))
                        prevTimestamp = segmentTimestamp;

                    // Update the previous point to the current point
                    prevPoint = point;
                }
            }

            return (dataPoints, totalDistance);
        }

        /// <summary>
        /// Tries to get the next timestamp associated with a given coordinate in a temporal mapping.
        /// </summary>
        /// <param name="point">Point for which timestamp must be retrieved</param>
        /// <param name="temporalMapping">Temporal mapping consisting of all co-ordinates in trace  <br />
        /// associated with timestamps in sequential order per coordinate</param>
        /// <param name="value">Value to be updated with retrieved timestamp</param>
        /// <param name="peek">Indicates whether timestamp should be removed from mapping or only peeked at</param>
        /// <returns>True if the retrieval of the timestamp from the mapping was successful, false otherwise.</returns>
        public bool TryGetPointTimestamp(Point point, Dictionary<Point, 
            Queue<double>> temporalMapping, out double value, bool peek = false)
        {
            // Check if mapping contains key with provided coordinates and if the queue of timestamps at the given
            // coordinate is empty
            if (temporalMapping.TryGetValue(new Point(Math.Round(point.X, 0), Math.Round(point.Y, 0)), 
                out Queue<double> timestampQueue) && timestampQueue.Count > 0)
            {
                // If key exists in mapping and timestamps queue is non-empty, return first timestamp in queue
                // Remove the timestamp from the queue if necessary
                value = peek ? timestampQueue.Peek() : timestampQueue.Dequeue();
                return true;
            }

            // If mapping does not contain key or timestamps queue is empty, return false indicating
            // retrieval was unsuccessful
            value = double.PositiveInfinity;
            return false;
        }

        /// <summary>
        /// Calculates the average speed in a given segment
        /// </summary>
        /// <param name="distance">Distance covered during a segment</param>
        /// <param name="timeInMilliseconds">Duration in milliseconds of a segment</param>
        /// <returns>Average speed over a segment</returns>
        public double CalculateSpeed(double distance, double timeInMilliseconds)
        {
            // Convert segment duration from milliseconds to seconds
            double timeInSeconds = timeInMilliseconds / 1000;

            // If segment duration is invalid, return stationary speed
            // Else return speed given by the formula distance / time
            return timeInMilliseconds <= 0 ? 0 : distance / timeInSeconds;
        }

        /// <summary>
        /// Averages the y-coordinates of the data points by averaging a fixed number of 
        /// consecutive points, then normalizes the x-coordinate of the data points by dividing
        /// the value by the length of the longest trace.
        /// Averaging will smoothen the speed over progress curve to reduce the severity
        /// of fluctuations caused by event-based timestamps, in order to obtain a more interpretable
        /// curve.
        /// </summary>
        /// <param name="dataPoints">The data points to average and normalize</param>
        /// <param name="longestTraceLength">The length of the trace to divide by</param>
        public void NormalizeAndAverageDataPoints(List<DataPoint> dataPoints, double longestTraceLength)
        {
            // Number of neighboring data points to take into consideration for the averaging of
            // speed chosen through functional testing
            int neighbors = ApplicationConfig.Instance.DataPointsAveragingNeighbors;

            // Copy list of data points to temporary variable, in order to use it as a read-only
            // reference for averaging purposes
            var dataPointsRef = new DataPoint[dataPoints.Count];
            dataPoints.CopyTo(dataPointsRef);

            // In parallel, average and normalize all data points
            Parallel.For(0, dataPoints.Count, dataPointIndex =>
            {
                double averageY = 0;

                // Collect neighbors in both directions from the current data point
                for (var i = dataPointIndex - (neighbors / 2); i <= dataPointIndex + neighbors / 2; i++)
                {
                    // Clamp index between zero and the length of the list of data points, to avoid index
                    // out of bounds errors
                    var index = Math.Clamp(i, 0, dataPoints.Count - 1);

                    // Accumulate the speed value for the current data point
                    averageY += dataPointsRef[index].Y;
                }

                // Average the speed for collected neighboring data points
                // +1 because the mid-point is also considered, which means we have
                // neighbors + 1 points to consider
                averageY /= neighbors + 1;

                // Normalize the length and update the data point
                dataPoints[dataPointIndex] = new DataPoint(
                    dataPoints[dataPointIndex].X / longestTraceLength * 100
                    , averageY);
            });
        }
    }
}
