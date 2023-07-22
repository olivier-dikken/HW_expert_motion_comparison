using HandwritingFeedback.Models;
using HandwritingFeedback.Util;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for CreateEDMView.xaml
    /// </summary>
    public partial class CreateEDMView : Page, IMenuHeaderControls
    {
        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }

        private EDMCreationHelpler _EDMCreationHelper;
        private ExerciseData _currentExercise;

        public CreateEDMView(ExerciseData exerciseData)
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

            _currentExercise = exerciseData;

            this.DataContext = this;

            InitializeComponent();

            this._EDMCreationHelper = new EDMCreationHelpler(_currentExercise, CanvasBG, ExpertEditCanvas, OverlayCanvas, this);
        }


        private void SubmitCanvasButton_Click(object sender, RoutedEventArgs e)
        {
            _EDMCreationHelper.SubmitNewSample();
            DisableCanvasToolBar();

            ConfirmSampleButton.Visibility = Visibility.Visible;
            DiscardSampleSubmissionButton.Visibility = Visibility.Visible;
        }

        private void ConfirmSampleSubmissionButton_Click(object sender, RoutedEventArgs e)
        {
            _EDMCreationHelper.ConfirmNewSample();
            SubmitButton.IsEnabled = false;

            ConfirmSampleButton.Visibility = Visibility.Collapsed;
            DiscardSampleSubmissionButton.Visibility = Visibility.Collapsed;

            EnableCanvasToolBar();
        }

        private void DiscardSampleButton_Click(object sender, RoutedEventArgs e)
        {
            _EDMCreationHelper.DiscardNewSample();
            SubmitButton.IsEnabled = false;

            ConfirmSampleButton.Visibility = Visibility.Collapsed;
            DiscardSampleSubmissionButton.Visibility = Visibility.Collapsed;

            EnableCanvasToolBar();
        }

        private void ClearCanvasButton_Click(object sender, RoutedEventArgs e)
        {
            _EDMCreationHelper.ReloadCanvas();
        }

        private void UndoCanvasButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExpertEditCanvas.Strokes.Count > 0)
            {
                ExpertEditCanvas.Strokes.RemoveAt(ExpertEditCanvas.Strokes.Count - 1);
            }
        }

        public void ReloadCanvasOnSizeChange(object sender, SizeChangedEventArgs e)
        {
            _EDMCreationHelper.ReloadCanvas();
        }



        public void EnableSubmit(object sender, InkCanvasStrokeCollectedEventArgs e)
        {            
            SubmitButton.IsEnabled = true;
        }

        public void DisableSubmit()
        {
            SubmitButton.IsEnabled = false;
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
            this._EDMCreationHelper.ReloadCanvas();
        }

        public void UndoCanvasButton(object sender, RoutedEventArgs e)
        {
            if (ExpertEditCanvas.Strokes.Count > 0)
            {
                ExpertEditCanvas.Strokes.RemoveAt(ExpertEditCanvas.Strokes.Count - 1);
            }
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

        private void EnableCanvasToolBar()
        {
            CanvasToolBar.IsEnabled = true;
            CanvasToolBar.Visibility = Visibility.Visible;
        }

        private void DisableCanvasToolBar()
        {
            CanvasToolBar.IsEnabled = false;
            CanvasToolBar.Visibility = Visibility.Collapsed;
        }
    }
}
