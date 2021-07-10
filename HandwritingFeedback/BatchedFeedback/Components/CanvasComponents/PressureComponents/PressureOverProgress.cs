using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Util;
using OxyPlot;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.PressureComponents
{
    /// <summary>
    /// Configures components to display the pressure throughout the student and expert's trace in a graph.
    /// </summary>
    public class PressureOverProgress : BFCanvasComponent
    {
        /// <summary>
        /// Constructor that initializes the labels for the graph.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at a exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        public PressureOverProgress(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils) : base(studentTraceUtils,
            expertTraceUtils)
        {
            Synthesis = new LineGraph
            {
                Title = "Pressure of Student's Trace Compared to Expert's",
                XAxisLabel = "Student Trace Completion (%)",
                YAxisLabel = "Pressure (%)",
                CurvedLine = false
            };
        }

        /// <summary>
        /// Generates data points for the graph, from 0-100% of the length of the trace.
        /// </summary>
        /// <returns>A Synthesis object which contains the information to construct a LineGraph for pressure over progress</returns>
        public override Synthesis Synthesize()
        {
            var result = (LineGraph) Synthesis;
            
            var studentTraceLengths = new List<double>();
            var expertTraceLengths = new List<double>();
            double studentTraceLength = StudentTraceUtils.CalculateTraceLengths(studentTraceLengths);
            double expertTraceLength = ExpertTraceUtils.CalculateTraceLengths(expertTraceLengths);
            
            // This will be used to put the lengths between 0-100%.
            var biggestTraceLength = studentTraceLength;
            
            if (expertTraceLength > studentTraceLength)
            {
                // The expert's trace is longer, so this will be used as x-axis.
                result.XAxisLabel = "Expert Trace Completion (%)";
                biggestTraceLength = expertTraceLength;
            }

            var studentDataPoints = new List<DataPoint>();
            var expertDataPoints = new List<DataPoint>();

            // Ratio that was used for real-time feedback for previous point
            double previousRatio = 0d;
            
            int studentIndex = 0;
            foreach (StylusPoint studentPoint in StudentTraceUtils.GetAllPoints())
            {
                double xValue = (studentTraceLengths[studentIndex] / biggestTraceLength) * 100d;
                
                // Calculate the ratio that was used for real-time feedback
                double ratio = RealtimeFeedback.RealtimeFeedback.PressureRatio(ExpertTraceUtils, studentPoint);
                // Mark the current point if it is the beginning or the end of an error (a ratio of 0 means no error)
                if (previousRatio == 0d && ratio != 0d || previousRatio != 0d && ratio == 0d)
                    result.ErrorZonesXValues.Add(xValue);
                previousRatio = ratio;
                
                double yValue = studentPoint.PressureFactor * 100d;
                studentDataPoints.Add(new DataPoint(xValue, yValue));
                studentIndex++;
            }
            
            // If an error occured at the last point, set the end of the last rectangle here
            if (result.ErrorZonesXValues.Count % 2 != 0) result.ErrorZonesXValues.Add(studentDataPoints.Last().X);
            
            int expertIndex = 0;
            foreach (StylusPoint expertPoint in ExpertTraceUtils.GetAllPoints())
            {
                double xValue = (expertTraceLengths[expertIndex] / biggestTraceLength) * 100;
                double yValue = expertPoint.PressureFactor * 100d;
                expertDataPoints.Add(new DataPoint(xValue, yValue));
                expertIndex++;
            }

            var studentSeries = LineGraph.CreateSeries(studentDataPoints, OxyColors.Blue, "Student Trace");
            result.AddSeries(studentSeries);
            var expertSeries = LineGraph.CreateSeries(expertDataPoints, OxyColors.Red, "Expert Trace");
            result.AddSeries(expertSeries);
            return result;
        }
    }
}