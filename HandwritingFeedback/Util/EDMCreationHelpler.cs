using HandwritingFeedback.BatchedFeedback;
using HandwritingFeedback.Config;
using HandwritingFeedback.InkCanvases;
using HandwritingFeedback.Models;
using HandwritingFeedback.View.UpdatedUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static HandwritingFeedback.Util.EDMCreationHelpler.EDMModelComparison;

namespace HandwritingFeedback.Util
{
    public class EDMCreationHelpler
    {
        private Page createEDMView;

        private ExpertInkCanvas bgCanvas;
        private ExpertInkCanvas editCanvas;
        private Canvas overlayCanvas;

        private ExerciseItem theExercise;
        private ExerciseData theExerciseData;
        private StrokeCollection _targetTrace;

        private ExpertDistributionModel currentEDM;
        private ExpertDistributionModel newSampleEDM;
        
        private List<StrokeCollection> performances;
        private int iteration;

        private Dictionary<string, double[]> previousSampleTransformedData;
        

        public EDMCreationHelpler(ExerciseData theExercise, ExpertInkCanvas bgC, ExpertInkCanvas editC, Canvas overlayC, Page createEDMView)
        {
            this.theExerciseData = theExercise;
            //this.theExercise = theExercise;
            this.bgCanvas = bgC;
            this.editCanvas = editC;
            this.overlayCanvas = overlayC;
            this.createEDMView = createEDMView;
            Setup();
        }

        private void Setup()
        {
            iteration = 0;
            this._targetTrace = theExercise.targetTrace;
            TraceUtils tu = new TraceUtils(_targetTrace);
            int ttNumberOfStylusPoints = tu.GetNumberOfStylusPoints();            
            currentEDM = new ExpertDistributionModel(ttNumberOfStylusPoints);            
            newSampleEDM = new ExpertDistributionModel(ttNumberOfStylusPoints);
            performances = new List<StrokeCollection>();
            previousSampleTransformedData = new Dictionary<string, double[]>();
            foreach(string ft in GlobalState.FeatureNames)
            {
                previousSampleTransformedData[ft] = new double[ttNumberOfStylusPoints];
            }
            ReloadCanvas();
        }

        private void SaveExercise()
        {

            EDMData edmData = currentEDM.GetEDMData();
            string fileName = "EDMData";
            ExpertDistributionModel.SaveToFile(GlobalState.CreateContentsPreviousFolder + "\\" + fileName, edmData);

            //TODO save strokecollection to file
 
            //return to menu & show success message
            
        }


        /// <summary>
        /// after creating a new sample trace, submit the trace to view distribution contribution
        /// </summary>
        public void SubmitNewSample()
        {
            //collect strokes
            StrokeCollection submittedPerformance = editCanvas.Strokes.Clone();
            performances.Add(submittedPerformance);
            //add to newSampleEDM
            AddSampleToEDM(newSampleEDM, submittedPerformance);

            if (iteration > 0) //dont compare edm models on first iteration, because there is only 1 model and no other model to compare to
            {
                //compare to currentEDM
                EDMModelComparison edmModelComparison = new EDMModelComparison(currentEDM, newSampleEDM);
                //TODO show vis
                ShowEDMModelComparisonDiffZonesOnTrace(edmModelComparison);
            }


        }

