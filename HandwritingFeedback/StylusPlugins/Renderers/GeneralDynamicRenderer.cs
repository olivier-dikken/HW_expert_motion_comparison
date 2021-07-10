using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.StylusPlugins.Wrappers;

namespace HandwritingFeedback.StylusPlugins.Renderers
{
    /// <summary>
    /// Dynamically saves custom data for both student and expert ink data.
    /// </summary>
    public class GeneralDynamicRenderer : DynamicRenderer
    {
        protected readonly DrawingContextWrapper DrawingContextWrapper =
            new DrawingContextWrapper();

        /// <summary>
        /// Records the time elapsed after the user first
        /// makes contact with the digitizer. Used to record
        /// timestamps for <see cref="RawStylusInput"/> received in <see cref="OnDraw"/>.
        /// </summary>
        protected static StopwatchWrapper StopwatchWrapper = new StopwatchWrapper();

        /// <summary>
        /// Time at which the first point in the most recent stroke was received
        /// </summary>
        public static long StrokeStartTime;

        /// <summary>
        /// Time at which the last point in the most recent stroke was received
        /// </summary>
        public long StrokeEndTime;

        /// <summary>
        /// Time at which the stylus first touched the canvas, after canvas initialization
        /// </summary>
        public long InputStartTime;

        /// <summary>
        /// Indicates whether stylus is currently touching the canvas
        /// </summary>
        public bool IsStylusDown;
        
        /// <summary>
        /// Constructor for <see cref="GeneralDynamicRenderer"/> invoked when a custom ink canvas is constructed
        /// </summary>
        public GeneralDynamicRenderer()
        {
            StopwatchWrapper.WrappedInstance = new Stopwatch();
            ResetTimer();
        }

        /// <summary>
        /// Constructor for <see cref="GeneralDynamicRenderer"/> used for testing to mock stopwatch related logic.
        /// </summary>
        /// <param name="wrapper">Stopwatch wrapper to replace the current wrapper with</param>
        public GeneralDynamicRenderer(StopwatchWrapper wrapper) : this()
        {
            StopwatchWrapper = wrapper;
            StopwatchWrapper.Start();
        }
        
        /// <summary>
        /// Get the number of elapsed milliseconds after the stopwatch was initially started
        /// </summary>
        /// <returns>Elapsed time in milliseconds</returns>
        public static long GetElapsedTimeInMilliseconds()
        {
            return StopwatchWrapper.GetElapsedMilliseconds();
        }
        
        /// <summary>
        /// This method and its DrawingContext parameter are not testable by default,
        /// so it calls the public OnDraw equivalent with a wrapped DrawingContext
        /// which can be mocked.
        /// </summary>
        /// <param name="drawingContext">Used to draw shapes to the canvas</param>
        /// <param name="stylusPoints">New stylus points received since the last update</param>
        /// <param name="geometry">Objects to be used for hit-testing, rendering 2D data, etc.</param>
        /// <param name="fillBrush">The brush selected by the User</param>
        protected override void OnDraw(DrawingContext drawingContext, StylusPointCollection stylusPoints, Geometry geometry, Brush fillBrush)
        {
            DrawingContextWrapper.WrappedInstance = drawingContext;
            CustomOnDraw(DrawingContextWrapper, stylusPoints, geometry, fillBrush);
            
            // Invoke the base OnDraw method to render wet ink to the screen
            // This should not be done for the StudentDynamicRenderer, as it implements
            // custom wet ink.
            if (!(this is StudentDynamicRenderer))
            {
                base.OnDraw(drawingContext, stylusPoints, geometry, fillBrush);
            }
        }

