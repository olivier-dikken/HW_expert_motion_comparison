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

        int windowSize = 50;
        int thresholdStd = 5;
        int minimumDistanceHalf = 30;

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

        public List<(string, int)> GetGUIParameters()
        {
            List<(string, int)> paramKeyValue = new List<(String, int)>();

            paramKeyValue.Add(("Window Size", windowSize));
            paramKeyValue.Add(("Threshold Multiple of STD", thresholdStd));
            paramKeyValue.Add(("Minimum Distance Half", minimumDistanceHalf));

            return paramKeyValue;
        }

        public void UpdateParameters(List<(string, int)> newValues)
        {
            foreach((string, int) keyVal in newValues)
            {
                switch (keyVal.Item1)
                {
                    case "Window Size":
                        windowSize = keyVal.Item2;
                        break;
                    case "Threshold Multiple of STD":
                        thresholdStd = keyVal.Item2;
                        break;
                    case "Minimum Distance Half":
                        minimumDistanceHalf = keyVal.Item2;
                        break;
                    default:
                        Debug.WriteLine("Parameter key not found: {0}", keyVal.Item1);
                        break;
                }
            }
        }

        public Synthesis SynthesizeNew(string newTitle)
        {
            Synthesis = new LineGraph
            {
                Title = "Keypoint detection for 'pressure'.",
                XAxisLabel = "Student Trace Completion (%)",
                YAxisLabel = "Pressure (%)",
                CurvedLine = false
            };
            return Synthesize();
        }

        public override Synthesis Synthesize()
        {
            var result = (LineGraph)Synthesis;

            var expertTraceLengths = new List<double>();
            double expertTraceLength = ExpertTraceUtils.CalculateTraceLengths(expertTraceLengths);


            
            result.XAxisLabel = "Expert Trace DataPoint Index";
                

            var expertDataPoints = new List<DataPoint>();

            //derivte datapoints extra feature
            List<(double, double)> xyValues = new List<(double, double)>();
            List<(double, double)> pressureValues = new List<(double, double)>();
            var derivativeDataPoints = new List<DataPoint>();

            int expertIndex = 0;
            List<(double, float)> expertValues = new List<(double, float)> { };
            foreach (StylusPoint expertPoint in ExpertTraceUtils.GetAllPoints())
            {
                double xValue = expertTraceLengths[expertIndex];
                double yValue = expertPoint.PressureFactor * 100d;

                expertValues.Add((xValue, expertPoint.PressureFactor));                

                expertDataPoints.Add(new DataPoint(xValue, yValue));

                xyValues.Add((expertPoint.X, expertPoint.Y));
                pressureValues.Add((xValue, yValue));
                
                expertIndex++;
            }

            //get slope
            List<double> slope = FeatureUtils.getSlope(pressureValues);
            var slopePoints = new List<DataPoint>();
            expertIndex=0;
            foreach (double angle in slope)
            {
                double xValue = expertTraceLengths[expertIndex];
                slopePoints.Add(new DataPoint(xValue, 50+angle));
                expertIndex++;
            }
            var expertDeltaSeries = LineGraph.CreateSeries(slopePoints, OxyColors.Blue, "Expert Trace Derivative");
            result.AddSeries(expertDeltaSeries);

            //result.Keypoints = extractKeypoints_basic(expertValues);
            result.DebugKeypoints = extractKeypoints_mva(expertValues, windowSize, thresholdStd, minimumDistanceHalf);

            var expertSeries = LineGraph.CreateSeries(expertDataPoints, OxyColors.Red, "Expert Trace");
            result.AddSeries(expertSeries);
            return result;
        }

        public void DifferenceOfGaussians()
        {
            //TODO https://hal.archives-ouvertes.fr/hal-01252726/file/dense-bag-temporal.pdf
        }


        public List<(double, double)> getKeypointsCanvasLocation()
        {
            
            var expertTraceLengths = new List<double>();
            double expertTraceLength = ExpertTraceUtils.CalculateTraceLengths(expertTraceLengths);

            int expertIndex = 0;
            List<(double, float)> expertValues = new List<(double, float)> { };
            foreach (StylusPoint expertPoint in ExpertTraceUtils.GetAllPoints())
            {
                double xValue = expertTraceLengths[expertIndex];
                expertValues.Add((xValue, expertPoint.PressureFactor));
                expertIndex++;                
            }

            List <(double, double)> keypoints = extractKeypoints_mva(expertValues, windowSize, thresholdStd, minimumDistanceHalf);

            List<(double, double)> realLocations = new List<(double, double)>();
            //convert keypoints to canvas x,y locations:
            foreach((double, double) kp in keypoints)
            {
                int index = (int)Math.Min(Math.Round((ExpertTraceUtils.GetAllPoints().Count / expertTraceLength) * kp.Item1), ExpertTraceUtils.GetAllPoints().Count-1);
                StylusPoint point = ExpertTraceUtils.GetAllPoints().ElementAt(index);
                realLocations.Add((point.X, point.Y));
            }
            return realLocations;
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

        List<(double, double)> extractKeypoints_basic(List<(double, float)> expertValues)
        {
            List<(double, double)> foundKeypoints = new List<(double, double)> { };

            float keypointThreshold = 0.15f; // = 20% change in subsequent datapoint; should look at angle/derivative so that it doesn't depend on sample rate

            int expertIndex = 0;
            double prevYValue = -1;
            foreach ((double xValue, float yValue) in expertValues)
            {

                //kp method 1 threshold:
                float diff = Math.Abs((float)prevYValue - (float)yValue);
                if (prevYValue == -1)
                    diff = 0;

                if (diff > keypointThreshold)
                {
                    Debug.WriteLine("Keypoint Added to error zone x={0} y={1} diff={2} diff_threshold={3}", xValue, yValue, diff, keypointThreshold);
                    foundKeypoints.Add((xValue, yValue));
                }
                //end method 1

                prevYValue = yValue;
                expertIndex++;
            }

            return foundKeypoints;
        }

        List<(double, double)> extractKeypoints_mva(List<(double, float)> expertValues, int windowSize, double thresholdStd, int minimumDistanceHalf)
        {
            List<(double, double, double)> pointDifferenceScores = new List<(double, double, double)> { }; //format: xValue, yValue, differenceScore
            List<(double, double)> foundKeypoints = new List<(double, double)> { };


            int expertIndex = 0;
            var previousValues = new List<double>();
            int mvaSize = windowSize; //number of datapoints for moving average
            double mvaTolerance = thresholdStd; //tolerance threshold = mvaTolerance*std
            foreach ((double xValue, float yValue) in expertValues)
            {
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
                    double diff = Math.Abs((float)avg - (float)yValue) - std * mvaTolerance;
                    //if (diff > 0)
                    //{
                    //    foundKeypoints.Add((xValue, yValue));
                    //    Debug.WriteLine("DebugKeypoint Added to error zone x={0} y={1} diff={2} avg={3} std={4}", xValue, yValue, diff, avg, std);
                    //}

                    pointDifferenceScores.Add((xValue, yValue, Math.Max(diff, 0)));
                    

                    previousValues.RemoveAt(0);
                    previousValues.Add(yValue);
                }

                //end method 2

                expertIndex++;
            }

            //sliding window over pointDifferenceScores to select biggest outliers as keypoints
            //nested loop (slow) approach
            int window_size_half = minimumDistanceHalf;
            double xPoint;
            double yPoint;
            double diffPoint;
            double xCompare;
            double yCompare;
            double diffCompare;
            bool noBetter = false;
            if (pointDifferenceScores.Count > window_size_half*2)
            {
                for (int i = 0; i < pointDifferenceScores.Count - 1; i++)
                {
                    (xPoint, yPoint, diffPoint) = pointDifferenceScores.ElementAt(i);

                    if(diffPoint > 0) //check if this potential KP has no better KP nearby
                    {
                        noBetter = true;
                        for (int j = Math.Max(0, i - window_size_half); j < Math.Min(pointDifferenceScores.Count - 1, i + window_size_half); j++)
                        {
                            (xCompare, yCompare, diffCompare) = pointDifferenceScores.ElementAt(j);
                            if(diffCompare > diffPoint) //if Point is not biggest outlier then move on
                            {
                                noBetter = false;
                                break;
                            }
                        }
                        if (noBetter) //best potential KP in window
                        {
                            foundKeypoints.Add((xPoint, yPoint));
                            i = i + window_size_half - 1;//all points within window_size_half are less than the foundKeyPoint
                        }
                    }


                }
            } else
            {
                Debug.WriteLine("Series has less datapoints than window size");
            }

            return foundKeypoints;
        }
    }
}
