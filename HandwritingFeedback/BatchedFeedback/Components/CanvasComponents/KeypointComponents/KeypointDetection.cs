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

            float keypointThreshold = 5f;

            int maxExpertIndex = ExpertTraceUtils.GetAllPoints().Count - 1;

            Debug.WriteLine("student points: {0}, trace length: {1}", StudentTraceUtils.GetAllPoints().Count, studentTraceLength);
            Debug.WriteLine("expert points: {0}, trace length: {1}", ExpertTraceUtils.GetAllPoints().Count, expertTraceLength);

            int expertIndex = 0;
            double prevYValue = -1;
            var previousValues = new List<double>();
            int mvaSize = 10; //number of datapoints for moving average
            double mvaTolerance = 5; //tolerance threshold = mvaTolerance*std
            List<(double, float)> expertValues = new List<(double, float)> { };
            foreach (StylusPoint expertPoint in ExpertTraceUtils.GetAllPoints())
            {
                double xValue = (expertTraceLengths[expertIndex] / biggestTraceLength) * 100;
                double yValue = expertPoint.PressureFactor * 100d;

                expertValues.Add((xValue, expertPoint.PressureFactor));

                expertDataPoints.Add(new DataPoint(xValue, yValue));

                //kp method 1 threshold:
                float diff = Math.Abs((float)prevYValue - (float)yValue);
                if (prevYValue == -1)
                    diff = 0;

                if (diff > keypointThreshold)
                {
                    Debug.WriteLine("Keypoint Added to error zone x={0} y={1} diff={2} diff_threshold={3}", xValue, yValue, diff, keypointThreshold);
                    result.Keypoints.Add((xValue, yValue));
                }
                //end method 1

                //kp method 2 mva for outlier detection:
                //outlier is > 1 std
                if (previousValues.Count < mvaSize)
                {
                    previousValues.Add(yValue);
                }
                else
                {
                    double std = standardDeviation(previousValues);
                    double avg = previousValues.Average();
                    diff = Math.Abs((float)avg - (float)yValue);
                    if (diff > std * mvaTolerance)
                    {
                        result.DebugKeypoints.Add((xValue, yValue));
                        Debug.WriteLine("DebugKeypoint Added to error zone x={0} y={1} diff={2} avg={3} std={4}", xValue, yValue, diff, avg, std);
                    }

                    previousValues.RemoveAt(0);
                    previousValues.Add(yValue);
                }

                //end method 2

                prevYValue = yValue;
                expertIndex++;                
            }


            int studentIndex = 0;
            foreach (StylusPoint studentPoint in StudentTraceUtils.GetAllPoints())
            {
                double xValue = (studentTraceLengths[studentIndex] / biggestTraceLength) * 100d;

                float studentPressure = studentPoint.PressureFactor;

                double yValue = studentPoint.PressureFactor * 100d;              

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

        static double standardDeviation(IEnumerable<double> sequence)
        {
            double result = 0;

            if (sequence.Any())
            {
                double average = sequence.Average();
                double sum = sequence.Sum(d => Math.Pow(d - average, 2));
                result = Math.Sqrt((sum) / (sequence.Count() - 1));
            }
            return result;
        }
    }
}
