using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using HandwritingFeedback.Config;
using HandwritingFeedback.Models;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for PracticeMode.xaml
    /// </summary>
    public partial class PracticeMode : Page
    {
        public static TraceUtils ExpertTraceUtils { get; set; }
        public static TraceUtils StudentTraceUtils { get; private set; }
        public static StrokeCollection ExpertOutline = new StrokeCollection();

        int helperLineType;

        EDMData loadedEDMData;

        public static StrokeCollection TargetTrace;
        
        /// <summary>
        /// Constructor for practice mode view in general case.
        /// </summary>
        public PracticeMode()
        {
            InitializeComponent();
            Load();
            
            StudentCanvas.DefaultStylusPointDescription =
                ApplicationConfig.Instance.StylusPointDescription;

            // After the first stroke is completed, the submit button will be enabled
            StudentCanvas.StrokeCollected += EnableSubmission;
            
            // Clear the canvas of previous ink
            StudentCanvas.Strokes.Clear();

            //TODO clean code...
            ExpertCanvas.Strokes.Add(TargetTrace.Clone());
            StudentCanvas.IsEnabled = true;

            //draw exercise helper lines
            TraceUtils.DrawHelperLines(StudentCanvas, GlobalState.exercisesToHelperLineType[GlobalState.SelectedExercise]);
        }

        /// <summary>
        /// Instantiates singletons to prevent lag when touching the canvas for the first time
        /// </summary>
        private void Load()
        {
            //get current exercise and set title
            var TitleTextBlock = (TextBlock)this.FindName("TitleTextBlock");
            TitleTextBlock.Text = "Exercise: " + Path.GetFileName(GlobalState.SelectedExercisePath);            

            // Add all singleton classes with expensive constructors here
            VisualFeedback.GetInstance();
            AuditoryFeedback.GetInstance();

            ReadConfigFile();
        }
        
        /// <summary>
        /// read exercise config and load exercise
        /// </summary>
        private void ReadConfigFile()
        {            
            string[] lines = System.IO.File.ReadAllLines(GlobalState.SelectedExercisePath + "\\exerciseConfig.txt");

            string description = lines[0];
            helperLineType = Int32.Parse(lines[lines.Length - 2]);

            TargetTrace = FileHandler.LoadStrokeCollection(GlobalState.SelectedExercisePath + "\\TargetTrace.isf");
            ExpertTraceUtils = new TraceUtils(TargetTrace);

            loadedEDMData = ExpertDistributionModel.LoadFromFile(GlobalState.SelectedExercisePath + "\\EDMData");
            Debug.WriteLine($"number of loaded datapoints: {loadedEDMData.dataPoints.Length}");
        }

        /// <summary>
        /// Constructor for practice mode view when restarting an exercise and an expert trace is in memory.
        /// </summary>
        /// <param name="input">Data transferred from previous page.</param>
        public PracticeMode(BFInputData input) : this()
        {
            // Extract the expert's trace and its background trace from the given data
            ExpertTraceUtils = input.ExpertTraceUtils;
            ExpertOutline = input.ExpertOutline;

            // First place the outline
            ExpertCanvas.Strokes = ExpertOutline.Clone();

            // The expert's trace is placed on top of the outline
            ExpertCanvas.Strokes.Add(ExpertTraceUtils.Trace.Clone());                        

            // We are restarting an exercise when this method is called,
            // so the expert's trace is on the canvas and the student may
            // start writing.
            StudentCanvas.IsEnabled = true;
            
        }

        

        /// <summary>
        /// Separates the student and expert trace from each other and propagates
        /// these to the batched analytics view for summative feedback. This method
        /// then navigates to the batched analytics view.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void SubmitTrace(object sender, RoutedEventArgs e)
        {
            // The student should not be able to write after clicking submit
            StudentCanvas.IsEnabled = false;
            
            StudentTraceUtils = new TraceUtils(StudentCanvas.Strokes.Clone());

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

        private void CompareAlignment(object sender, RoutedEventArgs e)
        {
            // The student should not be able to write after clicking submit
            StudentCanvas.IsEnabled = false;

            StudentTraceUtils = new TraceUtils(StudentCanvas.Strokes.Clone());

            // Add student and expert traces to be sent to batched analytics
            var inputData = new BFInputData
            {
                StudentTraceUtils = StudentTraceUtils,
                ExpertTraceUtils = ExpertTraceUtils,
                ExpertOutline = ExpertOutline
            };

            // Navigate to batched analytics view and transfer traces
            var destination = new CompareAlignment(inputData);            
            NavigationService.Navigate(destination);
        }

        /// <summary>
        /// First tries to load the expert's trace, then creates an
        /// outline of the trace that gets placed behind the main
        /// expert trace on the canvas.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void ButtonLoad_ClickAsync(object sender, RoutedEventArgs e)
        {
            // If the file was loaded successfully, we can proceed
            if (FileHandler.ButtonLoadClick(sender, e, ExpertCanvas))
            {
                // The .ISF was loaded, so the canvas currently contains the expert's
                // trace, which gets saved here.
                ExpertTraceUtils = new TraceUtils(ExpertCanvas.Strokes.Clone());

                // Modify the color of the expert's trace
                // This will allow the expert's trace to be darker than
                // the outline.
                foreach (var stroke in ExpertTraceUtils.Trace)
                {
                    stroke.DrawingAttributes.Color = Color.FromRgb(200, 200, 200);
                }
                
                // The existing expert's trace on the canvas gets transformed into
                // a thicker outline here.
                foreach (var stroke in ExpertCanvas.Strokes)
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
                ExpertOutline = ExpertCanvas.Strokes.Clone();
                
                // Add the expert's trace on top of the outline
                ExpertCanvas.Strokes.Add(ExpertTraceUtils.Trace);
                
                StudentCanvas.Strokes.Clear();
                StudentCanvas.IsEnabled = true;
                SubmitButton.IsEnabled = false;

                //draw helper lines
                TraceUtils.DrawHelperLines(StudentCanvas, GlobalState.exercisesToHelperLineType[GlobalState.SelectedExercise]);
            }
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
            if (!SubmitButton.IsEnabled && ExpertTraceUtils.Trace.Count != 0)
            {
                SubmitButton.IsEnabled = true;
            }
        }
    }
}
