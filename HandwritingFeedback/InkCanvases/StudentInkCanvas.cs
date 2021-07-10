using System.Windows.Controls;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.StylusPlugins.Renderers;
using HandwritingFeedback.StylusPlugins.Strokes;

namespace HandwritingFeedback.InkCanvases
{
    /// <summary>
    /// Provides a custom InkCanvas to be used in practice mode, which can be used to modify the dry ink.
    /// </summary>
    public class StudentInkCanvas : InkCanvas
    {
        /// <summary>
        /// Constructor for an instance of Student Ink Canvas.
        /// </summary>
        public StudentInkCanvas() : base()
        {
            // Use the custom dynamic renderer on the student InkCanvas.
            this.DynamicRenderer = new StudentDynamicRenderer();
        }

        /// <summary>
        /// Invoked when the user tries to erase a stroke.
        /// </summary>
        /// <param name="e">The erase event</param>
        protected override void OnStrokeErasing(InkCanvasStrokeErasingEventArgs e)
        {
            // We cancel any erase attempt because it would
            // interfere greatly with feedback and stroke analysis
            e.Cancel = true;
        }

        /// <summary>
        /// Invoked when the user lifts their stylus. Dry ink processing begins here.
        /// </summary>
        /// <param name="e">Event arguments, including the collected stroke</param>
        public void CustomOnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            // Stop audio given based on pen pressure
            AuditoryFeedback.GetInstance().Player?.Pause();
            
            // Remove the original stroke and add a custom stroke.
            Strokes.Remove(e.Stroke);
            StudentStroke studentStroke = new StudentStroke(e.Stroke.StylusPoints);
            Strokes.Add(studentStroke);

            // Pass the custom stroke to base class' OnStrokeCollected method.
            InkCanvasStrokeCollectedEventArgs args =
                new InkCanvasStrokeCollectedEventArgs(studentStroke);
            base.OnStrokeCollected(args);
        }
        
        /// <summary>
        /// This method is not testable due to its accessibility level,
        /// so it calls the public CustomOnStrokeCollected equivalent
        /// which can be accessed and therefore tested.
        /// </summary>
        /// <param name="e">TEvent arguments, including the collected stroke</param>
        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            CustomOnStrokeCollected(e);
        }
    }
}