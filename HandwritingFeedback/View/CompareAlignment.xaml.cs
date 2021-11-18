using HandwritingFeedback.Models;
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
        private readonly int yOffset = 500;
        private readonly TraceUtils _expertTraceUtils;
        private readonly TraceUtils _studentTraceUtils;
        private readonly StrokeCollection _expertOutline;
        private readonly List<(int, int)> alignmentVector;


        public CompareAlignment(BFInputData input)
        {
            InitializeComponent();

            // Extract student and expert trace from input
            _expertTraceUtils = input.ExpertTraceUtils;
            _studentTraceUtils = input.StudentTraceUtils;
            _expertOutline = input.ExpertOutline;

            inkCanvas.Strokes = _expertOutline.Clone();
            inkCanvas.Strokes.Add(_expertTraceUtils.Trace.Clone());


            StrokeCollection offsetCollection = OffsetStrokeCollection(_studentTraceUtils.Trace, yOffset);
            inkCanvas.Strokes.Add(offsetCollection);


            alignmentVector = getBestPath(_expertTraceUtils.Trace, _studentTraceUtils.Trace);
            DrawAlignmentLines(_expertTraceUtils.Trace, offsetCollection, alignmentVector);
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




        public void TestDTW(StrokeCollection scExpert, StrokeCollection scStudent)
        {
            Debug.WriteLine("Testing DTW");
            List<(float, float)> coordsE = Alignment.TraceToCoords(scExpert);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);

            Point p = new Point();            
            

            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsS);

            List<(int, int)> alignmentVector = Alignment.TestBestPath(coordDistances);

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

        private void TrySaveAndLoad(object sender, RoutedEventArgs e)
        {
            string fileName = "TEST_SAVE";
            AlignmentTestSample toSave = new AlignmentTestSample(_expertTraceUtils.Trace, _studentTraceUtils.Trace, alignmentVector);
            toSave.SaveToFile(fileName);            
            AlignmentTestSample ats = AlignmentTestSample.LoadFromFile(fileName);
            if(ats != null)
            {
                Debug.WriteLine("TESTING SAVE ALIGNMENT");
                Debug.WriteLine(ats.alignmentVector.ToString());
            }
        }
    }
}
