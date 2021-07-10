using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Util;
using OxyPlot;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents
{
    /// <summary>
    /// Computes the student's accuracy compared to the expert model at regular time intervals.
    /// </summary>
    public class AccuracyOverProgress : BFCanvasComponent
    {
        private CalculationHelper CalculationHelper { get; }

        /// <summary>
        /// Constructor to initialize a component to compute accuracy over time after an exercise.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at an exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        /// <param name="calculationHelper">Calculation helper to assist calculation of accuracy</param>
        public AccuracyOverProgress(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils, CalculationHelper calculationHelper) : base(studentTraceUtils,
            expertTraceUtils)
        {
            // Configure properties of line graph visualization
            this.Synthesis = new LineGraph
            {
                Title = "Cumulative Accuracy of Student's Trace Compared to Expert's",
                XAxisLabel = "Student Trace Completion (%)",
                YAxisLabel = "Accuracy (%)",
                CurvedLine = false
            };
            CalculationHelper = calculationHelper;
        }

        /// <summary>
        /// Generates stream of x-y coordinates to be used to plot accuracy of student's trace compared to the expert model over trace progress.
        /// </summary>
        /// <returns>A Synthesis object which contains the information to construct a LineGraph for accuracy over progress</returns>
        public override Synthesis Synthesize()
        {
            // Data points to be added to the graph, starting at (0,0).
            List<DataPoint> dataPoints = new List<DataPoint>();
            dataPoints.Add(new DataPoint(0, 0));
            LineGraph result = (LineGraph) this.Synthesis;

            // Put all points from student trace in one StylusPointCollection
            var allStudentPoints = StudentTraceUtils.GetAllPoints();

            // Calculate the length of the expert stroke.
            double expertLength = ExpertTraceUtils.CalculateTraceLengths(null);

            // Calculate lengths up to each stylus point.
            List<double> studentDistances = new List<double>();
            double studentLength = StudentTraceUtils.CalculateTraceLengths(studentDistances);

            // If the expert's trace is longer then the student's, the ratio will be greater than 1.
            // In such cases we want to use the expert's trace length as unit on the x-axis.
            double expertStudentRatio = expertLength / studentLength;

            // Generate parameters for data points generation

            // The number of sections in which the trace wil be divided. A higher number would require more computation.
            // A lower number could possibly not be representative of the progress throughout the trace.
            const int sections = 20;
            int currentSection = 0;
            // The increase in length for 2 consecutive data points.
            double interval = studentLength / sections;
            double currentSectionLength = studentLength / sections;
            // Index of the StylusPoint where the previous interval ended.
            int previousPointIndex = 0;

            // Ratio that was used for real-time feedback for previous point
            double previousRatio = 0d;

            for (int i = 0; i < allStudentPoints.Count; i++)
            {
                // Calculate the accuracy over a section and add the data point to the graph.

                double xValue = (expertStudentRatio < 1) ?
                    studentDistances[i]/studentLength * 100 :
                    // The expert is longer, so these data points will only go up to
                    // a certain fraction of the expert trace completion
                    ((studentDistances[i]/studentLength) * 100) / expertStudentRatio;

                // Calculate the ratio that was used for real-time feedback
                double ratio = RealtimeFeedback.RealtimeFeedback.AccuracyRatio(ExpertTraceUtils, allStudentPoints[i]);
                // Mark the current point if it is the beginning or the end of an error (a ratio of 0 means no error)
                if (previousRatio == 0d && ratio != 0d || previousRatio != 0d && ratio == 0d)
                    result.ErrorZonesXValues.Add(xValue);
                previousRatio = ratio;
                
                if (currentSectionLength > studentDistances[i] && i != allStudentPoints.Count - 1) continue;
                
                // The number of stylus points that will be taken into consideration when calculating the accuracy up to a certain length.
                int pointsToTake = i - previousPointIndex + 1;
                Stroke strokeSection =
                    new Stroke(new StylusPointCollection(allStudentPoints.Skip(previousPointIndex)
                        .Take(pointsToTake)));

                // Convert to StrokeCollection to use in accuracy calculator.
                var strokeSectionCollection = new StrokeCollection {strokeSection};
                
                // Add calculated accuracy to data point collection
                // and put it in the right place in the graph from 0-100%.
                // The expert trace is split to improve hit testing performance
                double yValue = CalculationHelper.CalculateAccuracy(
                    strokeSectionCollection, ExpertTraceUtils.Split(), sections, currentSection);
                
                dataPoints.Add(new DataPoint(xValue, yValue));

                previousPointIndex = i;
                currentSectionLength += interval;
                currentSection++;
            }
            
            // If an error occured at the last point, set the end of the last rectangle here
            if (result.ErrorZonesXValues.Count % 2 != 0) result.ErrorZonesXValues.Add(dataPoints.Last().X);

            // The expert trace is longer, so this will be used on the x-axis.
            if (expertStudentRatio > 1)
            {
                result.XAxisLabel = "Expert Trace Completion (%)";
            }

            // Add line in graph for student data points
            var studentSeries = LineGraph.CreateSeries(dataPoints, OxyColors.Blue, "Student Trace");
            result.AddSeries(studentSeries);

            // Add expert line to the graph, which would be a diagonal line in the graph.
            var expertPoints = CreateExpertDataPoints(expertStudentRatio);
            var expertSeries = LineGraph.CreateSeries(expertPoints, OxyColors.Red, "Expert Trace");
            result.AddSeries(expertSeries);

            return result;
        }

        /// <summary>
        /// Create data points to represent the expert's line.
        /// </summary>
        /// <returns>The data points that represent a diagonal line with slope 1 from (0,0) to (100,100)</returns>
        private static List<DataPoint> CreateExpertDataPoints(double expertFractionOfStudentPoints)
        {
            List<DataPoint> expertPoints = new List<DataPoint>();
            expertPoints.Add(new DataPoint(0, 0));
            // The expert has less points, so it will be completed at this number of points.
            if (expertFractionOfStudentPoints < 1)
            {
                expertPoints.Add(new DataPoint(expertFractionOfStudentPoints * 100, 100));
            }

            expertPoints.Add(new DataPoint(100, 100));


            return expertPoints;
        }
    }
}