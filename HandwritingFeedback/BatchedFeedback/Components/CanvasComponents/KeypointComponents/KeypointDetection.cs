using System;
using System.Collections.Generic;

using System.Linq;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Util;
using OxyPlot;

using System.Diagnostics;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.KeypointComponents
{
    public class KeypointDetection : BFCanvasComponent
    {
        public KeypointDetection(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils) : base(studentTraceUtils, expertTraceUtils)
        {
            Synthesis = new LineGraph
            {
                Title = "Keypoint detection for 'pressure'.",
                XAxisLabel = "Student Trace Completion (%)",
                YAxisLabel = "Pressure (%)",
                CurvedLine = false
            };
        }

        public override Synthesis Synthesize()
        {
            var result = (LineGraph)Synthesis;

            var studentTraceLengths = new List<double>();
            var expertTraceLengths = new List<double>();
            double studentTraceLength = StudentTraceUtils.CalculateTraceLengths(studentTraceLengths);
            double expertTraceLength = ExpertTraceUtils.CalculateTraceLengths(expertTraceLengths);

            double expertStudentRatio = expertTraceLength / studentTraceLength;

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

            float keypointThreshold = 0.2f;

            int maxExpertIndex = ExpertTraceUtils.GetAllPoints().Count - 1;

            Debug.WriteLine("student points: {0}, trace length: {1}", StudentTraceUtils.GetAllPoints().Count, studentTraceLength);
            Debug.WriteLine("expert points: {0}, trace length: {1}", ExpertTraceUtils.GetAllPoints().Count, expertTraceLength);

            int expertIndex = 0;
            List<(double, float)> expertValues = new List<(double, float)> { };
            foreach (StylusPoint expertPoint in ExpertTraceUtils.GetAllPoints())
            {
                double xValue = (expertTraceLengths[expertIndex] / biggestTraceLength) * 100;
                double yValue = expertPoint.PressureFactor * 100d;

                expertValues.Add((xValue, expertPoint.PressureFactor));

                expertDataPoints.Add(new DataPoint(xValue, yValue));
                expertIndex++;
            }


            int studentIndex = 0;
            foreach (StylusPoint studentPoint in StudentTraceUtils.GetAllPoints())
            {
                double xValue = (studentTraceLengths[studentIndex] / biggestTraceLength) * 100d;

                float studentPressure = studentPoint.PressureFactor;                                   

                double yValue = studentPoint.PressureFactor * 100d;

                float expertYValue = 0;
                for (int j = 0; j < expertValues.Count-1; j++)
                {
                    if(xValue >= expertValues[j].Item1 && xValue < expertValues[j+1].Item1)
                    {
                        expertYValue = expertValues[j].Item2;
                        break;
                    }
                }

     
                float diff = Math.Abs(expertYValue - studentPressure);

                if (diff > keypointThreshold)
                {
                    Debug.WriteLine("Keypoint Added to error zone x={0} y={1} diff={2} diff_threshold={3}", xValue, yValue, diff, keypointThreshold);
                    result.Keypoints.Add((xValue, yValue));
                }

                studentDataPoints.Add(new DataPoint(xValue, yValue));
                studentIndex++;
            }

            // If an error occured at the last point, set the end of the last rectangle here
            if (result.ErrorZonesXValues.Count % 2 != 0) result.ErrorZonesXValues.Add(studentDataPoints.Last().X);


            var studentSeries = LineGraph.CreateSeries(studentDataPoints, OxyColors.Blue, "Student Trace");
            result.AddSeries(studentSeries);
            var expertSeries = LineGraph.CreateSeries(expertDataPoints, OxyColors.Red, "Expert Trace");
            result.AddSeries(expertSeries);
            return result;
        }
    }
}
