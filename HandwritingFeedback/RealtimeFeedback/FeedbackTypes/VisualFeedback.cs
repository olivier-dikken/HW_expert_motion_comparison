using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.StylusPlugins.Wrappers;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;
using HandwritingFeedback.View.UpdatedUI;

namespace HandwritingFeedback.RealtimeFeedback.FeedbackTypes
{
    /// <summary>
    /// Singleton class that contains code to modify the color of wet ink strokes,
    /// based on different handwriting attributes (pressure, accuracy, etc.).
    /// </summary>
    public class VisualFeedback : RealtimeFeedback
    {
        /// <summary>
        /// Brush used to draw with the default color.
        /// </summary>
        [ThreadStatic]
        private static SolidColorBrush _defaultBrush;

        /// <summary>
        /// Brush used to draw with an invisible color, used for drawing gaps in a dashed line.
        /// </summary>
        [ThreadStatic]
        private static SolidColorBrush _invisibleBrush;

        private static VisualFeedback _instance;

        /// <summary>
        /// Keeps track of length of stroke that is currently being drawn, used for drawing dashed lines.
        /// </summary>
        private static double _strokeLength;

        /// <summary>
        /// Getter for the class instance, which makes this class a Singleton.
        /// </summary>
        /// <returns>The instance of the class</returns>
        public static VisualFeedback GetInstance()
        {
            _instance ??= new VisualFeedback();
            return _instance;
        }

        /// <summary>
        /// Collects feedback, processes, and renders the given StylusPoints as wet ink. <br />
        /// Invoked when new input from Stylus/Touch/Mouse is received, <br />
        /// or when the stylus is lifted (dry ink).
        /// </summary>
        public void Feedback(DrawingContextWrapper drawingContext,
            StylusPointCollection stylusPoints)
        {
            // Create a new brush if it was previously null
            _defaultBrush ??= new SolidColorBrush(ApplicationConfig.Instance.VisualConfig.DefaultColor);
            _invisibleBrush ??= new SolidColorBrush(Colors.Transparent);

            // Allocate memory to store the previous point to draw from
            Point prevPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);

            // PensCache is used by different threads, such as the application thread
            // and the InkCanvas thread, therefore access to this resource needs to be synchronized.
            Monitor.Enter(StudentStroke.PensCache);

            int cacheCount = StudentStroke.PensCache.Count;

            // When the cache is empty a new stroke starts
            if (cacheCount == 0) _strokeLength = 0d;

            // Iterate over all stylus points and draw lines from point to point
            // The appearance of any drawn line is affected by the attributes of the current stylus point
            for (int i = 0; i < stylusPoints.Count; i++)
            {
                StylusPoint stylusPoint = stylusPoints[i];
                var pt = stylusPoint.ToPoint();
                var input = new RTFInputData
                {
                    StylusPoint = stylusPoint
                };

                if (double.IsFinite(prevPoint.X) && double.IsFinite(prevPoint.Y))
                    _strokeLength += Point.Subtract(pt, prevPoint).Length;

                float pressure = stylusPoints[i].PressureFactor;

                // Pen controls the thickness and color of the stroke
                // Set thickness based on pressure * 3
                var pen = new Pen(_defaultBrush,
                    pressure * 3 * ApplicationConfig.Instance.VisualConfig.PenThicknessModifier);

                // Calculate the closest expert point for the current student point.
                double distance = StudentPracticeView.ExpertTraceUtils.GetClosestPoint(pt).distance;

                // If a closest expert point was found calculate feedback
                if (double.IsFinite(distance))
                {
                    // Gather feedback from the FeedbackComponents of selected InputSources in order to update the Pen
                    foreach (InputSource source in InputSources)
                    {
                        source.CollectVisual(pen, input);
                    }
                }
                // Student is outside of scan diameter
                else 
                {
                    // Insert a gap at a fixed interval, making the line dashed
                    if (_strokeLength % (2 * ApplicationConfig.Instance.VisualConfig.DashedLineGapLength) > 
                        ApplicationConfig.Instance.VisualConfig.DashedLineGapLength)
                    {
                        // Make the brush invisible to draw a gap
                        pen.Brush = _invisibleBrush;
                    }
                    // Disable pressure sensitivity
                    pen.Thickness = 0.5d * ApplicationConfig.Instance.VisualConfig.PenThicknessModifier;
                }

                // Make the pen unmodifiable to allow for shared usage across threads
                pen.Freeze();

                // If we are at the beginning of the stroke, reinforce the first point
                // to ensure that dots and punctuation marks are visible.
                if (cacheCount == 0)
                    StudentStroke.ReinforcePoint(stylusPoints[0], pen, drawingContext);
                
                // The Windows Ink API includes the last point of the previously collected points
                // as the first point in the newly collected points.
                // We do not need to cache the tuple for this point again.
                if (cacheCount == 0 || i != 0)
                {
                    // Cache the tuple (used for dry ink rendering)
                    StudentStroke.PensCache.Add((stylusPoint, pen));
                }

                // Draw a line from the current point to the previous point
                // using the pen and its brush
                drawingContext.DrawLine(pen, prevPoint, pt);
                prevPoint = pt;
            }
            
            // Release the lock, allowing other threads to use the resource
            Monitor.Exit(StudentStroke.PensCache);
        }

        /// <summary>
        /// Scales the RGB (red, green, blue) values according to a scalar.
        /// </summary>
        /// <param name="c">Color for which values should be scaled</param>
        /// <param name="scalar">Scalar that values are multiplied by</param>
        /// <returns>A new color with scaled RGB values</returns>
        public static Color ScaleColor(Color c, double scalar)
        {
            // The ratio needs to be between 0 and 1, in order to
            // correctly scale the color gradient
            scalar = Math.Clamp(scalar, 0d, 1d);

            return Color.FromRgb(
                (byte)Math.Clamp(c.R * scalar, 0, 255),
                (byte)Math.Clamp(c.G * scalar, 0, 255),
                (byte)Math.Clamp(c.B * scalar, 0, 255));
        }

        /// <summary>
        /// Adds the RGB channels of two colors, without considering the alpha channel.
        /// </summary>
        /// <param name="color1">The first color</param>
        /// <param name="color2">The second color</param>
        /// <returns>A new color with each sum as resulting channel</returns>
        public static Color AddColorChannels(Color color1, Color color2)
        {
            return Color.FromRgb(
                (byte)Math.Clamp(color1.R + color2.R, 0, 255),
                (byte)Math.Clamp(color1.G + color2.G, 0, 255),
                (byte)Math.Clamp(color1.B + color2.B, 0, 255));
        }
    }
}
