using HandwritingFeedback.Config;
using HandwritingFeedback.InkCanvases;
using HandwritingFeedback.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace HandwritingFeedback.Util
{
    public class EDMCreationHelpler
    {
        private ExpertInkCanvas bgCanvas;
        private ExpertInkCanvas editCanvas;

        private ExerciseItem theExercise;
        private StrokeCollection _targetTrace;

        private ExpertDistributionModel currentEDM;
        private ExpertDistributionModel newSampleEDM;
        
        private List<StrokeCollection> performances;
        private int iteration;

        public EDMCreationHelpler(ExerciseItem theExercise, ExpertInkCanvas bgC, ExpertInkCanvas editC)
        {
            this.theExercise = theExercise;
            this.bgCanvas = bgC;
            this.editCanvas = editC;
            Setup();
        }

        private void Setup()
        {
            iteration = 0;
            this._targetTrace = theExercise.targetTrace;
            TraceUtils tu = new TraceUtils(_targetTrace);
            int ttNumberOfStylusPoints = tu.GetNumberOfStylusPoints();
            currentEDM = new ExpertDistributionModel(ttNumberOfStylusPoints);
            newSampleEDM = new ExpertDistributionModel(_targetTrace.Count);
            performances = new List<StrokeCollection>();
            ReloadCanvas();
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
            //TODO compare to currentEDM
            //TODO show vis

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
        }

        public void DiscardNewSample()
        {
            //TODO attach to a discard sample button
            performances.RemoveAt(performances.Count);
            newSampleEDM = currentEDM.DeepCopy();//rm new sample from newSampleEDM
            ReloadCanvas();
        }


        /// <summary>
        /// clear canvas and load exercise target trace
        /// </summary>
        public void ReloadCanvas()
        {
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
            foreach (string featureName in GlobalState.FeatureNames)
            {
                double[] transformed = TransformStrokeDataToTarget(_targetTrace, newSample, featureName);
                edm.AddTransformed(transformed, featureName);
            }
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
                                    toAdd = MathF.Atan2(y_diff, x_diff);
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
