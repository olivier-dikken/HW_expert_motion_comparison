using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.BatchedFeedback;
using HandwritingFeedback.Models;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.View
{
    public struct EDMComparisonResult
    {
        public EDMComparisonResult(List<float> val_student, List<float> val_scores, List<float> val_avg, List<float> val_std, string theTitle, string theYlabel)
        {
            Value_student = val_student;
            Value_scores = val_scores;
            Value_avg = val_avg;
            Value_std = val_std;
            title= theTitle;
            ylabel = theYlabel;
        }

        public List<float> Value_student { get; set; }
        public List<float> Value_scores{ get; set; }
        public List<float> Value_avg { get; set; }
        public List<float> Value_std { get; set; }

        public string title { get; }
        public string ylabel { get; }

    }

    /// <summary>
    /// Interaction logic for BatchedAnalytics.xaml
    /// </summary>
    public partial class BatchedAnalytics_EDM : Page
    {
        private readonly TraceUtils _expertTraceUtils;
        private readonly TraceUtils _studentTraceUtils;
        private readonly StrokeCollection _expertOutline;
        BFViewManager_EDM manager;

        /// <summary>
        /// Constructor for batched analytics view.
        /// </summary>
        /// <param name="input">Data transferred from previous page</param>
        public BatchedAnalytics_EDM(BFInputData input, EDMData edmData)
        {
            InitializeComponent();

            // Extract student and expert trace from input
            _expertTraceUtils = input.ExpertTraceUtils;
            _studentTraceUtils = input.StudentTraceUtils;
            _expertOutline = input.ExpertOutline;
            
            inkCanvas.Strokes = _expertOutline.Clone();
            inkCanvas.Strokes.Add(_expertTraceUtils.Trace.Clone());
            inkCanvas.Strokes.Add(_studentTraceUtils.Trace);

            //1- align student points with target trace (to know which EDM points to use)
            List<(int, int)> alignmentVector = getBestPath(_studentTraceUtils.Trace, _expertTraceUtils.Trace);
            //1.2 - show alignment (visually)
            StrokeCollection offsetTargetTrace = OffsetStrokeCollection(_expertTraceUtils.Trace, 200);
            DrawAlignmentLines(_studentTraceUtils.Trace, offsetTargetTrace, alignmentVector);
            inkCanvas.Strokes.Add(offsetTargetTrace);

            //1.3 compute error per datapoint per feature
            List<EDMComparisonResult> comparisonResults = ComputeErrorBasedOnEDM(_studentTraceUtils.Trace, alignmentVector, edmData);

            
            // Populate unit and graphing docks
            input.UnitValueDock = unitValueDock;
            input.GraphDock = graphDock;
            //input.ParametersDock = parametersDock;

            manager = new BFViewManager_EDM(input, comparisonResults);
            manager.PopulateDocks();

            //TODO - evaluate features (visual color code, not in graphs)
        }

        private List<EDMComparisonResult> ComputeErrorBasedOnEDM(StrokeCollection trace, List<(int, int)> aV, EDMData edmData)
        {
            List<EDMComparisonResult> result = new List<EDMComparisonResult> ();
            List<float> x_student = new List<float>();
            List<float> x_scores = new List<float>();
            List<float> x_avg = new List<float>();
            List<float> x_std = new List<float>();

            List<float> y_student = new List<float>();
            List<float> y_scores = new List<float>();
            List<float> y_avg = new List<float>();
            List<float> y_std = new List<float>();

            int totalPoints = 0;
            for(int i = 0; i < trace.Count; i++)
            {
               totalPoints += trace[i].StylusPoints.Count;
            }

            //use alignmentVector to find corresponding edmDataPoints
            List<EDMDataPoint> relevantEDMPoints = new List<EDMDataPoint>();
            int previousStudentMatchIndex = -1;
            EDMDataPoint[] correspondingDataPoints = new EDMDataPoint[totalPoints];
            for(int i = 0; i < aV.Count; i++)
            {
                if(aV[i].Item1 == previousStudentMatchIndex || previousStudentMatchIndex == -1) // student point aligned to several expert points
                    
                {                    
                } else //student index increased by 1
                {
                    //process relevantEDMPoints to correspondingDataPoints, for previous student index
                    if (relevantEDMPoints.Count == 1)
                        correspondingDataPoints[previousStudentMatchIndex] = relevantEDMPoints[0];
                    else
                    {
                        correspondingDataPoints[previousStudentMatchIndex] = edmDataPointAvg(relevantEDMPoints, previousStudentMatchIndex);
                    }
                    relevantEDMPoints.Clear();

                    //compare edmDataPoint values with corresponding student datapoint
                    StylusPoint studentPoint = GetPointFromTraceAt(trace, previousStudentMatchIndex);
                    float score_x = GetEDMScore(studentPoint, correspondingDataPoints[previousStudentMatchIndex], "X", 1);
                    float score_y = GetEDMScore(studentPoint, correspondingDataPoints[previousStudentMatchIndex], "Y", 1);

                    x_student.Add((float)studentPoint.X);
                    x_scores.Add(score_x);
                    x_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].X);
                    x_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].X_std);

                    y_student.Add((float)studentPoint.Y);
                    y_scores.Add(score_y);
                    y_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].Y);
                    y_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].Y_std);

                    //print in console
                    Debug.WriteLine("Student point " + previousStudentMatchIndex + " comparison with EDM point. X diff: " + score_x + " Y diff: " + score_y);
                }
                relevantEDMPoints.Add(edmData.dataPoints[aV[i].Item2]);                

                previousStudentMatchIndex = aV[i].Item1;
            }
            EDMComparisonResult x_comparison = new EDMComparisonResult(x_student, x_scores, x_avg, x_std, "X of student vs EDM", "X position");
            result.Add(x_comparison);
            EDMComparisonResult y_comparison = new EDMComparisonResult(y_student, y_scores, y_avg, y_std, "Y of student vs EDM", "Y position");
            result.Add(y_comparison);
            return result;
        }

        private float GetEDMScore(StylusPoint studentPoint, EDMDataPoint edmPoint, String feature, float scale)
        {
            switch (feature)
            {
                case "X":
                    return (float)((studentPoint.X - edmPoint.X) / (edmPoint.X_std * scale));
                    break;

                case "Y":
                    return (float)((studentPoint.Y - edmPoint.Y) / (edmPoint.Y_std * scale));
                    break;

                default:
                    Debug.WriteLine("Feature not found: " + feature);
                    break;
            }

            return 0;
        }

        private EDMDataPoint edmDataPointAvg(List<EDMDataPoint> relevantEDMPoints, int idx)
        {
            EDMDataPoint dataPoint = new EDMDataPoint(idx);
            double X = 0;
            double X_std = 0;
            double Y = 0;
            double Y_std = 0;
            double pressure = 0;
            double speed = 0;

            //take avg of each property
            for (int i = 0; i < relevantEDMPoints.Count; i++)
            {
                X += relevantEDMPoints[i].X / relevantEDMPoints.Count;
                X_std += relevantEDMPoints[i].X_std / relevantEDMPoints.Count;
                Y += relevantEDMPoints[i].Y / relevantEDMPoints.Count;
                Y_std += relevantEDMPoints[i].Y_std / relevantEDMPoints.Count;
                pressure += relevantEDMPoints[i].pressure / relevantEDMPoints.Count;
                speed += relevantEDMPoints[i].speed / relevantEDMPoints.Count;
            }
            dataPoint.X = X;
            dataPoint.Y = Y;
            dataPoint.X_std = X_std;
            dataPoint.Y_std = Y_std;
            dataPoint.pressure = pressure;
            dataPoint.speed = speed;
            return dataPoint;            
        }

        public StrokeCollection OffsetStrokeCollection(StrokeCollection toOffset, int yOffset)
        {
            StrokeCollection offsetCollection = new StrokeCollection();

            for (int i = 0; i < toOffset.Count; i++)
            {
                StylusPointDescription spd = toOffset[i].StylusPoints.Description;
                StylusPointCollection offsetPoints = new StylusPointCollection(spd);
                for (int j = 0; j < toOffset[i].StylusPoints.Count; j++)
                {
                    StylusPoint newPoint = toOffset[i].StylusPoints[j];

                    newPoint.Y += yOffset;
                    offsetPoints.Add(newPoint);
                }
                Stroke newStroke = new Stroke(offsetPoints);
                offsetCollection.Add(newStroke);
            }
            return offsetCollection;
        }

        public List<(int, int)> getBestPath(StrokeCollection scExpert, StrokeCollection scStudent)
        {
            List<(float, float)> coordsE = Alignment.TraceToCoords(scExpert);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);
            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsS);

            return Alignment.TestBestPath(coordDistances);
        }

        public void DrawAlignmentLines(StrokeCollection eStrokes, StrokeCollection offsetStrokes, List<(int, int)> aV)
        {
            int idx = 0;
            foreach ((int, int) match in aV)
            {
                StylusPoint spRef = GetPointFromTraceAt(eStrokes, match.Item1);
                StylusPoint spCor = GetPointFromTraceAt(offsetStrokes, match.Item2);
                Stroke mappingLine = MakeLine(spRef.X, spRef.Y, spCor.X, spCor.Y, (float)idx / (float)aV.Count);
                inkCanvas.Strokes.Add(mappingLine);
                idx++;
            }
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

            Debug.WriteLine("ERROR no styluspoint found in GetPointFromTraceAt");
            return new StylusPoint();
        }

        static Stroke MakeLine(double x1, double y1, double x2, double y2, float progress)
        {
            StylusPoint sp_1 = new StylusPoint(x1, y1);
            StylusPoint sp_2 = new StylusPoint(x2, y2);

            StylusPointCollection spCol = new StylusPointCollection();
            spCol.Add(sp_1);
            spCol.Add(sp_2);

            Stroke stroke = new Stroke(spCol);

            Color color = Colors.Green;
            color.ScA = 0.2f;
            color.ScG = 1 - progress;
            color.ScR = progress;
            stroke.DrawingAttributes.Color = color;
            return stroke;
        }

        /// <summary>
        /// Navigates to the page indicated by the button tag.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }
        
        /// <summary>
        /// Opens file explorer to save student trace.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void SaveStudentTrace(object sender, RoutedEventArgs e)
        {
            var result = new InkCanvas { Strokes = _studentTraceUtils.Trace };
            FileHandler.ButtonSaveAsClick(sender, e, result);
        }


        /// <summary>
        /// Restarts exercise by loading the previous expert model in practice mode,
        /// and navigates to the practice mode.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void Restart(object sender, RoutedEventArgs e)
        {
            // Add expert trace to be sent to practice mode
            var inputData = new BFInputData
            {
                ExpertTraceUtils = _expertTraceUtils,
                ExpertOutline = _expertOutline
            };

            // Navigate to next page and send package
            var destination = new PracticeMode(inputData); //TODO load EDM data
            NavigationService.Navigate(destination);
        }

        private void SaveAllBatchedFeedback(object sender, RoutedEventArgs e)
        {
            FileHandler.ButtonSaveAsImageClick(sender, e, MainGrid);
        }
    }
}
