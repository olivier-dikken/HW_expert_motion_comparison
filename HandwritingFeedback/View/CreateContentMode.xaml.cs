using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using HandwritingFeedback.Config;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for CreateContentMode.xaml
    /// </summary>
    public partial class CreateContentMode : Page
    {

        int helperLineType = 1;
        int lineSpacing = 20;


        public CreateContentMode()
        {
            InitializeComponent();

            ExpertCanvas.DefaultStylusPointDescription =
                ApplicationConfig.Instance.StylusPointDescription;


            TraceUtils.DrawHelperLines(ExpertCanvasBG, helperLineType, lineSpacing);
        }

        


        private void ButtonSwitchLines(object sender, RoutedEventArgs e)
        {
            helperLineType = (helperLineType + 1) % 4;
            ClearCanvas(sender, e);
        }

        private void ButtonLineIntervalIncrease(object sender, RoutedEventArgs e)
        {
            lineSpacing += 2;
            ClearCanvas(sender, e);
        }

        private void ButtonLineIntervalDecrease(object sender, RoutedEventArgs e)
        {
            if (lineSpacing < 4)
                return;
            lineSpacing -= 2;
            ClearCanvas(sender, e);
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

            //Add background helper lines WIP
            TraceUtils.DrawHelperLines(ExpertCanvasBG, helperLineType, lineSpacing);
        }

        /// <summary>
        /// Opens the file explorer and allows saving of the strokes on the canvas as ISF file.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void ButtonSave_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (FileHandler.ButtonSaveAsClick(sender, e, ExpertCanvas))
                ClearCanvas(sender, e);
        }

        private void ButtonNext(object sender, RoutedEventArgs e)
        {
            //create folder with filename
            string foldername = TextBoxFilename.Text;
            string workingDirectory = Environment.CurrentDirectory;
            string path = Directory.GetParent(workingDirectory).Parent.FullName + "\\SavedData\\Exercises\\" + foldername;

            Debug.WriteLine("working directory: " + workingDirectory);
            Debug.WriteLine("path to create folder at: " + path);

            if (!Directory.Exists(path))
            {
                GlobalState.CreateContentsPreviousFolder = path;
                Directory.CreateDirectory(path);
                //save trace to folder as TargetTrace.isf
                FileHandler.SaveTargetTrace(ExpertCanvas.Strokes, path);
                //navigate to the exercise config screen
                this.NavigationService.Navigate(new Uri("\\View\\CreateContentConfigMode.xaml", UriKind.Relative));                
            }
            else
            {
                Debug.WriteLine("Error, folder name already exists");
            }                        
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
        void EnableNext(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Canvas must contain at least 1 stroke.
            if (!NextButton.IsEnabled && ExpertCanvas.Strokes.Count != 0)
            {
                NextButton.IsEnabled = true;
            }
        }
    }
}
