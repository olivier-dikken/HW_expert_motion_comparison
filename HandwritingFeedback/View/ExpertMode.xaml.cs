using System.Windows;
using System.Windows.Controls;
using HandwritingFeedback.Config;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for ExpertMode.xaml
    /// </summary>
    public partial class ExpertMode : Page
    {
        public ExpertMode()
        {
            this.InitializeComponent();
            ExpertCanvas.DefaultStylusPointDescription =
                ApplicationConfig.Instance.StylusPointDescription;

            //Add background helper lines WIP
            TraceUtils.DrawHelperSquareGrid(ExpertCanvas);
        }

        /// <summary>
        /// Removes all strokes from the canvas.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            ExpertCanvas.Reset();
            SaveButton.IsEnabled = false;

            //Add background helper lines WIP
            TraceUtils.DrawHelperSquareGrid(ExpertCanvas);
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
        void EnableSave(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Canvas must contain at least 1 stroke.
            if (!SaveButton.IsEnabled && ExpertCanvas.Strokes.Count != 0)
            {
                SaveButton.IsEnabled = true;
            }
        }
    }
}