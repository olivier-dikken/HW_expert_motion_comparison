using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.StylusPlugins.Wrappers;

namespace HandwritingFeedback.StylusPlugins.Renderers
{
    /// <summary>
    /// This class contains code to modify the color of wet ink strokes,
    /// based on different handwriting attributes (pressure, accuracy, etc.).
    /// </summary>
    class StudentDynamicRenderer : GeneralDynamicRenderer
    {
        /// <summary>
        /// This method and its DrawingContext parameter are not testable by default,
        /// so it calls the public OnDraw equivalent with a wrapped DrawingContext
        /// which can be mocked.
        /// </summary>
        /// <param name="drawingContext">Used to draw shapes to the canvas</param>
        /// <param name="stylusPoints">New stylus points received since the last update</param>
        /// <param name="geometry">Objects to be used for hit-testing, rendering 2D data, etc.</param>
        /// <param name="fillBrush">The brush selected by the User</param>
        protected override void OnDraw(DrawingContext drawingContext,
                                       StylusPointCollection stylusPoints,
                                       Geometry geometry, Brush fillBrush)
        {
            DrawingContextWrapper.WrappedInstance = drawingContext;
            OnDraw(DrawingContextWrapper, stylusPoints, geometry, fillBrush);
            base.OnDraw(drawingContext, stylusPoints, geometry, fillBrush);
        }

        /// <summary>
        /// This is the point where data from the TabletSource first arrives.
        /// Processes and renders newly arriving stylus points as wet ink in real-time.
        /// Invoked when new input from Stylus/Touch/Mouse is received.
        /// </summary>
        /// <param name="drawingContext">Used to draw shapes to the canvas</param>
        /// <param name="stylusPoints">New stylus points received since the last update</param>
        /// <param name="geometry">Objects to be used for hit-testing, rendering 2D data, etc.</param>
        /// <param name="fillBrush">The brush selected by the User</param>
        public void OnDraw(DrawingContextWrapper drawingContext,
            StylusPointCollection stylusPoints,
            Geometry geometry, Brush fillBrush)
        {

            // Add method calls for different feedback types used
            // by the TabletSource (e.g. visual, auditory, haptic, etc.) below
            VisualFeedback.GetInstance().Feedback(drawingContext, stylusPoints);
            //AuditoryFeedback.GetInstance().Feedback(AuditoryFeedback.GetInstance()?.Player, stylusPoints);
        }
    }
}
