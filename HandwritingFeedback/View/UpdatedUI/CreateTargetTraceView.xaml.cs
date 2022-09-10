using HandwritingFeedback.Config;
using HandwritingFeedback.Models;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for CreateTargetTraceView.xaml
    /// </summary>
    public partial class CreateTargetTraceView : Page, IMenuHeaderControls
    {
        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }

        int helperLineType = 0;
        int lineSpacing = 20;

        public CreateTargetTraceView()
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

            this.DataContext = this;

            InitializeComponent();

            ExpertEditCanvas.DefaultStylusPointDescription =
                ApplicationConfig.Instance.StylusPointDescription;

            TraceUtils.DrawHelperLines(CanvasBG, helperLineType, lineSpacing);
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
            RedrawHelperLines();
        }

        private void ButtonLineIntervalIncrease(object sender, RoutedEventArgs e)
        {
            lineSpacing += 2;
            RedrawHelperLines();
        }

        private void ButtonLineIntervalDecrease(object sender, RoutedEventArgs e)
        {
            if (lineSpacing < 4)
                return;
            lineSpacing -= 2;
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
            //TODO
            throw new NotImplementedException();
        }

        public void UndoCanvasButton(object sender, RoutedEventArgs e)
        {
            //TODO
            throw new NotImplementedException();
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


        public async void SubmitCanvasButton(object sender, RoutedEventArgs e)
        {
            if (await ConfigureSaveFolder())
            {
                //navigate to the exercise config screen
                this.NavigationService.Navigate(new Uri("\\View\\UpdatedUI\\CreateExericseInfo.xaml", UriKind.Relative));
            } else //something went wrong, display error message and take back to teacher menu
            {
                Debug.WriteLine("Trying to create exercise folder failed. Please contact admin");
                this.NavigationService.Navigate(new Uri("\\View\\UpdatedUI\\ManageLearningContent.xaml", UriKind.Relative));
            }            
        }

        private async Task<bool> ConfigureSaveFolder()
        {
            String folderName = Guid.NewGuid().ToString();
            string workingDirectory = Environment.CurrentDirectory;
            string path = Directory.GetParent(workingDirectory).Parent.FullName + "\\SavedData\\Exercises\\" + folderName;

            Debug.WriteLine("working directory: " + workingDirectory);
            Debug.WriteLine("path to create folder at: " + path);

            if (!Directory.Exists(path))
            {
                GlobalState.CreateContentsPreviousFolder = path;
                Directory.CreateDirectory(path);
                //save trace to folder as TargetTrace.isf
                //FileHandler.SaveTargetTrace(RemoveOffsetFromStrokeCollection(ExpertCanvas.Strokes), path);
                FileHandler.SaveTargetTrace(ExpertEditCanvas.Strokes, path);

                //save target trace as .png for card thumbnail display
                FileHandler.SaveCanvasAsImage(ExpertEditCanvas, path, "TargetTrace");

                //create exercise config .txt file and write line type and spacing
                await FileHandler.WriteConfigTargetTraceView_Async(helperLineType, lineSpacing, path);

                return true;
            }
            else
            {
                Debug.WriteLine("Error, folder name already exists");
            }
            return false;
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
