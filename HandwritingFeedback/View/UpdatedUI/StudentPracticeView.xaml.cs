using HandwritingFeedback.Config;
using HandwritingFeedback.Models;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for StudentPracticeView.xaml
    /// </summary>
    public partial class StudentPracticeView : Page, IMenuHeaderControls
    {
        private float alignmentDistanceSeriousAttemptThreshold = 0.15f;
        private List<TraceUtils> studentTraceUtilsSet = new List<TraceUtils>();

        public static TraceUtils ExpertTraceUtils { get; set; }
        public static TraceUtils StudentTraceUtils { get; private set; }               

        EDMData loadedEDMData;
        public static StrokeCollection TargetTrace { get; private set; }
        public ExerciseItem ExerciseItem { get; private set; }

        public AnotherCommandImplementation HomeCommand { get; set; }
        public AnotherCommandImplementation BackCommand { get; set; }

        int helperLineType = 0;
        int lineSpacing = 20;
        Stroke refStartingPoint = null;
        int currentStrokeIndex = 0;
        bool incorrectAttempt = false;
        int currentRepetition = 1;

        public StudentPracticeView()
        {
            InitializeCommands();
            this.DataContext = this;
            InitializeComponent();
            LoadExercise();
            InitializeCanvas();
            InitializeFeedback();
            HighlightStroke(TargetTrace.Clone(), 0);        
        }

        private void InitializeFeedback()
        {
            VisualFeedback.GetInstance();
            //AuditoryFeedback.GetInstance();
        }

        private void InitializeCommands()
        {
            HomeCommand = new AnotherCommandImplementation(_ =>
            {
                HomeButton(null, null);
            });

            BackCommand = new AnotherCommandImplementation(_ =>
            {
                Button fakeButton = new Button();
                fakeButton.Tag = "\\View\\UpdatedUI\\ManageLearningContent.xaml";
                BackButton(fakeButton, null);
            });
        }

        private void LoadExercise()
        {
            ExerciseItem = FileHandler.GetExericseItem(GlobalState.SelectedExercisePath);
            helperLineType = ExerciseItem.lineType;
            lineSpacing = ExerciseItem.lineSpacing;
            exerciseInfoTextBlock.Text = ExerciseItem.description;
            SetRepetitionText();
            TargetTrace = FileHandler.LoadStrokeCollection(GlobalState.SelectedExercisePath + "\\TargetTrace.isf");
            ExpertTraceUtils = new TraceUtils(TargetTrace);
            loadedEDMData = ExpertDistributionModel.LoadFromFile(GlobalState.SelectedExercisePath + "\\EDMData");
            Debug.WriteLine($"number of loaded datapoints: {loadedEDMData.GetLength()}");
        }

        private void InitializeCanvas()
        {
            StudentEditCanvas.DefaultStylusPointDescription = ApplicationConfig.Instance.StylusPointDescription;
            StudentEditCanvas.StrokeCollected += EnableSubmission;
            StudentEditCanvas.StrokeCollected += OnStrokeCollected;
            StudentEditCanvas.StylusDown += OnStylusDown;
            CanvasBG.Strokes.Add(TargetTrace.Clone());
            StudentEditCanvas.IsEnabled = true;
            TraceUtils.DrawHelperLines(CanvasBG, helperLineType, lineSpacing);
        }

        private void SetRepetitionText()
        {
            repetitionStatusTextBlock.Text = $"{currentRepetition} / {ExerciseItem.repetitionAmount}";
        }

        /// <summary>
        /// on stylus down remove highlighted starting point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            RemoveHighlightedStartingPoint();
        }

        private void RemoveHighlightedStartingPoint()
        {
            if (refStartingPoint != null)
            {
                CanvasBG.Strokes.Remove(refStartingPoint);
            }
            else
            {
                Debug.WriteLine("Starting point not yet set");
            }
        }


        /// <summary>
        /// when a stroke is collected, if it was a serious attempt then move on to next stroke
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            UndoButton.IsEnabled = true;
            //get alignment distance between last student stroke and the target trace stroke it should correspond to
            if(currentStrokeIndex >= TargetTrace.Count)
            {
                Debug.WriteLine("All strokes have been matched. Previous stroke has no target trace stroke to match with");
                return;
            } 

            if (!incorrectAttempt && Alignment.getAlignmentDistance(e.Stroke, TargetTrace[currentStrokeIndex]) < alignmentDistanceSeriousAttemptThreshold) //if stroke was a serious attempt. Block is an incorrect attempt was made
            {
                HandleCorrectAttempt();
            } else
            {
                HandleIncorrectAttempt();
            }

            if(currentStrokeIndex == TargetTrace.Count && !incorrectAttempt) //when all strokes are done and is a serious attempt, enable submit button
            {
                EnableSubmitButton();                
            } else
            {
                DisableSubmitButton();                
            }
            
        }

        private void HandleCorrectAttempt()
        {
            Debug.WriteLine("Correct attempt");
            HighlightNextStroke();
            currentStrokeIndex++;
        }

        private void HandleIncorrectAttempt()
        {
            Debug.WriteLine("Highlight undo button!");
            HighlightStrokeAsError(TargetTrace.Clone(), currentStrokeIndex);
            UndoButton.Foreground = new SolidColorBrush(Colors.Red);
            incorrectAttempt = true;
            StudentEditCanvas.IsEnabled = false;
        }

        private void HighlightNextStroke()
        {
            HighlightStroke(TargetTrace.Clone(), currentStrokeIndex + 1);
        }

        private void EnableSubmitButton()
        {
            SubmitButton.IsEnabled = true;
            StudentEditCanvas.IsEnabled = false;
        }

        private void DisableSubmitButton()
        {
            SubmitButton.IsEnabled = false;
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
                toHighlight.DrawingAttributes.Color = Colors.Purple;
                strokeCollection.Add(toHighlight);      //add as last element so appears on top of all other targetTrace strokes           
            } else
            {
                Debug.WriteLine("highlight stroke, strokeIndex out of bounds");
            }
            CanvasBG.Strokes.Add(strokeCollection);
            HighlightStrokeStartingPoint(toHighlight);
        }

        private void HighlightStrokeAsError(StrokeCollection strokeCollection, int strokeIndex)
        {
            CanvasBG.Strokes.Clear();
            Stroke toHighlight = null;

            if (strokeCollection.Count > strokeIndex)
            {
                toHighlight = strokeCollection[strokeIndex];
                strokeCollection.RemoveAt(strokeIndex);                
                toHighlight.DrawingAttributes.Color = Colors.Orange;
                strokeCollection.Add(toHighlight);      //add as last element so appears on top of all other targetTrace strokes           
            }
            else
            {
                Debug.WriteLine("highlight stroke, strokeIndex out of bounds");
            }
            CanvasBG.Strokes.Add(strokeCollection);            
        }


        private void SubmitTrace(object sender, RoutedEventArgs e)
        {                                    
            // Navigate to batched analytics view and transfer traces
            if(currentRepetition == ExerciseItem.repetitionAmount)
            {
                SubmitLastRepetition();
            } else
            {
                StoreCurrentRepetition();
                MoveToNextRepetition();
            }            
        }

        private void SubmitLastRepetition()
        {
            StudentEditCanvas.IsEnabled = false;
            StudentTraceUtils = new TraceUtils(StudentEditCanvas.Strokes.Clone());
            studentTraceUtilsSet.Add(new TraceUtils(StudentEditCanvas.Strokes.Clone()));

            var inputData = new BFInputData
            {
                StudentTraceUtilsSet = studentTraceUtilsSet,
                StudentTraceUtils = StudentTraceUtils,
                ExpertTraceUtils = ExpertTraceUtils,
                ExpertOutline = ExpertTraceUtils.Trace
            };

            var destination = new BatchedAnalytics_EDM(inputData, loadedEDMData);
            NavigationService.Navigate(destination);
        }

        private void StoreCurrentRepetition()
        {
            studentTraceUtilsSet.Add(new TraceUtils(StudentEditCanvas.Strokes.Clone()));
        }

        private void MoveToNextRepetition()
        {
            currentRepetition++;
            ResetCanvas();
            StudentEditCanvas.IsEnabled = true;
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

  
        private void ResetCanvas()
        {
            StudentEditCanvas.Strokes.Clear();
            currentStrokeIndex = 0;
            RedrawHelperLines();            
            HighlightStroke(TargetTrace.Clone(), currentStrokeIndex);
            SetRepetitionText();
        }

        public void ClearCanvasButton(object sender, RoutedEventArgs e)
        {
            ResetCanvas();
        }

        public void UndoCanvasButton(object sender, RoutedEventArgs e)
        {
            CanvasBG.Strokes.Clear();
            CanvasBG.Strokes = TargetTrace.Clone();

            if (StudentEditCanvas.Strokes.Count > 0)
            {
                if (StudentEditCanvas.Strokes.Count == currentStrokeIndex)
                {
                    currentStrokeIndex--;
                }
                                    
                StudentEditCanvas.Strokes.RemoveAt(StudentEditCanvas.Strokes.Count - 1);

                if (StudentEditCanvas.Strokes.Count == currentStrokeIndex)
                {
                    incorrectAttempt = false;
                    HighlightStroke(TargetTrace.Clone(), currentStrokeIndex);
                } else
                {
                    HighlightStrokeAsError(TargetTrace.Clone(), currentStrokeIndex);//highlight stroke as error
                }                

                UndoButton.Background = Brushes.White; //set back to white incase it was highlighted
                UndoButton.Foreground = Brushes.Black;
            }
            if (StudentEditCanvas.Strokes.Count == 0)
            {
                UndoButton.IsEnabled = false;
            }
            StudentEditCanvas.IsEnabled = true;
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