        /// <summary>
        /// highlight the x,y coords with the biggest EDMModelComparison diff in avg and/or std 
        /// </summary>
        private void ShowEDMModelComparisonDiffZonesOnTrace(EDMModelComparison edmModelComparison)
        {
            SolidColorBrush sb;
            overlayCanvas.Children.RemoveRange(0, overlayCanvas.Children.Count);
            overlayCanvas.Visibility = System.Windows.Visibility.Visible; //show overlay canvas
            Dictionary<string, EDMModelComparisonValue[]> edmMCData = edmModelComparison.GetModelComparisonData();
            foreach (string ft in GlobalState.FeatureNames)
            {
                if (ft == "Pressure")
                {
                    sb = new SolidColorBrush(Colors.Blue);
                    EDMModelComparisonValue[] pressureData = edmMCData[ft];
                    for (int i = 0; i < edmModelComparison.length; i++)
                    {
                        double diffFormulaValue = Math.Abs((pressureData[i].meanOld - previousSampleTransformedData[ft][i]))/2;
                        Debug.WriteLine("Pressure diffFormulaValue: " + diffFormulaValue);
                        if(diffFormulaValue > 0.1) //if big diff in STD then draw overlay point
                        {                            
                            double x = previousSampleTransformedData["X"][i];
                            double y = previousSampleTransformedData["Y"][i];
                            OverlayDrawDatapoint(x, y, sb, ellipseDiameter: 7);
                        }
                    }
                }
                else if (ft == "Curvature")
                {
                    sb = new SolidColorBrush(Colors.Red);
                    EDMModelComparisonValue[] curvatureData = edmMCData[ft];
                    for (int i = 0; i < edmModelComparison.length; i++)
                    {
                        double meanCurv = curvatureData[i].meanOld;
                        double newCurv = previousSampleTransformedData[ft][i];
                        if (curvatureData[i].meanOld > 300 && curvatureData[i].meanOld < 60)
                        {
                            meanCurv += 180;
                            newCurv += 180;

                            meanCurv = meanCurv % 360;
                            newCurv = newCurv % 360;

                        }
                        double diffFormulaValue = Math.Abs((meanCurv - newCurv)) / 200;
                        Debug.WriteLine("Curvature data meanCurv: " + meanCurv + " newCurv: " + newCurv);
                        Debug.WriteLine("Curvature diffFormulaValue: " + diffFormulaValue);
                        if (diffFormulaValue > 0.1) //if big diff in STD then draw overlay point
                        {
                            double x = previousSampleTransformedData["X"][i];
                            double y = previousSampleTransformedData["Y"][i];
                            OverlayDrawDatapoint(x, y, sb, ellipseDiameter: 12);
                        }
                    }
                }
                //else if (ft == "Speed")
                //{
                //    sb = new SolidColorBrush(Colors.Orange);
                //    EDMModelComparisonValue[] speedData = edmMCData[ft];
                //    for (int i = 0; i < edmModelComparison.length; i++)
                //    {
                //        double diffFormulaValue = Math.Abs((speedData[i].avgRawDiff - previousSampleTransformedData[ft][i])) / 25;
                //        Debug.WriteLine("Speed diffFormulaValue: " + diffFormulaValue);
                //        if (diffFormulaValue > 0.2) //if big diff in STD then draw overlay point
                //        {
                //            double x = previousSampleTransformedData["X"][i];
                //            double y = previousSampleTransformedData["Y"][i];
                //            OverlayDrawDatapoint(x, y, sb, ellipseDiameter: 7);
                //        }
                //    }
                //}
                else //onlylimited ft for demo
                    continue;


                //remember which dots belong to which ft?
            }            
        }

        void OverlayDrawDatapoint(double X, double Y, SolidColorBrush strokeColor, int ellipseDiameter = 8)
        {

            

            Ellipse ellipse = new Ellipse();
            ellipse.StrokeThickness = 1;
            //ellipse.Fill = new SolidColorBrush(Color.FromRgb(255, 240, 31));
            ellipse.Fill = strokeColor;
            ellipse.Stroke = strokeColor;
           // ellipse.Stroke = new SolidColorBrush(Color.FromRgb(0, 32, 255));
            ellipse.Width = ellipseDiameter;
            ellipse.Height = ellipseDiameter;

            Canvas.SetLeft(ellipse, X - (ellipseDiameter / 2));
            Canvas.SetTop(ellipse, Y - (ellipseDiameter / 2));

            overlayCanvas.Children.Add(ellipse);
        }




        public class EDMModelComparison
        {
            Dictionary<string, EDMModelComparisonValue[]> edmModelComparisonData;
            public int length { get; }

            public EDMModelComparison(ExpertDistributionModel edm1, ExpertDistributionModel edm2)
            {
                length = edm1.length;
                edmModelComparisonData = new Dictionary<string, EDMModelComparisonValue[]>();
                foreach (string ft in GlobalState.FeatureNames)
                {
                    edmModelComparisonData[ft] = new EDMModelComparisonValue[edm1.length];
                }
                CompareEDMData(edm1.GetEDMData(), edm2.GetEDMData());
            }

