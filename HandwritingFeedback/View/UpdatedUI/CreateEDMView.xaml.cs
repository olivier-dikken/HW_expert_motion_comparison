using HandwritingFeedback.Models;
using HandwritingFeedback.Util;
using System;
using System.Collections.Generic;
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

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for CreateEDMView.xaml
    /// </summary>
    public partial class CreateEDMView : Page, IMenuHeaderControls
    {
        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }

        EDMCreationHelpler _EDMCreationHelpler;
        ExerciseItem _currentExercise;
        
        public CreateEDMView()
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
            _currentExercise = ExerciseItem.FromExerciseConfigFile(GlobalState.CreateContentsPreviousFolder);
            
            this.DataContext = this;

            InitializeComponent();

            this._EDMCreationHelpler = new EDMCreationHelpler(_currentExercise, CanvasBG, ExpertEditCanvas, OverlayCanvas, this);
        }


        public void SubmitCanvasButton(object sender, RoutedEventArgs e)
        {
            _EDMCreationHelpler.SubmitNewSample();
            CanvasToolBar.Visibility = Visibility.Collapsed;
            CanvasToolBar.IsEnabled = false;

            ConfirmSampleButton.Visibility = Visibility.Visible;
            ConfirmSampleButton.IsEnabled = true;
            DiscardSampleSubmissionButton.Visibility = Visibility.Visible;
            DiscardSampleSubmissionButton.IsEnabled = true;
        }

        public void ReloadCanvasOnSizeChange(object sender, SizeChangedEventArgs e)
        {
            _EDMCreationHelpler.ReloadCanvas();
        }

        public void ConfirmSampleSubmissionButton(object sender, RoutedEventArgs e)
        {
            _EDMCreationHelpler.ConfirmNewSample();
            SubmitButton.IsEnabled = false;

            ConfirmSampleButton.Visibility = Visibility.Collapsed;
            ConfirmSampleButton.IsEnabled = false;
            DiscardSampleSubmissionButton.Visibility = Visibility.Collapsed;
            DiscardSampleSubmissionButton.IsEnabled = false;

            CanvasToolBar.Visibility = Visibility.Visible;
            CanvasToolBar.IsEnabled = true;
        }

        public void DiscardSampleButton(object sender, RoutedEventArgs e)
        {
            _EDMCreationHelpler.DiscardNewSample();
            SubmitButton.IsEnabled = false;

            ConfirmSampleButton.Visibility = Visibility.Collapsed;
            ConfirmSampleButton.IsEnabled = false;
            DiscardSampleSubmissionButton.Visibility = Visibility.Collapsed;
            DiscardSampleSubmissionButton.IsEnabled = false;

            CanvasToolBar.Visibility = Visibility.Visible;
            CanvasToolBar.IsEnabled = true;
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
            this._EDMCreationHelpler.ReloadCanvas();
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
    }
}
