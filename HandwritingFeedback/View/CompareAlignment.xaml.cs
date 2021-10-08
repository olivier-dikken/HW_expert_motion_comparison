using HandwritingFeedback.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for CompareAlignment.xaml
    /// </summary>
    public partial class CompareAlignment : Page
    {
        private readonly int Yoffset = 500;
        private readonly TraceUtils _expertTraceUtils;
        private readonly TraceUtils _studentTraceUtils;
        private readonly StrokeCollection _expertOutline;


        public CompareAlignment(BFInputData input)
        {
            InitializeComponent();

            // Extract student and expert trace from input
            _expertTraceUtils = input.ExpertTraceUtils;
            _studentTraceUtils = input.StudentTraceUtils;
            _expertOutline = input.ExpertOutline;

            inkCanvas.Strokes = _expertOutline.Clone();
            inkCanvas.Strokes.Add(_expertTraceUtils.Trace.Clone());

         
            StrokeCollection offsetCollection = new StrokeCollection();
            
            for(int i = 0; i < _studentTraceUtils.Trace.Count; i++)
            {
                StylusPointDescription spd = _studentTraceUtils.Trace[i].StylusPoints.Description;
                StylusPointCollection offsetPoints = new StylusPointCollection(spd);
                for(int j = 0; j < _studentTraceUtils.Trace[i].StylusPoints.Count; j++)
                {
                    StylusPoint newPoint = _studentTraceUtils.Trace[i].StylusPoints[j];                                        

                    newPoint.Y += Yoffset;
                    offsetPoints.Add(newPoint);
                }
                Stroke newStroke = new Stroke(offsetPoints);
                offsetCollection.Add(newStroke);
            }
            inkCanvas.Strokes.Add(offsetCollection);

            //int[] alignmentVector = GetAlignmentMapping(_expertTraceUtils.Trace, offsetCollection);
            //Debug.WriteLine("Alignment vector: " + String.Join(", ", alignmentVector));

            //for(int i = 0; i < alignmentVector.Length; i++)            
            //{
            //    int idx = alignmentVector[i];
            //    StylusPoint spRef = GetPointFromTraceAt(_expertTraceUtils.Trace, i);
            //    StylusPoint spCor = GetPointFromTraceAt(offsetCollection, idx);
            //    float progress = (float)i / (float)alignmentVector.Length;
            //    Stroke mappingLine = MakeLine(spRef.X, spRef.Y, spCor.X, spCor.Y, progress);
            //    inkCanvas.Strokes.Add(mappingLine);

            //    Debug.WriteLine("progress : " + progress);
            //}

            //TestDTW(_expertTraceUtils.Trace, _studentTraceUtils.Trace);
            List<(int, int)> alignmentVector = getBestPath(_expertTraceUtils.Trace, _studentTraceUtils.Trace);
            int idx = 0;
            foreach((int, int) match in alignmentVector)
            {
                StylusPoint spRef = GetPointFromTraceAt(_expertTraceUtils.Trace, match.Item1);
                StylusPoint spCor = GetPointFromTraceAt(offsetCollection, match.Item2);
                Stroke mappingLine = MakeLine(spRef.X, spRef.Y, spCor.X, spCor.Y, (float)idx/(float)alignmentVector.Count);
                inkCanvas.Strokes.Add(mappingLine);
                idx++;
            }
        }

        public List<(int, int)> getBestPath(StrokeCollection scExpert, StrokeCollection scStudent)
        {
            List<(float, float)> coordsE = Alignment.TraceToCoords(scExpert);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);
            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsS);

            return Alignment.TestBestPath(coordDistances);
            
        }


        public void TestDTW(StrokeCollection scExpert, StrokeCollection scStudent)
        {
            Debug.WriteLine("Testing DTW");
            List<(float, float)> coordsE = Alignment.TraceToCoords(scExpert);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);

            Point p = new Point();            
            

            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsS);

            List<(int, int)> alignmentVector = Alignment.TestBestPath(coordDistances);

            //Debug.WriteLine($"coordDistances matrix size: {coordDistances.GetLength(0)} {coordDistances.GetLength(1)}");
            //Debug.WriteLine("Showing rows/cols [20-30]");
            //for (int i = 20; i < Math.Min(coordDistances.GetLength(0), 30); i++)
            //{
            //    for (int j = 20; j < Math.Min(coordDistances.GetLength(1), 30); j++)
            //    {
            //        Debug.Write($"{coordDistances[i, j]}");
            //    }
            //    Debug.Write(Environment.NewLine);
            //}
            //float bestPathDistance = Alignment.BestPathDistance(coordDistances);
            //Debug.WriteLine($"bestPathDistance: {bestPathDistance}");
        }

        StylusPoint GetPointFromTraceAt(StrokeCollection trace, int idx)
        {
            int counter = 0;
            foreach(Stroke stroke in trace)
            {
                foreach(StylusPoint sp in stroke.StylusPoints)
                {
                    if (counter == idx)
                        return sp;
                    counter++;
                }
            }

            Debug.WriteLine("ERROR no styluspoint found in GetPointFromTraceAt");
            return new StylusPoint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expertStrokes"></param>
        /// <param name="studentStrokes"></param>
        /// <returns>a list of same size as expertStrokes with per expertPoint the matched student point index, -1 if not matched</returns>
        static int[] GetAlignmentMapping(StrokeCollection expertStrokes, StrokeCollection studentStrokes)
        {
            int totalExpertPoints = 0;
            foreach(Stroke stroke in expertStrokes)
            {
                totalExpertPoints += stroke.StylusPoints.Count;
            }
            int[] result = new int[totalExpertPoints];

            int totalStudentPoints = 0;
            foreach(Stroke stroke in studentStrokes)
            {
                totalStudentPoints += stroke.StylusPoints.Count;
            }

            for(int i = 0; i < totalExpertPoints; i++)
            {
                //linear mapping to test
                result[i] = Math.Min(i, totalStudentPoints - 1);
            }
            return result;
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
            color.ScG = 1-progress;
            color.ScR = progress;
            stroke.DrawingAttributes.Color = color;
            return stroke;
        }

        /// <summary>
        /// Navigates to the page indicated by the button tag.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }
    }
}
