using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Models;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;
using OxyPlot;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents
{
    /// <summary>
    /// Computes the student's accuracy compared to the expert model at regular time intervals.
    /// </summary>
    public class FeatureOverProgress_EDM : BFCanvasComponent
    {
        EDMComparisonResult _comparisonResult;
        /// <summary>
        /// Constructor to initialize a component to compute accuracy over time after an exercise.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at an exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        public FeatureOverProgress_EDM(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils, EDMComparisonResult comparisonResult, string title, string ylabel) : base(studentTraceUtils,
            expertTraceUtils)
        {
            // Configure properties of line graph visualization
            this.Synthesis = new LineGraph
            {
                Title = title,
                XAxisLabel = "Student point index",
                YAxisLabel = ylabel,
                CurvedLine = false
            };
            _comparisonResult = comparisonResult;
        }

        /// <summary>
        /// Generates stream of x-y coordinates to be used to plot accuracy of student's trace compared to the expert model over trace progress.
        /// </summary>
        /// <returns>A Synthesis object which contains the information to construct a LineGraph for accuracy over progress</returns>
        public override Synthesis Synthesize()
        {
            // Data points to be added to the graph, starting at (0,0).
            List<DataPoint> dataPoints_student = new List<DataPoint>();
            dataPoints_student.Add(new DataPoint(0, 0));
            List<DataPoint> dataPoints_edm_avg = new List<DataPoint>();
            dataPoints_edm_avg.Add(new DataPoint(0, 0));
            List<DataPoint> dataPoints_edm_std_upper = new List<DataPoint>();
            dataPoints_edm_std_upper.Add(new DataPoint(0, 0));
            List<DataPoint> dataPoints_edm_std_lower = new List<DataPoint>();
            dataPoints_edm_std_lower.Add(new DataPoint(0, 0));

            LineGraph result = (LineGraph) this.Synthesis;
            
            
            for (int i = 0; i < _comparisonResult.Value_student.Count; i++)
            {                
                dataPoints_student.Add(new DataPoint(i, _comparisonResult.Value_student[i]));
                dataPoints_edm_avg.Add(new DataPoint(i, _comparisonResult.Value_avg[i]));
                dataPoints_edm_std_upper.Add(new DataPoint(i, _comparisonResult.Value_avg[i] + _comparisonResult.Value_std[i]));
                dataPoints_edm_std_lower.Add(new DataPoint(i, _comparisonResult.Value_avg[i] - _comparisonResult.Value_std[i]));
            }            

            // Add line in graph for student data points
            var studentSeries = LineGraph.CreateSeries(dataPoints_student, OxyColors.Blue, "Student Trace");
            result.AddSeries(studentSeries);
            
            var expertSeries = LineGraph.CreateSeries(dataPoints_edm_avg, OxyColors.Red, "Expert Trace");
            result.AddSeries(expertSeries);
            
            var expertSeries_std_upper = LineGraph.CreateSeries(dataPoints_edm_std_upper, OxyColors.Orange, "Expert Trace std upper");
            result.AddSeries(expertSeries_std_upper);
            var expertSeries_std_lower = LineGraph.CreateSeries(dataPoints_edm_std_lower, OxyColors.Orange, "Expert Trace std lower");
            result.AddSeries(expertSeries_std_lower);

            return result;
        }

    }
}