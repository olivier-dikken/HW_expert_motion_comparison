using HandwritingFeedback.Config;
using HandwritingFeedback.Models;
using HandwritingFeedback.Templates;
using HandwritingFeedback.Util;
using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for CreateTargetTraceView.xaml
    /// </summary>
    public partial class CreateTargetTraceView : Page, IMenuHeaderControls
    {
        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }

        // The exercise data object that will be passed to the next page
        private ExerciseData exerciseData;

        // Variable to store the expert's target trace strokes
        private StrokeCollection targetTraceStrokes;

        int helperLineType;
        int lineSpacing;

        public ICommand SubmitCanvasButtonCommand { get; }

        public CreateTargetTraceView(ExerciseData exerciseData)
        {
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
            this.exerciseData = exerciseData;

            // Initialize the command to submit the target trace
            SubmitCanvasButtonCommand = new AnotherCommandImplementation(SubmitCanvasButton);

            this.DataContext = this;
            
            helperLineType = this.exerciseData.LineType;
            lineSpacing = this.exerciseData.LineSpacing;
            
            InitializeComponent();

            ExpertEditCanvas.DefaultStylusPointDescription =
               ApplicationConfig.Instance.StylusPointDescription;

            // Attach the Loaded event handler, otherwise drawing the helper lines does not work
            Loaded += (sender, e) => RedrawHelperLines();
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

        private void ButtonSwitchLines(object sender, RoutedEventArgs e)
        {
            helperLineType = (helperLineType + 1) % 4;
            this.exerciseData.LineType = helperLineType;
            RedrawHelperLines();
        }

        private void ButtonLineIntervalIncrease(object sender, RoutedEventArgs e)
        {
            lineSpacing += 2;
            this.exerciseData.LineSpacing = lineSpacing;
            RedrawHelperLines();
        }

        private void ButtonLineIntervalDecrease(object sender, RoutedEventArgs e)
        {
            if (lineSpacing < 4)
                return;
            lineSpacing -= 2;
            this.exerciseData.LineSpacing = lineSpacing;
            RedrawHelperLines();
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
            ExpertEditCanvas.Reset();
        }


        public void ClearCanvasButton(object sender, RoutedEventArgs e)
        {
            ExpertEditCanvas.Reset();
            RedrawHelperLines();            
        }

        public void UndoCanvasButton(object sender, RoutedEventArgs e)
        {
            if(ExpertEditCanvas.Strokes.Count > 0)
            {
                ExpertEditCanvas.Strokes.RemoveAt(ExpertEditCanvas.Strokes.Count - 1);
            }                
        }

        /// <summary>
        /// confirm to save pop up dialog. Doesnt work yet.
        /// </summary>
        private async void ExecuteRunDialog()
        {
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ConfirmationDialog { DataContext = null};

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", null, ClosingEventHandler);            

            Debug.WriteLine("Dialog confirmed: " + view.GetStatus().ToString());
            //check the result...
            Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
            => Debug.WriteLine("You can intercept the closing event, and cancel here.");


        public async void SubmitCanvasButton(object parameter)
        {
            // Save the expert's target trace
            targetTraceStrokes = ExpertEditCanvas.Strokes.Clone();

            try
            {
                // Save the target trace strokes to the ExerciseData object
                this.exerciseData.TargetTraceStrokes = targetTraceStrokes;

                // Save helper line type and spacing to ExerciseData object
                this.exerciseData.LineType = helperLineType;
                this.exerciseData.LineSpacing = lineSpacing;

                //save exercise data to file
                await FileHandler.WriteExerciseData_Async(this.exerciseData);
                FileHandler.SaveTargetTrace(targetTraceStrokes, this.exerciseData.Path);

                // Save target trace as .png for card thumbnail display
                FileHandler.SaveCanvasAsImage(ExpertEditCanvas, this.exerciseData.Path, "TargetTrace");

                // Navigate to the exercise config screen
                NavigateToCreateEDMView();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Trying to create exercise folder failed. Please contact admin. " + ex.Message);
                this.NavigationService.Navigate(new Uri("\\View\\UpdatedUI\\ManageLearningContent.xaml", UriKind.Relative));
            }
        }


        private void NavigateToCreateEDMView()
        {
            // Navigates to the CreateEDMView page with the updated ExerciseData
            this.NavigationService.Navigate(new CreateEDMView(this.exerciseData));
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
