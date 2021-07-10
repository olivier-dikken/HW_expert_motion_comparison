using System.Windows.Controls;
using HandwritingFeedback.StylusPlugins.Renderers;
using HandwritingFeedback.StylusPlugins.Strokes;

namespace HandwritingFeedback.InkCanvases
{
    /// <summary>
    /// Provides a custom InkCanvas to be used in practice mode, which can be used to save custom data in dry ink.
    /// </summary>
    public class ExpertInkCanvas : InkCanvas
    {
        /// <summary>
        /// Constructor for instance of Expert Ink Canvas.
        /// </summary>
        public ExpertInkCanvas() : base()
        {
            // Use the custom dynamic renderer on the expert InkCanvas.
            DynamicRenderer = new GeneralDynamicRenderer();
        }

        /// <summary>
        /// Clears the strokes on the canvas and instantiates a new <see cref="GeneralDynamicRenderer"/>
        /// for this canvas
        /// </summary>
        public void Reset()
        {
            Strokes.Clear();
            ((GeneralDynamicRenderer) DynamicRenderer).ResetTimer();
        }

        /// <summary>
        /// Invoked when the user lifts their stylus. Dry ink processing begins here.
        /// </summary>
        /// <param name="e">Event arguments, including the collected stroke</param>
        public void CustomOnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            // Remove the original stroke and add a custom stroke.
            Strokes.Remove(e.Stroke);
            GeneralStroke stroke = new GeneralStroke(e.Stroke.StylusPoints);
            Strokes.Add(stroke);

            // Pass the custom stroke to base class' OnStrokeCollected method.
            InkCanvasStrokeCollectedEventArgs args =
                new InkCanvasStrokeCollectedEventArgs(stroke);
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
