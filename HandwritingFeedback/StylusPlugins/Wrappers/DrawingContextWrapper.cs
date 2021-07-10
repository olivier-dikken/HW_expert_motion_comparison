using System.Windows;
using System.Windows.Media;

namespace HandwritingFeedback.StylusPlugins.Wrappers
{
    /// <summary>
    /// A wrapper for the <see cref="DrawingContext"/> Windows Ink API class. <br />
    /// Enables testing of methods that render ink to the canvas.
    /// </summary>
    public class DrawingContextWrapper
    {
        /// <summary>
        /// The <see cref="DrawingContext"/> being wrapped.
        /// </summary>
        public DrawingContext WrappedInstance;

        /// <summary>
        /// Calls the actual <see cref="DrawLine"/> method of the <see cref="WrappedInstance"/>.
        /// </summary>
        /// <param name="pen">The pen to draw with</param>
        /// <param name="point0">The point to start drawing from</param>
        /// <param name="point1">The point to draw to</param>
        public virtual void DrawLine(Pen pen, Point point0, Point point1)
        {
            WrappedInstance.DrawLine(pen, point0, point1);
        }

        /// <summary>
        /// Calls the actual <see cref="DrawLine"/> method of the <see cref="WrappedInstance"/>.
        /// </summary>
        /// <param name="brush">The brush to fill the ellipse with</param>
        /// <param name="pen">The pen used for drawing the ellipse outline</param>
        /// <param name="center">The location of the center of the ellipse</param>
        /// <param name="radiusX">The horizontal radius of the ellipse</param>
        /// <param name="radiusY">The vertical radius of the ellipse</param>
        public virtual void DrawEllipse(Brush brush, Pen pen,
            Point center, double radiusX, double radiusY)
        {
            WrappedInstance.DrawEllipse(brush, pen, center, radiusX, radiusY);
        }
    }
}