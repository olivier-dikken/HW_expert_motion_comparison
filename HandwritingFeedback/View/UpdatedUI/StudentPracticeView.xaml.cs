﻿using HandwritingFeedback.Config;
using HandwritingFeedback.Config.Visual;
using HandwritingFeedback.Models;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.Templates;
using HandwritingFeedback.Util;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for StudentPracticeView.xaml
    /// </summary>
    public partial class StudentPracticeView : Page, IMenuHeaderControls
    {
        private float alignmentDistanceSeriousAttemptThreshold = 0.2f;

        public static TraceUtils ExpertTraceUtils { get; set; }
        public static TraceUtils StudentTraceUtils { get; private set; }
        public static StrokeCollection ExpertOutline = new StrokeCollection();

        EDMData loadedEDMData;

        public static StrokeCollection TargetTrace;

        public ExerciseItem exerciseItem;


        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }

        int helperLineType = 0;
        int lineSpacing = 20;

        Stroke refStartingPoint = null;

        int currentStrokeIndex = 0;
        bool incorrectAttempt = false;

        public StudentPracticeView()
        {

            exerciseItem = FileHandler.GetExericseItem(GlobalState.SelectedExercisePath);
            helperLineType = exerciseItem.lineType;
            lineSpacing = exerciseItem.lineSpacing;

            HomeCommand = new AnotherCommandImplementation(
                _ =>
                {
                    HomeButton(null, null);
                });
            BackCommand = new AnotherCommandImplementation(
                _ =>
                {
                    Button fakeButton = new Button();
                    fakeButton.Tag = "\\View\\UpdatedUI\\ManageLearningContent.xaml";
                    BackButton(fakeButton, null);
                });

            this.DataContext = this;

            InitializeComponent();

            TargetTrace = FileHandler.LoadStrokeCollection(GlobalState.SelectedExercisePath + "\\TargetTrace.isf");
            ExpertTraceUtils = new TraceUtils(TargetTrace);

            loadedEDMData = ExpertDistributionModel.LoadFromFile(GlobalState.SelectedExercisePath + "\\EDMData");
            Debug.WriteLine($"number of loaded datapoints: {loadedEDMData.GetLength()}");

            StudentEditCanvas.DefaultStylusPointDescription =
                ApplicationConfig.Instance.StylusPointDescription;
            // After the first stroke is completed, the submit button will be enabled
            StudentEditCanvas.StrokeCollected += EnableSubmission;
            StudentEditCanvas.StrokeCollected += OnStrokeCollected;
            StudentEditCanvas.StylusDown += OnStylusDown;
            //add target trace to trace over
            CanvasBG.Strokes.Add(TargetTrace.Clone());
            StudentEditCanvas.IsEnabled = true;

            TraceUtils.DrawHelperLines(CanvasBG, helperLineType, lineSpacing);
            

            VisualFeedback.GetInstance();
            //AuditoryFeedback.GetInstance();

            HighlightStroke(TargetTrace.Clone(), 0);
        
        }

        /// <summary>
        /// on stylus down remove highlighted starting point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            //remove highlighted starting point
            if(refStartingPoint == null)
            {
                Debug.WriteLine("Starting point not yet set");
            } else
            {
                CanvasBG.Strokes.Remove(refStartingPoint);
            }
        }


        /// <summary>
        /// when a stroke is collected, if it was a serious attempt then move on to next stroke
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            //get alignment distance between last student stroke and the target trace stroke it should correspond to
            if(currentStrokeIndex >= TargetTrace.Count)
            {
                Debug.WriteLine("All strokes have been matched. Previous stroke has no target trace stroke to match with");
                return;
            }
            
            
            if (!incorrectAttempt && Alignment.getAlignmentDistance(e.Stroke, TargetTrace[currentStrokeIndex]) < alignmentDistanceSeriousAttemptThreshold) //if stroke was a serious attempt. Block is an incorrect attempt was made
            {
                Debug.WriteLine("Incorrect attempt");
                //highlight the next stroke                
                HighlightStroke(TargetTrace.Clone(), currentStrokeIndex+1);//highlight next stroke
            } else
            {
                //stroke was not a serious attempt, show message to undo? / highlight undo button?
                Debug.WriteLine("Highlight undo button!");
                UndoButton.Foreground = new SolidColorBrush(Colors.Red);
                //also highlight current stroke in RED to show it has become inactive due to incorrect attempt
                incorrectAttempt = true;
            }


            currentStrokeIndex++; //end function by incrementing index counter
        }

    
        


        private void HighlightStrokeStartingPoint(Stroke stroke)
        {
            if(stroke == null)
            {
                Debug.WriteLine("HighlightStrokeStartingPoint received empty stroke");
                return;
            }

            //draw ellipse on first point of stroke
            StylusPoint strokeStart = stroke.StylusPoints[0];

            double radius = 6;

            StylusPointCollection pts = new StylusPointCollection();
            pts.Add(new StylusPoint(strokeStart.X, strokeStart.Y));

            refStartingPoint = new customDotStroke(pts, radius);
            refStartingPoint.DrawingAttributes.Color = Colors.DarkOrange;

            CanvasBG.Strokes.Add(refStartingPoint);       
        }

        private void HighlightStroke(StrokeCollection strokeCollection, int strokeIndex)
        {
            CanvasBG.Strokes.Clear();
            Stroke toHighlight = null;


             if (strokeCollection.Count > strokeIndex)
            {                
                toHighlight = strokeCollection[strokeIndex];
                strokeCollection.RemoveAt(strokeIndex);
                //SolidColorBrush scb = new SolidColorBrush(Colors.Purple);
                toHighlight.DrawingAttributes.Color = Colors.Purple;
                strokeCollection.Add(toHighlight);      //add as last element so appears on top of all other targetTrace strokes           
            } else
            {
                Debug.WriteLine("highlight stroke, strokeIndex out of bounds");
            }
            CanvasBG.Strokes.Add(strokeCollection);

            HighlightStrokeStartingPoint(toHighlight);
        }


        private void SubmitTrace(object sender, RoutedEventArgs e)
        {
            // The student should not be able to write after clicking submit
            StudentEditCanvas.IsEnabled = false;

            StudentTraceUtils = new TraceUtils(StudentEditCanvas.Strokes.Clone());

            // Add student and expert traces to be sent to batched analytics
            var inputData = new BFInputData
            {
                StudentTraceUtils = StudentTraceUtils,
                ExpertTraceUtils = ExpertTraceUtils,
                ExpertOutline = ExpertTraceUtils.Trace//ExpertOutline
            };

            Debug.WriteLine("studentTraceUtils.Trace.Count: " + inputData.StudentTraceUtils.Trace.Count);
            Debug.WriteLine("expertTraceUtils.Trace.Count: " + inputData.ExpertTraceUtils.Trace.Count);
            Debug.WriteLine("expertOutline.Count: " + inputData.ExpertOutline.Count);

            // Navigate to batched analytics view and transfer traces
            var destination = new BatchedAnalytics_EDM(inputData, loadedEDMData);
            NavigationService.Navigate(destination);
        }

        /// <summary>
        /// This method enables the submit button when an expert trace is loaded on the canvas
        /// and at least one stroke has been written by the user. Checking for the user's
        /// stroke count is not necessary because this method gets invoked only when the
        /// user lifts their pen, which guarantees at least one stroke.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void EnableSubmission(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Expert trace must be loaded and contain at least 1 stroke
            if (!SubmitButton.IsEnabled)
            {
                SubmitButton.IsEnabled = true;
            }
        }

        

        private void RedrawHelperLines()
        {
            CanvasBG.Reset();
            TraceUtils.DrawHelperLines(CanvasBG, helperLineType, lineSpacing);         
        }

        /// <summary>
        /// Removes all strokes from the canvas.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        public void ClearCanvasButton(object sender, RoutedEventArgs e)
        {
            StudentEditCanvas.Strokes.Clear();
            currentStrokeIndex = 0;
            RedrawHelperLines();
            //redraw next stroke
            HighlightStroke(TargetTrace.Clone(), currentStrokeIndex);
        }

        public void UndoCanvasButton(object sender, RoutedEventArgs e)
        {            
            if (StudentEditCanvas.Strokes.Count > 0)
            {
                incorrectAttempt = false;

                StudentEditCanvas.Strokes.RemoveAt(StudentEditCanvas.Strokes.Count - 1);
                currentStrokeIndex--;
                HighlightStroke(TargetTrace.Clone(), currentStrokeIndex);

                UndoButton.Background = Brushes.White; //set back to white incase it was highlighted
                UndoButton.Foreground = Brushes.Black;
            }
        }

       

        public void SubmitCanvasButton(object sender, RoutedEventArgs e)
        {
            SubmitTrace(sender, e);
        }

      

        public void BackButton(object sender, RoutedEventArgs e)
        {
            //TODO add pop up warning message, confirm discard current creation
            CommonUtils.Navigate(sender, e, this);
        }

        public void CloseApplicationButton(object sender, RoutedEventArgs e)
        {
            ((IMenuHeaderControls)this).CloseApplication();
        }

        public void HelpButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void HomeButton(object sender, RoutedEventArgs e)
        {
            ((IMenuHeaderControls)this).GoHome(this);
        }

        public void ReportBugButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SettingsButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }
    }
}

