using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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

        private StrokeCollection _offsetTargetTrace;
        private List<(int, int)> _alignmentVector;
        private EDMData _edmData;

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

            _edmData = edmData;
            //1- align student points with target trace (to know which EDM points to use)
            _alignmentVector = getBestPath(_studentTraceUtils.Trace, _expertTraceUtils.Trace);
            //1.2 - show alignment (visually)
            _offsetTargetTrace = OffsetStrokeCollection(_expertTraceUtils.Trace, 200);
            //DrawAlignmentLines(_studentTraceUtils.Trace, offsetTargetTrace, alignmentVector);
            inkCanvas.Strokes.Add(_offsetTargetTrace);

            //1.3 compute error per datapoint per feature
            List<EDMComparisonResult> comparisonResults = ComputeErrorBasedOnEDM(_studentTraceUtils.Trace, _alignmentVector, _edmData);


            // Populate unit and graphing docks
            input.UnitValueDock = unitValueDock;
            input.GraphDock = graphDock;
            //input.ParametersDock = parametersDock;

            manager = new BFViewManager_EDM(input, comparisonResults, overlayCanvas, this);
            manager.PopulateDocks();

             
            CenterScrollViewOnTrace(inkCanvas.Strokes);
        }

        public void OverlaySelectDatapoint(int studentIndex)
        {
            //clear current selection
            overlayCanvas.Children.RemoveRange(0, overlayCanvas.Children.Count);

            StylusPoint sp = GetPointFromTraceAt(_studentTraceUtils.Trace, studentIndex);           

            //draw alignment lign to corresponding expert dp
            DrawNearAlignmentLinesForDatapoint(_studentTraceUtils.Trace, _offsetTargetTrace, _alignmentVector, studentIndex);

            //draw ellipse on student dp
            OverlayDrawDatapoint(sp.X, sp.Y);

            manager.UpdateSelectedPoint(studentIndex);
        }

        public void OverlaySelectDatapointOffsetTrace(int otIndex)
        {
            //clear current selection
            overlayCanvas.Children.RemoveRange(0, overlayCanvas.Children.Count);

            StylusPoint sp = GetPointFromTraceAt(_offsetTargetTrace, otIndex);

            //draw alignment lign to corresponding expert dp
            DrawNearAlignmentLinesForDatapoint( _offsetTargetTrace, _studentTraceUtils.Trace, _alignmentVector, otIndex);

            //draw ellipse on student dp
            OverlayDrawDatapoint(sp.X, sp.Y);

            manager.UpdateSelectedPoint(otIndex);
        }

        /// <summary>
        /// onclick --> find nearest point of student trace or offsetTargetTrace (and select point)
        /// </summary>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            int mouseInteractionDistance = 50;
            Point pnt = e.GetPosition(overlayCanvas);
            double mouse_x = pnt.X;
            double mouse_y = pnt.Y;

            //find nearest point of traces
            TraceUtils _offsetTargetTraceUtils = new TraceUtils(_offsetTargetTrace);
            (StylusPoint closestPoint_offsetTrace, double dist_offsetTrace, int ot_traceIndex, int ot_pointIndex) = _offsetTargetTraceUtils.GetClosestPointIndex(pnt);
            (StylusPoint closestPoint_studentTrace, double dist_studentTrace, int stud_traceIndex, int stud_pointIndex) = _studentTraceUtils.GetClosestPointIndex(pnt);

            int pointIndexCalc = 0;
            if (dist_offsetTrace < dist_studentTrace) //offsetTrace has closest point
            {
                if(dist_offsetTrace < mouseInteractionDistance)
                {
                    //find index and select the point in the graphs
                    
                    for(int i = 0; i < ot_traceIndex; i++)
                    {
                        pointIndexCalc += _offsetTargetTraceUtils.Trace[i].StylusPoints.Count;
                    }
                    pointIndexCalc += ot_pointIndex;

                    //highlight pointIndex in canvas
                    OverlaySelectDatapointOffsetTrace(pointIndexCalc);
                    //highlight pointIndex also in graphs

                }
            }
            else //studentTrace has closest point
            {
                if(dist_studentTrace < mouseInteractionDistance)
                {
                    //find index and select the point in the graphs
                    for (int i = 0; i < stud_traceIndex; i++)
                    {
                        pointIndexCalc += _studentTraceUtils.Trace[i].StylusPoints.Count;
                    }
                    pointIndexCalc += stud_pointIndex;

                    //highlight pointIndex in canvas
                    OverlaySelectDatapoint(pointIndexCalc);
                    //highlight pointIndex also in graphs

                }
            }

            
            
        }

        

        void OverlayDrawDatapoint(double X, double Y)
        {
            int ellipseDiameter = 8;

            Ellipse ellipse = new Ellipse();
            ellipse.StrokeThickness = 1;
            ellipse.Fill = new SolidColorBrush(Color.FromRgb(255, 240, 31));
            ellipse.Stroke = new SolidColorBrush(Color.FromRgb(0, 32, 255));
            ellipse.Width = ellipseDiameter;
            ellipse.Height = ellipseDiameter;
            
            Canvas.SetLeft(ellipse, X - (ellipseDiameter/2));
            Canvas.SetTop(ellipse, Y - (ellipseDiameter / 2));

            overlayCanvas.Children.Add(ellipse);
        }

        void CenterScrollViewOnTrace(StrokeCollection traceToCenter)
        {
            TraceUtils traceUtilsTraceToCenter = new TraceUtils(traceToCenter);
            int[] traceBounds = traceUtilsTraceToCenter.GetBounds();

            inkCanvas.Width = traceBounds[2] + 100;
            inkCanvas.Height = traceBounds[3] + 100;

            Debug.WriteLine("inkcanvas width: " + inkCanvas.Width);
            Debug.WriteLine("canvasDock.ScrollableHeight: " + canvasDock.ScrollableHeight);



            Debug.WriteLine("trace bounds: " + traceBounds[0] + ", " + traceBounds[1] + ", " + traceBounds[2] + ", "+traceBounds[3] + ".");
            int scrollToX = (int)inkCanvasScaleTransform.ScaleX * (traceBounds[0]) ;
            int scrollToY = (int)inkCanvasScaleTransform.ScaleY * (traceBounds[1]) ;
            ScrollToCanvasLocation(scrollToX, scrollToY);

            //TODO Zoom to fit content
        }

        void ScrollToCanvasLocation(int x, int y)
        {
            if(canvasDock.ScrollableHeight< y && canvasDock.ScrollableWidth < x)
            {
                canvasDock.ScrollToHorizontalOffset(x);
                canvasDock.ScrollToVerticalOffset(y);
            } else
            {
                Debug.WriteLine("ScrollToCanvasLocation error: coordinates out of bound. Width: " + canvasDock.ScrollableWidth + ". Height: " + canvasDock.ScrollableHeight + " input was x: " + x + " y: " + y);   
            }
           
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

            List<float> curv_student = new List<float>();
            List<float> curv_scores = new List<float>();
            List<float> curv_avg = new List<float>();
            List<float> curv_std = new List<float>();

            List<float> pressure_student = new List<float>();
            List<float> pressure_scores = new List<float>();
            List<float> pressure_avg = new List<float>();
            List<float> pressure_std = new List<float>();

            List<float> speed_student = new List<float>();
            List<float> speed_scores = new List<float>();
            List<float> speed_avg = new List<float>();
            List<float> speed_std = new List<float>();

            int totalPoints = 0;
            for(int i = 0; i < trace.Count; i++)
            {
               totalPoints += trace[i].StylusPoints.Count;
            }

            //use alignmentVector to find corresponding edmDataPoints
            List<EDMDataPoint> relevantEDMPoints = new List<EDMDataPoint>();
            int previousStudentMatchIndex = -1;
            EDMDataPoint[] correspondingDataPoints = new EDMDataPoint[totalPoints];
            StylusPoint previousStudentPoint;
            StylusPoint studentPoint;
            bool firstPointSet = false;
            for (int i = aV.Count-1; i >= 0; i--)
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
                    if (firstPointSet)
                    {
                        previousStudentPoint = studentPoint;
                    }                    
                    studentPoint = GetPointFromTraceAt(trace, previousStudentMatchIndex);
                    
                    float score_x = GetEDMScore(studentPoint, correspondingDataPoints[previousStudentMatchIndex], "X", 1);
                    float score_y = GetEDMScore(studentPoint, correspondingDataPoints[previousStudentMatchIndex], "Y", 1);
                    float score_pressure = GetEDMScore(studentPoint, correspondingDataPoints[previousStudentMatchIndex], "Pressure", 1);

                    x_student.Add((float)studentPoint.X);
                    x_scores.Add(score_x);
                    x_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].X);
                    x_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].X_std);

                    y_student.Add((float)studentPoint.Y);
                    y_scores.Add(score_y);
                    y_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].Y);
                    y_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].Y_std);

                    pressure_student.Add((float)studentPoint.PressureFactor);
                    pressure_scores.Add(score_pressure);
                    pressure_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].Pressure);
                    pressure_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].Pressure_std);

                    if (firstPointSet)
                    {
                        float score_curv = GetEDMScore_Derivative(studentPoint, previousStudentPoint, correspondingDataPoints[previousStudentMatchIndex], "Curvature", 1);
                        float x_diff = (float)(studentPoint.X - previousStudentPoint.X);
                        float y_diff = (float)(studentPoint.Y - previousStudentPoint.Y);
                        float angle = MathF.Atan2(y_diff, x_diff);

                        curv_student.Add(angle);
                        curv_scores.Add(score_curv);
                        curv_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].Curvature);
                        curv_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].Curvature_std);

                        float score_speed = GetEDMScore_Derivative(studentPoint, previousStudentPoint, correspondingDataPoints[previousStudentMatchIndex], "Speed", 1);
                        float diagonalLength = MathF.Sqrt(MathF.Pow((float)(studentPoint.X - previousStudentPoint.X), 2) + MathF.Pow((float)(studentPoint.Y - previousStudentPoint.Y), 2));
                        //TODO divide by time passed! 

                        speed_student.Add(diagonalLength);
                        speed_scores.Add(score_speed);
                        speed_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].Speed);
                        speed_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].Speed_std);
                    }

                    //print in console
                    Debug.WriteLine("Student point " + previousStudentMatchIndex + " comparison with EDM point. X diff: " + score_x + " Y diff: " + score_y);
                    firstPointSet = true;
                }
                relevantEDMPoints.Add(edmData.dataPoints[aV[i].Item2]);                

                previousStudentMatchIndex = aV[i].Item1;
            }
            EDMComparisonResult x_comparison = new EDMComparisonResult(x_student, x_scores, x_avg, x_std, "X of student vs EDM", "X position");
            result.Add(x_comparison);
            EDMComparisonResult y_comparison = new EDMComparisonResult(y_student, y_scores, y_avg, y_std, "Y of student vs EDM", "Y position");
            result.Add(y_comparison);
            EDMComparisonResult curv_comparison = new EDMComparisonResult(curv_student, curv_scores, curv_avg, curv_std, "Curvature of student vs EDM", "Curvature angle in degrees");
            result.Add(curv_comparison);
            EDMComparisonResult pressure_comparison = new EDMComparisonResult(pressure_student, pressure_scores, pressure_avg, pressure_std, "Pressure of student vs EDM", "Pen PressureFactor on scale [0-1]");
            result.Add(pressure_comparison);
            EDMComparisonResult speed_comparison = new EDMComparisonResult(speed_student, speed_scores, speed_avg, speed_std, "Speed of student vs EDM", "Trace speed");
            result.Add(speed_comparison);
            return result;
        }

        private float GetEDMScore(StylusPoint studentPoint, EDMDataPoint edmPoint, String feature, float scale)
        {
            switch (feature)
            {
                case "X":
                    return (float)((studentPoint.X - edmPoint.X) / (edmPoint.X_std * scale));                    

                case "Y":
                    return (float)((studentPoint.Y - edmPoint.Y) / (edmPoint.Y_std * scale));                    

                case "Pressure":
                    return (float)((studentPoint.PressureFactor - edmPoint.Pressure) / (edmPoint.Pressure_std * scale));                                                      

                default:
                    Debug.WriteLine("Feature not found: " + feature);
                    break;
            }

            return 0;
        }

        private float GetEDMScore_Derivative(StylusPoint studentPoint, StylusPoint previousStudentPoint, EDMDataPoint edmPoint, String feature, float scale)
        {
            switch (feature)
            {
                case "Curvature":
                    float x_diff = (float)(studentPoint.X - previousStudentPoint.X);
                    float y_diff = (float)(studentPoint.Y - previousStudentPoint.Y);                           
                    return MathF.Acos(x_diff)/(MathF.Sqrt(MathF.Pow(x_diff,2)+MathF.Pow(y_diff, 2)));

                case "Speed":                    
                    float diagonalLength = MathF.Sqrt(MathF.Pow((float)(studentPoint.X - previousStudentPoint.X), 2) + MathF.Pow((float)(studentPoint.Y - previousStudentPoint.Y), 2));
                    //TODO divide by time passed! 
                    return (float)((diagonalLength - edmPoint.Speed) / (edmPoint.Speed_std * scale));

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
            double Pressure = 0;
            double Pressure_std = 0;
            double Speed = 0;
            double Speed_std = 0;

            //take avg of each property
            for (int i = 0; i < relevantEDMPoints.Count; i++)
            {
                X += relevantEDMPoints[i].X / relevantEDMPoints.Count;
                X_std += relevantEDMPoints[i].X_std / relevantEDMPoints.Count;
                Y += relevantEDMPoints[i].Y / relevantEDMPoints.Count;
                Y_std += relevantEDMPoints[i].Y_std / relevantEDMPoints.Count;
                Pressure += relevantEDMPoints[i].Pressure / relevantEDMPoints.Count;
                Pressure_std += relevantEDMPoints[i].Pressure / relevantEDMPoints.Count;
                Speed += relevantEDMPoints[i].Speed / relevantEDMPoints.Count;
                Speed_std += relevantEDMPoints[i].Speed / relevantEDMPoints.Count;
            }
            dataPoint.X = X;
            dataPoint.Y = Y;
            dataPoint.X_std = X_std;
            dataPoint.Y_std = Y_std;
            dataPoint.Pressure = Pressure;
            dataPoint.Pressure_std = Pressure_std;
            dataPoint.Speed = Speed;
            dataPoint.Speed_std = Speed_std;
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

        /// <summary>
        /// draw alignment lines for selected DP and neighbouring DP within bounds index distance
        /// </summary>
        /// <param name="studentStrokes"></param>
        /// <param name="offsetStrokes"></param>
        /// <param name="aV"></param>
        /// <param name="studentIndex"></param>
        /// <param name="bounds">max index distance to draw alignment lines for</param>
        public void DrawNearAlignmentLinesForDatapoint(StrokeCollection studentStrokes, StrokeCollection offsetStrokes, List<(int, int)> aV, int studentIndex, int bounds = 5)
        {
            int idx = 0;
            int minIndex = studentIndex - bounds;
            int maxIndex = studentIndex + bounds;
            foreach ((int, int) match in aV)
            {
                if(match.Item1 > minIndex && match.Item1 < maxIndex)
                {
                    StylusPoint spRef = GetPointFromTraceAt(studentStrokes, match.Item1);
                    StylusPoint spCor = GetPointFromTraceAt(offsetStrokes, match.Item2);
                    float alpha = 1f - (Math.Abs(studentIndex - match.Item1)*1f / (1f*bounds));
                    float progress = 1f * (bounds + match.Item1 - studentIndex) / (2f*bounds);

                    Debug.WriteLine("progress: " + progress);

                    Line line = new Line();
                    line.X1 = spRef.X;
                    line.Y1 = spRef.Y;
                    line.X2 = spCor.X;
                    line.Y2 = spCor.Y;
                    
                    int red = (int) (progress * 254);
                    int green = 255 - red;
                    int blue = match.Item1 == studentIndex ? 255 : 0;

                    line.Stroke = new SolidColorBrush(Color.FromArgb((byte)(alpha*254),(byte)red, (byte)green, (byte)blue));
                    overlayCanvas.Children.Add(line);                    
                }
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

        static Stroke MakeLine(double x1, double y1, double x2, double y2, float progress, float alpha = 0.2f)
        {
            StylusPoint sp_1 = new StylusPoint(x1, y1);
            StylusPoint sp_2 = new StylusPoint(x2, y2);

            StylusPointCollection spCol = new StylusPointCollection();
            spCol.Add(sp_1);
            spCol.Add(sp_2);

            Stroke stroke = new Stroke(spCol);

            Color color = Colors.Green;
            color.ScA = alpha;
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
