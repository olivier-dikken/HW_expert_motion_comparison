using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for CompareAlignmentAlgorithms.xaml
    /// </summary>
    public partial class CompareAlignmentAlgorithms : Page
    {
        StrokeCollection TargetTrace;
        StrokeCollection CandidateTrace;
        List<(int, int)> aV;

        public CompareAlignmentAlgorithms()
        {
            InitializeComponent();
        }

        private void LoadTestFolder(object sender, RoutedEventArgs e)
        {
            
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = GlobalState.TestAlignmentPath;
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK)
            {
                Debug.WriteLine("Selected path: ");
                Debug.WriteLine(fbd.SelectedPath);
                //get files: TargetTrace.isf; CandidateTrace.isf; aV.txt;
                String aVFileName = fbd.SelectedPath + "\\aV.txt";
                String ttFileName = fbd.SelectedPath + "\\TargetTrace.isf";
                String ctFileName = fbd.SelectedPath + "\\CandidateTrace.isf";

                aV = loadAVFile(aVFileName);
                TargetTrace = FileHandler.LoadStrokeCollection(ttFileName);
                CandidateTrace = FileHandler.LoadStrokeCollection(ctFileName);

                //show traces 
                inkCanvas.Strokes = TargetTrace.Clone();
                StrokeCollection offsetCollection = OffsetStrokeCollection(CandidateTrace, 500);
                inkCanvas.Strokes.Add(offsetCollection);


                DrawAlignmentLines(TargetTrace, offsetCollection, aV);

                //show evaluation score
                double score = AlignmentScore(aV, aV);
                Debug.WriteLine("score: " + score);
                
            }
        }

        public double AlignmentScore(List<(int, int)> newAlignment, List<(int, int)> objective)
        {
            double errorScore = 0;
            for (int i = 0; i < newAlignment.Count; i++)
            {
                (int, int) currentMatch = newAlignment[i];
                if (objective.Contains(currentMatch))
                    continue;
                //if match not in newAlignment then compute the error distance
                errorScore += 1;
            }
            return errorScore;
        }

        List<(int, int)> loadAVFile(String aVFilePath)
        {
            List<(int, int)> aV = new List<(int, int)>();

            if (File.Exists(aVFilePath))
            {
                Debug.WriteLine(String.Format("Reading file {0}", aVFilePath));
                String allFileText = System.IO.File.ReadAllText(aVFilePath);
                String[] split = allFileText.Split("), (");
                foreach (String doubleEntry in split)
                {
                    String[] entryPair = doubleEntry.Replace(" ", "").Replace("(", "").Replace(")", "").Split(",");
                    aV.Add((int.Parse(entryPair[0]), int.Parse(entryPair[1])));
                }
            }
            else
            {
                Debug.WriteLine("File load aV not found");
            }

            return aV;
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
    }
}
