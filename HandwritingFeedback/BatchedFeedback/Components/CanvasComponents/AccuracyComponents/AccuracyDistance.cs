using System;
using System.Collections.Generic;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Config;
using HandwritingFeedback.Util;
using OxyPlot;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents
{
    /// <summary>
    /// Configures components to display the distance from the student trace to the expert trace in a graph.
    /// </summary>
    public class AccuracyDistance : BFCanvasComponent
    {
        /// <summary>
        /// Constructor that initializes the labels for the graph.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at a exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        public AccuracyDistance(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils) : base(studentTraceUtils,
            expertTraceUtils)
        {
            Synthesis = new LineGraph
            {
                Title = "Distance to Expert Trace",
                XAxisLabel = "Student Trace Completion (%)",
                YAxisLabel = "Distance (pixels)",
                CurvedLine = false,
                // We keep a consist initial range of the y-axis.
                MinimumYRange = ApplicationConfig.Instance.ClosestPointScanDiameter / 2,
                // The distance can not be negative, so we limit what the graph initially displays.
                AbsoluteMinimumY = 0
            };
        }

        /// <summary>
        /// Generates data points for the graph, from 0-100% of the length of the trace.
        /// </summary>
        /// <returns>A Synthesis object which contains the information to construct <br/>
        /// a LineGraph with the distance from the student trace to the expert trace.</returns>
        public override Synthesis Synthesize()
        {
            var result = (LineGraph) Synthesis;
            var (inRangeDataPoints, outOfRangeDataPoints) = CreateDataPoints();

            // Two series will be added to the graph.
            // - One to show the points that are within the scan range of the closest point algorithm.
            // - The other to show the points that are outside of this range.
            //     - For these points the distance to the closest point is not calculated so the distance is equal to the scan range.
            //     - This line is colored differently to make that distinction.
            var studentSeries = LineGraph.CreateSeries(inRangeDataPoints, OxyColors.Blue, "Student Trace");
            result.AddSeries(studentSeries);
            var studentOutsideSeries = LineGraph.CreateSeries(outOfRangeDataPoints, OxyColors.Orange, "Student Trace Outside Scan Range");
            result.AddSeries(studentOutsideSeries);
            return result;
        }

        /// <summary>
        /// Creates two data point lists that indicate the distance from each student point to the closest expert point.
        /// Smooth transitions are made between the 2 lines.
        /// </summary>
        /// <returns>The inRangeDataPoints list contains the points that are within the scan range of the closest point algorithm. <br/>
        /// The outOfRangeDataPoints list contains the points that are outside the scan range of the closest point algorithm. <br/>
        /// These data points have a distance that is equal to the scan radius.</returns>
        internal (List<DataPoint> inRangeDataPoints, List<DataPoint> outOfRangeDataPoints) CreateDataPoints()
        {

            var inRangeDataPoints = new List<DataPoint>();
            var allStudentStylusPoints = StudentTraceUtils.GetAllPoints();

            var studentTraceLengths = new List<double>();
            double studentTraceLength = StudentTraceUtils.CalculateTraceLengths(studentTraceLengths);

            // A different line will be displayed when the student is outside the scan area of the closest point algorithm.
            var outOfRangeDataPoints = new List<DataPoint>();
            
            // The maximum distance the user can deviate from the expert trace before leaving the scan area
            // This value is similar to the scanDiameter calculated in TraceUtils.GetClosestPoint,
            // but the calculation here gives the radius instead of the diameter.
            double maxDistance = Math.Max(ApplicationConfig.Instance.ClosestPointScanDiameter / 2d,
                2d * ApplicationConfig.Instance.MaxDeviationRadius);
            
            // This subtraction is needed to adjust the highest possible y-value in order to
            // take the thickness of the expert's stroke outline into account.
            maxDistance -= ApplicationConfig.Instance.MaxDeviationRadius;

            // Used to check when to transition from one line to the other.
            bool previousIsOutsideScanArea = false;

            double yCoordinate = -1d;

            int studentIndex = 0;
            foreach (StylusPoint studentPoint in allStudentStylusPoints)
            {
                double previousY = yCoordinate;

                // This indicates how much of the student trace has been completed.
                double xValue = (studentTraceLengths[studentIndex++] / studentTraceLength) * 100d;
                yCoordinate = ExpertTraceUtils.GetClosestPoint(studentPoint.ToPoint()).distance;

                // The closest point is outside of the scan range.
                if (double.IsInfinity(yCoordinate))
                {
                    yCoordinate = maxDistance;

                    // Only add a new data point when the end is reached or
                    // the current point is the start of an out of bounds part of the graph.
                    if (double.IsInfinity(previousY) || previousY == maxDistance)
                    {
                        // Add a datapoint at the end otherwise the last part of the line won't be displayed.
                        if (studentIndex == allStudentStylusPoints.Count - 1)
                        {
                            outOfRangeDataPoints.Add(new DataPoint(xValue, yCoordinate));
                        }
                        continue;
                    }
                    inRangeDataPoints.Add(new DataPoint(xValue, maxDistance));
                    // The in-range line will cut off here.
                    inRangeDataPoints.Add(new DataPoint(xValue, Double.PositiveInfinity));

                    // This is the start of the line where the student points are getting out of the scan range.
                    outOfRangeDataPoints.Add(new DataPoint(xValue, yCoordinate));

                    previousIsOutsideScanArea = true;
                    continue;
                }

                // The out-of-range line stops here.
                if (previousIsOutsideScanArea)
                {
                    outOfRangeDataPoints.Add(new DataPoint(xValue, maxDistance));
                    // The out-of-range line will be cut off here.
                    outOfRangeDataPoints.Add(new DataPoint(xValue, Double.PositiveInfinity));

                    // The in-range line starts again here. 
                    inRangeDataPoints.Add(new DataPoint(xValue, maxDistance));
                }
                // We add a data point for the student points that are within range,
                // with the distance to the closest expert point.
                
                // The actual distance from the expert depends on the
                // thickness of the expert's trace.
                var relativeDistance = 
                    Math.Max(0, yCoordinate - ApplicationConfig.Instance.MaxDeviationRadius);
                inRangeDataPoints.Add(new DataPoint(xValue, 
                    Math.Min(relativeDistance, maxDistance)));

                previousIsOutsideScanArea = false;
            }

            return (inRangeDataPoints, outOfRangeDataPoints);
        }
    }
}