            public Dictionary<string, EDMModelComparisonValue[]> GetModelComparisonData()
            {
                return edmModelComparisonData;
            }

            /// <summary>
            /// get diff in std and avg for all index of each feature
            /// </summary>
            /// <param name="edm1"></param>
            /// <param name="edm2"></param>
            private void CompareEDMData(EDMData edmD1, EDMData edmD2)
            {
                foreach(string ft in GlobalState.FeatureNames)
                {
                    for (int i = 0; i < edmD1.GetLength(); i++)
                    {
                        //edmModelComparisonData[ft][i] = new EDMModelComparisonValue(edmD1.GetData()[ft][i].mean, edmD2.GetData()[ft][i].mean, edmD1.GetData()[ft][i].GetSTD(), edmD2.GetData()[ft][i].GetSTD());
                        edmModelComparisonData[ft][i] = new EDMModelComparisonValue(edmD1.GetFeatureMean(i,ft), edmD2.GetFeatureMean(i, ft), edmD1.GetFeatureStandardDeviation(i,ft), edmD2.GetFeatureStandardDeviation(i, ft));
                    }
                }
            }

            public class EDMModelComparisonValue
            {
                public double avgRawDiff;
                public double stdRawDiff;
                public double avgNormDiff;
                public double stdNormDiff;

                public double meanOld;
                public double stdOld;

                public EDMModelComparisonValue(double mean1, double mean2, double std1, double std2)
                {
                    meanOld = mean1;
                    stdOld = std1;
                    avgRawDiff = mean1 - mean2;
                    stdRawDiff = std1 - std2;
                    avgNormDiff = mean1 - mean2;//TODO WRONG FORUMLA FOR NORM USE MIN/MAX values
                    stdNormDiff = std1 - std2;
                }
            }
        }


        /// <summary>
        /// after sample submission, the contribution to the EDM is visualized, at which point the expert can confirm to add this sample to the EDM
        /// </summary>
        public void ConfirmNewSample()
        {
            iteration++;

            currentEDM = newSampleEDM.DeepCopy();

            //reload canvas to be ready for next sample
            ReloadCanvas();

            if(iteration == 5)
            {
                //save and go to main menu
                SaveExercise();

                Button fakeButton = new Button();
                fakeButton.Tag = "\\View\\UpdatedUI\\MainMenu.xaml";
                CommonUtils.Navigate(fakeButton, null, createEDMView);
            }
        }

        public void DiscardNewSample()
        {
            //TODO attach to a discard sample button
            performances.RemoveAt(performances.Count-1);
            newSampleEDM = currentEDM.DeepCopy();//rm new sample from newSampleEDM
            ReloadCanvas();
        }


        /// <summary>
        /// clear canvas and load exercise target trace
        /// </summary>
        public void ReloadCanvas()
        {
            overlayCanvas.Visibility = System.Windows.Visibility.Collapsed; //hide overlay canvas
            bgCanvas.Reset();
            editCanvas.Reset();
            //load outline trace
            LoadTargetTrace();
            //helper lines
            TraceUtils.DrawHelperLines(bgCanvas, theExercise.lineType, theExercise.lineSpacing);
        }


        private void LoadTargetTrace()
        {
            TraceUtils ExpertTraceUtils = new TraceUtils(this._targetTrace);

            // Modify the color of the expert's trace
            // This will allow the expert's trace to be darker than
            // the outline.
            foreach (var stroke in ExpertTraceUtils.Trace)
            {
                stroke.DrawingAttributes.Color = Color.FromRgb(200, 200, 200);
            }      

            // Add the expert's trace on top of the outline
            bgCanvas.Strokes.Add(ExpertTraceUtils.Trace);
        }


