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
using HandwritingFeedback.Config;
using System.IO;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for EvaluateAlignmentAlgorithm.xaml
    /// </summary>
    public partial class EvaluateAlignmentAlgorithm : Page
    {
        public static TraceUtils ExpertTraceUtils { get; set; }
        public static TraceUtils StudentTraceUtils { get; private set; }
        public static StrokeCollection ExpertOutline = new StrokeCollection();
        StrokeCollection TargetTrace;
        StrokeCollection CandidateTrace;

        bool eSubmitted = false;

        public EvaluateAlignmentAlgorithm()
        {
            InitializeComponent();

            ExpertCanvas.DefaultStylusPointDescription =
                ApplicationConfig.Instance.StylusPointDescription;
        }


        private void InitStudentCanvas()
        {
            NextButton.Content = "Submit";

            LoadOutlineTrace();                   
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }

        private void CreateTestSample(object sender, RoutedEventArgs e)
        {
            //set canvas to draw E stroke and submit

            //then allow drawing of S trance and submit

            //comput mapping_GT on submission
        }

        /// <summary>
        /// clone to expertCanvasBG
        /// </summary>
        private void LoadOutlineTrace()
        {
            FileHandler.LoadTrace(GlobalState.CreateContentsPreviousFolder + "\\TargetTrace.isf", ExpertCanvasBG);
            // The .ISF was loaded, so the canvas currently contains the expert's
            // trace, which gets saved here.
            ExpertTraceUtils = new TraceUtils(ExpertCanvasBG.Strokes.Clone());

            // Modify the color of the expert's trace
            // This will allow the expert's trace to be darker than
            // the outline.
            foreach (var stroke in ExpertTraceUtils.Trace)
            {
                stroke.DrawingAttributes.Color = Color.FromRgb(200, 200, 200);
            }

            // The existing expert's trace on the canvas gets transformed into
            // a thicker outline here.
            foreach (var stroke in ExpertCanvasBG.Strokes)
            {
                // Make the background trace uniform by disabling pressure sensitivity
                stroke.DrawingAttributes.IgnorePressure = true;

                // Set the stroke size for each stroke based on the configured value
                stroke.DrawingAttributes.Width =
                    ApplicationConfig.Instance.MaxDeviationRadius * 2d;
                stroke.DrawingAttributes.Height =
                    ApplicationConfig.Instance.MaxDeviationRadius * 2d;
            }

            // The canvas currently contains the expert's
            // outline, which gets saved here.
            ExpertOutline = ExpertCanvasBG.Strokes.Clone();

            // Add the expert's trace on top of the outline
            ExpertCanvasBG.Strokes.Add(ExpertTraceUtils.Trace);
        }

        private void ButtonNext(object sender, RoutedEventArgs e)
        {
            if (!eSubmitted)
            {


                //create folder with filename
                string workingDirectory = Environment.CurrentDirectory;
                string path = Directory.GetParent(workingDirectory).Parent.FullName + "\\SavedData\\TestAlignment\\" + Guid.NewGuid().ToString();
                Debug.WriteLine("working directory: " + workingDirectory);
                Debug.WriteLine("path to create folder at: " + path);

                if (!Directory.Exists(path))
                {
                    GlobalState.CreateContentsPreviousFolder = path;
                    Directory.CreateDirectory(path);
                    //save trace to folder as TargetTrace.isf
                    FileHandler.SaveTargetTrace(ExpertCanvas.Strokes, path);
                    TargetTrace = ExpertCanvas.Strokes;
                    eSubmitted = true;                    
                    ClearCanvas(sender, e);
                    InitStudentCanvas();
                }
                else
                {
                    Debug.WriteLine("Error, folder name already exists");
                }
            } else
            {
                //save student trace
                Debug.WriteLine("Saving student stroke");
                FileHandler.SaveCandidateTrace(ExpertCanvas.Strokes, GlobalState.CreateContentsPreviousFolder);
                CandidateTrace = ExpertCanvas.Strokes;
                //get alignment
                List<(int, int)> alignmentVector = getBestPath(TargetTrace, CandidateTrace);
                File.WriteAllTextAsync(GlobalState.CreateContentsPreviousFolder + "\\aV.txt", String.Join(", ", alignmentVector));
                ClearCanvas(sender, e);
                eSubmitted = false;
                NextButton.Content = "Next";
            }
        }

        public List<(int, int)> getBestPath(StrokeCollection scExpert, StrokeCollection scStudent)
        {
            List<(float, float)> coordsE = Alignment.TraceToCoords(scExpert);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);
            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsS);

            return Alignment.TestBestPath(coordDistances);
        }

        /// <summary>
        /// Removes all strokes from the canvas.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            ExpertCanvasBG.Reset();
            ExpertCanvas.Reset();
            NextButton.IsEnabled = false;
        }

        /// <summary>
        /// This method enables the save button when 
        /// at least one stroke is present on the canvas.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        void EnableNext(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Canvas must contain at least 1 stroke.
            if (!NextButton.IsEnabled && ExpertCanvas.Strokes.Count != 0)
            {
                NextButton.IsEnabled = true;
            }
        }

        private void LoadTestSample(object sender, RoutedEventArgs e)
        {
            //load test sample
            AlignmentTestSample ats = AlignmentTestSample.LoadFromFile("TEST_SAVE");

            

            //show traces 
            ExpertCanvas.Strokes = ats.eStrokes.Clone();
            StrokeCollection offsetCollection = OffsetStrokeCollection(ats.sStrokes, 500);
            ExpertCanvas.Strokes.Add(offsetCollection);

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
                ExpertCanvas.Strokes.Add(mappingLine);
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
