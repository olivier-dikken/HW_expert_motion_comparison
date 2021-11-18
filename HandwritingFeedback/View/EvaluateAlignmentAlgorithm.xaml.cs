using HandwritingFeedback.Models;
using HandwritingFeedback.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// Interaction logic for EvaluateAlignmentAlgorithm.xaml
    /// </summary>
    public partial class EvaluateAlignmentAlgorithm : Page
    {
        public EvaluateAlignmentAlgorithm()
        {
            InitializeComponent();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }

        private void LoadTestSample(object sender, RoutedEventArgs e)
        {
            //load test sample
            AlignmentTestSample ats = AlignmentTestSample.LoadFromFile("TEST_SAVE");

            

            //show traces 
            inkCanvas.Strokes = ats.eStrokes.Clone();
            StrokeCollection offsetCollection = OffsetStrokeCollection(ats.sStrokes, 500);
            inkCanvas.Strokes.Add(offsetCollection);

            //run alignment algorithm
            List<(int, int)> aV = ats.alignmentVector;

            //show alignment
            int subsetSize = 50;
            List<(int, int)> newAlignment = GetAlignment(ats.eStrokes, ats.sStrokes, subsetSize);
          
            DrawAlignmentLines(ats.eStrokes, offsetCollection, newAlignment);

            //show evaluation score
            double score = AlignmentScore(newAlignment, aV);

            // Format text to be added to dock
            var view = new TextBlock
            {
                Text = "Alignment Evaluation Errors: " + score.ToString() + " total points: " + newAlignment.Count
            };

            // Add new text to dock
            unitValueDock.Children.Add(view);
        }

        public List<(int, int)> GetAlignment(StrokeCollection scExpert, StrokeCollection scStudent, int subsetSize)
        {
            List<(float, float)> coordsE = Alignment.TraceToCoords(scExpert);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);

            List<(float, float)> coordsSSubset = new List<(float, float)>();
            //get subset of coordsS
            for(int i = 0; i < subsetSize; i++)
            {
                coordsSSubset.Add(coordsS[i]);
            }

            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsSSubset);

            //get only subset alignment
            List<(int, int)> completeAlignment = Alignment.TestBestPath(coordDistances);
            List<(int, int)> subsetAlignment = new List<(int, int)>();
            for (int i = completeAlignment.Count-1; i > 0 ; i--)
            {
                subsetAlignment.Add(completeAlignment[i]);
                if (completeAlignment[i].Item2 == subsetSize - 1)
                    break;
            }
            return subsetAlignment;
        }

        public double AlignmentScore(List<(int, int)> newAlignment, List<(int, int)> objective)
        {
            double errorScore = 0;
            for(int i = 0; i < newAlignment.Count; i++)
            {
                (int, int) currentMatch = newAlignment[i];
                if (objective.Contains(currentMatch))
                    continue;
                //if match not in newAlignment then compute the error distance
                errorScore += 1;
            }
            return errorScore;
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
    }
}