        /// <summary>
        /// Add all features of sample to EDM. Make sure all newSample data is aligned to the _targetTrace, with as many datapoints as _targetTrace.
        /// </summary>
        /// <param name="edm"></param>
        /// <param name="newSample"></param>
        private void AddSampleToEDM(ExpertDistributionModel edm, StrokeCollection newSample)
        {
            //store expertPerformance of previous iteration
            Dictionary<string, double[]> add_data = new Dictionary<string, double[]>();
            foreach (string featureName in GlobalState.FeatureNames)
            {
                double[] transformed = TransformStrokeDataToTarget(_targetTrace, newSample, featureName);
                add_data[featureName] = transformed;
                previousSampleTransformedData[featureName] = transformed;
            }

            edm.AddTransformed(add_data);
        }

        /// <summary>
        /// DTW best alignment between the two strokecollections
        /// </summary>
        /// <param name="scExpert"></param>
        /// <param name="scStudent"></param>
        /// <returns></returns>
        public List<(int, int)> getBestPath(StrokeCollection scExpert, StrokeCollection scStudent)
        {
            List<(float, float)> coordsE = Alignment.TraceToCoords(scExpert);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);
            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsS);

            return Alignment.TestBestPath(coordDistances);
        }

        StylusPoint GetPointFromTraceAt(StrokeCollection trace, int idx)
        {            
            int counter = 0;
            foreach (Stroke stroke in trace)
            {
                foreach (StylusPoint sp in stroke.StylusPoints)
                {
                    if (counter == idx)
                        return sp;
                    counter++;
                }
            }

            Debug.WriteLine("ERROR no styluspoint found in GetPointFromTraceAt with idx: " + idx);
            return new StylusPoint();
        }


        /// <summary>
        /// So the toTransform ends up with the same amount of datapoints as the target, with indexes corresponding to aligned points between the target and toTransform
        /// </summary>
        /// <param name="target"></param>
        /// <param name="toTransform"></param>
        private double[] TransformStrokeDataToTarget(StrokeCollection target, StrokeCollection toTransform, string featureName)
        {
            List<(int, int)> alignmentVector = getBestPath(target, toTransform);
            TraceUtils tu = new TraceUtils(_targetTrace);
            int targetLength = tu.GetNumberOfStylusPoints();
            double[] transformed = new double[targetLength];

            double nextVals;
            double amount;
            for (int i = 0; i < targetLength; i++)
            {
                nextVals = 0;
                amount = 0;
                for (int j = 0; j < alignmentVector.Count; j++)
                {
                    if (alignmentVector[j].Item1 == i)
                    {
                        StylusPoint currentPoint = GetPointFromTraceAt(toTransform, alignmentVector[j].Item2);
                        StylusPoint previousPoint;
                        double toAdd = 0;
                        switch (featureName)
                        {
                            case "X":
                                toAdd = currentPoint.X;
                                break;

                            case "Y":
                                toAdd = currentPoint.Y;
                                break;

                            case "Curvature":
                                if (alignmentVector[j].Item2 == 0)
                                {
                                    toAdd = 0;
                                }
                                else
                                {
                                    previousPoint = GetPointFromTraceAt(toTransform, (alignmentVector[j].Item2 - 1));
                                    float x_diff = (float)(currentPoint.X - previousPoint.X);
                                    float y_diff = (float)(currentPoint.Y - previousPoint.Y);                                    
                                    float rad = MathF.Atan2(y_diff, x_diff);
                                    var deg = rad * (180 / Math.PI);
                                    toAdd = deg;
                                }
                                break;

                            case "Pressure":
                                toAdd = currentPoint.PressureFactor;
                                break;

                            case "Speed":
                                previousPoint = GetPointFromTraceAt(toTransform, (alignmentVector[j].Item2 - 1));
                                float diagonalLength = MathF.Sqrt(MathF.Pow((float)(currentPoint.X - previousPoint.X), 2) + MathF.Pow((float)(currentPoint.Y - previousPoint.Y), 2));
                                toAdd = diagonalLength; //TODO divide by time passed!                                
                                break;

                            default:
                                Debug.WriteLine($"unrecognized feature name: {featureName} in TransformStrokeDataToTarget");
                                return null;

                        }

                        nextVals += toAdd;
                        amount += 1;
                    }
                }
                transformed[i] = nextVals / amount;
            }
            return transformed;
        }


    }


}