        /// <summary>
        /// This method is called from StudentDynamicRenderer.<see cref="StudentDynamicRenderer.OnDraw"/>
        /// It creates a temporal cache to store timestamps with newly arriving stylus points.
        /// The temporal cache is used for batched and real-time feedback involving the speed attribute.
        /// </summary>
        /// <param name="drawingContext">Used to draw shapes to the canvas</param>
        /// <param name="stylusPoints">New stylus points received since the last update</param>
        /// <param name="geometry">Objects to be used for hit-testing, rendering 2D data, etc.</param>
        /// <param name="fillBrush">The brush selected by the User</param>
        public void CustomOnDraw(DrawingContextWrapper drawingContext, StylusPointCollection stylusPoints, Geometry geometry, Brush fillBrush)
        {
            // When submitting an exercise immediately after lifting the stylus, a large delay can be invoked
            // as the application switches to the UI thread before returning to this method.
            // Therefore, the timestamp is no longer representative and the earlier saved stroke end time should
            // be used instead.
            // In either case subtract the input start time to determine the relative time the user took to generate
            // the given stylus points.
            long elapsedTime = (IsStylusDown ? StopwatchWrapper.GetElapsedMilliseconds() : StrokeEndTime) - InputStartTime;

            // The Windows Ink API includes the last point of the previously collected points
            // as the first point in the newly collected points.
            // Therefore, any valid input must have a previous point as well as at least one new point,
            // unless the first points in a new stroke are given for which no previous point exist.
            if (stylusPoints.Count < 2 && GeneralStroke.TemporalCache.Count > 0) return;

            // Temporal cache is used by multiple threads so access must be synchronized
            Monitor.Enter(GeneralStroke.TemporalCache);

            // Add the event time-stamp along with the associated stylus points to the cache
            // As mentioned above, the first point needs to be skipped as it is included
            // automatically by the Ink API (except for when a new stroke has started, in
            // which case the cache is empty)
            GeneralStroke.TemporalCache.Add(GeneralStroke.TemporalCache.Count == 0
                ? (stylusPoints, elapsedTime)
                : (new StylusPointCollection(stylusPoints.Skip(1)), elapsedTime));

            // Release the lock, allowing other threads to use the resource
            Monitor.Exit(GeneralStroke.TemporalCache);
        }

        /// <summary>
        /// Invoked when the stylus touches the digitizer.
        /// This method is not testable due to its accessibility level,
        /// so it calls the public <see cref="CustomOnStylusDown"/> equivalent
        /// which can be accessed and therefore tested.
        /// </summary>
        /// <param name="rawStylusInput">Raw data that contains information about input from the stylus</param>
        protected override void OnStylusDown(RawStylusInput rawStylusInput)
        {
            CustomOnStylusDown();
            base.OnStylusDown(rawStylusInput);
        }

        /// <summary>
        /// Responsible for starting the Stopwatch used for calculating
        /// Stroke timestamps.
        /// </summary>
        public void CustomOnStylusDown()
        {
            IsStylusDown = true;
            StrokeStartTime = StopwatchWrapper.GetElapsedMilliseconds();
            
            // If input start time has not been set, this method was invoked when the user first made contact
            // with the digitizer, so update the input start time
            if (InputStartTime == -1) InputStartTime = StrokeStartTime;
        }

        /// <summary>
        /// Invoked when the stylus is lifted from the digitizer.
        /// This method is not testable due to its accessibility level,
        /// so it calls the public <see cref="CustomOnStylusUp"/> equivalent
        /// which can be accessed and therefore tested.
        /// </summary>
        /// <param name="rawStylusInput">Raw data that contains information about input from the stylus</param>
        protected override void OnStylusUp(RawStylusInput rawStylusInput)
        {
            CustomOnStylusUp();
            base.OnStylusUp(rawStylusInput);
        }

        /// <summary>
        /// Responsible for updating <see cref="StrokeEndTime"/> and setting
        /// <see cref="IsStylusDown"/> to false.
        /// </summary>
        public void CustomOnStylusUp()
        {
            IsStylusDown = false;
            StrokeEndTime = StopwatchWrapper.GetElapsedMilliseconds();
        }

        /// <summary>
        /// Restarts the stopwatch of the <see cref="StopwatchWrapper"/> and
        /// resets the fields used to keep track of elapsed time.
        /// </summary>
        public void ResetTimer()
        {
            // Initialize new stopwatch to determine times at which stylus points were drawn
            StopwatchWrapper.Reset();
            StopwatchWrapper.Start();
            
            StrokeStartTime = -1;
            InputStartTime = -1;
            StrokeEndTime = -1;
            IsStylusDown = false;
        }
    }
}
