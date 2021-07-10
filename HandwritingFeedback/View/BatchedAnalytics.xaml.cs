using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using HandwritingFeedback.BatchedFeedback;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for BatchedAnalytics.xaml
    /// </summary>
    public partial class BatchedAnalytics : Page
    {
        private readonly TraceUtils _expertTraceUtils;
        private readonly TraceUtils _studentTraceUtils;
        private readonly StrokeCollection _expertOutline;

        /// <summary>
        /// Constructor for batched analytics view.
        /// </summary>
        /// <param name="input">Data transferred from previous page</param>
        public BatchedAnalytics(BFInputData input)
        {
            InitializeComponent();

            // Extract student and expert trace from input
            _expertTraceUtils = input.ExpertTraceUtils;
            _studentTraceUtils = input.StudentTraceUtils;
            _expertOutline = input.ExpertOutline;
            
            inkCanvas.Strokes = _expertOutline.Clone();
            inkCanvas.Strokes.Add(_expertTraceUtils.Trace.Clone());
            inkCanvas.Strokes.Add(_studentTraceUtils.Trace);

            // Populate unit and graphing docks
            input.UnitValueDock = unitValueDock;
            input.GraphDock = graphDock;

            var manager = new BFViewManager(input);
            manager.PopulateDocks();
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
        /// Opens file explorer to save student trace.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void SaveStudentTrace(object sender, RoutedEventArgs e)
        {
            var result = new InkCanvas { Strokes = _studentTraceUtils.Trace };
            FileHandler.ButtonSaveAsClick(sender, e, result);
        }

        /// <summary>
        /// Restarts exercise by loading the previous expert model in practice mode,
        /// and navigates to the practice mode.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void Restart(object sender, RoutedEventArgs e)
        {
            // Add expert trace to be sent to practice mode
            var inputData = new BFInputData
            {
                ExpertTraceUtils = _expertTraceUtils,
                ExpertOutline = _expertOutline
            };

            // Navigate to next page and send package
            var destination = new PracticeMode(inputData);
            NavigationService.Navigate(destination);
        }

        private void SaveAllBatchedFeedback(object sender, RoutedEventArgs e)
        {
            FileHandler.ButtonSaveAsImageClick(sender, e, MainGrid);
        }
    }
}
