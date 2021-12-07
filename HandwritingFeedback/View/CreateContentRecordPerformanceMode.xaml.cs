﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for CreateContentRecordPerformanceMode.xaml
    /// </summary>
    public partial class CreateContentRecordPerformanceMode : Page
    {
        int iteration = 0;
        int numberOfSamples = 5;

        int type = 0;
        int helperLineType = 0;
        int startingPoint = 0;
        int lineSpacing = 20;

        StrokeCollection[] expertPerformances;

        public static TraceUtils ExpertTraceUtils { get; set; }
        public static TraceUtils StudentTraceUtils { get; private set; }
        public static StrokeCollection ExpertOutline = new StrokeCollection();

        StrokeCollection TargetTrace;

        StrokeCollection expertPerformance = null;

        bool showingAlignment = false;

        public CreateContentRecordPerformanceMode()
        {
            InitializeComponent();
            Load();

            expertPerformances = new StrokeCollection[numberOfSamples];
        }

        private void ReadConfigFile()
        {
            string[] lines = System.IO.File.ReadAllLines(GlobalState.CreateContentsPreviousFolder + "\\exerciseConfig.txt");

            string description = lines[0];
            type = Int32.Parse(lines[lines.Length - 3]);
            helperLineType = Int32.Parse(lines[lines.Length - 2]);
            startingPoint = Int32.Parse(lines[lines.Length - 1]);
        }

        private void Load()
        {
            //read config from text file
            ReadConfigFile();

            //load exercise config
            iteration = 1; //start at iteration 1 instead of 0
            LoadIteration();

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

        /// <summary>
        /// Removes all strokes from the canvas.
        /// </summary>
        private void ClearCanvas()
        {
            LoadIteration();                    
        }

        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            ClearCanvas();
        }

        private void ButtonSubmit(object sender, RoutedEventArgs e)
        {
            if (showingAlignment)
            {
                //add/enable a confirm button

                //compute difference per datapoint compared to TargetTrace (many_to_1 -> avg the many; 1_to_many -> copy the 1)

                //and save the aligned differences in array per dimension (output array same number of datapoints as TargetTrace)                        

                //set iteration number         
                if (iteration == numberOfSamples) //IF LAST SUBMISSION
                {
                    SaveExercise();
                }
                else //go to next iteration
                {
                    NextIteration();
                }
            } else //show alignment
            {
                //get expert strokecollection
                expertPerformance = ExpertCanvas.Strokes.Clone();
                //show alignment
                ShowAlignment();
                SubmitButton.Content = "Confirm";
                SubmitButton.IsEnabled = true;
            }            
        }

        private void ShowAlignment()
        {
            //clear canvas
            ExpertCanvasBG.Reset();
            ExpertCanvas.Reset();
            SubmitButton.IsEnabled = false;

            //show alignment
            // 1 - get TargetTrace collection and offset it
            TargetTrace = FileHandler.LoadStrokeCollection(GlobalState.CreateContentsPreviousFolder + "\\TargetTrace.isf");
            StrokeCollection offsetTargetTrace = OffsetStrokeCollection(TargetTrace, 300);
            ExpertCanvas.Strokes.Add(expertPerformance);
            ExpertCanvas.Strokes.Add(offsetTargetTrace);

            //get alignment
            List<(int, int)> alignmentVector = getBestPath(TargetTrace, expertPerformance);
            DrawAlignmentLines( offsetTargetTrace, expertPerformance, alignmentVector);

            showingAlignment = true;            
        }

        private void TransformStrokeDataToTarget(StrokeCollection target, StrokeCollection toTransform)
        {
            List<(int, int)> alignmentVector = getBestPath(target, toTransform);
            int targetLength = alignmentVector[0].Item1;
            double[] transformed_x = new double[targetLength];

            double nextVals;
            double amount;
            for(int i = 0; i < targetLength; i++)
            {
                nextVals = 0;
                amount = 0;
                for(int j = 0; j < alignmentVector.Count; j++)
                {
                    if(alignmentVector[j].Item1 == i)
                    {
                        nextVals += GetPointFromTraceAt(toTransform, alignmentVector[j].Item2).X;
                        amount += 1;
                    }
                }                
                transformed_x[i] = nextVals / amount;
            }
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

        public List<(int, int)> getBestPath(StrokeCollection scExpert, StrokeCollection scStudent)
        {
            List<(float, float)> coordsE = Alignment.TraceToCoords(scExpert);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);
            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsS);

            return Alignment.TestBestPath(coordDistances);
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

        private void LoadIteration()
        {
            //clear canvas
            ExpertCanvasBG.Reset();
            ExpertCanvas.Reset();
            SubmitButton.IsEnabled = false;

            //discard previous expert recording
            expertPerformance = null;

            //show trace/no trace
            if (type == 0)//show trace
            {
                LoadOutlineTrace();
            }
            //show starting point yes/now
            if (startingPoint == 1)//show starting point
            {

            }    

            //helper lines
            TraceUtils.DrawHelperLines(ExpertCanvasBG, helperLineType, lineSpacing);

            //show iteration (start with 5 recordings) 1/5            
            if (iteration == numberOfSamples)
            {
                SubmitButton.Content = "Finish Submission";
            }
            else
            {
                SubmitButton.Content = "Submit " + iteration + "/" + numberOfSamples;
            }

            showingAlignment = false;
        }

        private void NextIteration()
        {
            //store expertPerformance of previous iteration
            TransformStrokeDataToTarget(TargetTrace, expertPerformance);
            expertPerformances[iteration - 1] = expertPerformance;
            
            iteration++;

            LoadIteration();            
        }

        private void SaveExercise()
        {
            //TargetTrace and config already saved, need to save EDM and add RDY to config file
            //convert arrays per dimension to avg + std per dimension
            

            //save to file, format: dimension=[0,4,2,1,...] e.g. distance_avg=[0.18,1.1,0.87,...]; distance_std=[0.11, 0.12, 0.03,...]

            //return to menu
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
        /// This method enables the save button when 
        /// at least one stroke is present on the canvas.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        void EnableSubmit(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Canvas must contain at least 1 stroke.
            if (!SubmitButton.IsEnabled && ExpertCanvas.Strokes.Count != 0)
            {
                SubmitButton.IsEnabled = true;
            }
        }
    }
}