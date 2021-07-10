using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.StylusPlugins.Wrappers;

namespace HandwritingFeedback.StylusPlugins.Strokes
{
    /// <summary>
    /// A class for rendering custom strokes of dry ink.
    /// </summary>
    public class StudentStroke : GeneralStroke
    {
        private readonly DrawingContextWrapper _drawingContextWrapper =
            new DrawingContextWrapper();
        
        /// <summary>
        /// The final tuples of stylus point and pen used for wet ink for each  <br />
        /// StylusPoint in the current stroke are cached here. <br />
        /// This cache is used for rendering dry ink without having to recompute <br />
        /// the color for each StylusPoint based on the enabled feedback. <br />
        /// After completion of a stroke this cache is cleared <br />
        /// to make room for the tuples of the next stroke.
        /// </summary>
        public static readonly IList<(StylusPoint stylusPoint, Pen pen)> PensCache =
            new List<(StylusPoint, Pen)>();

        /// <summary>
        /// Each instance of a StudentStroke stores the state of the global cache
        /// (<see cref="PensCache"/>) when used for the first time. <br />
        /// This local copy allows for clearing of the global cache when a stroke is completed, <br />
        /// without losing the cached tuples for this stroke.
        /// Whenever a StudentStroke is rendered it uses the tuples in this local cache.
        /// </summary>
        public (StylusPoint stylusPoint, Pen pen)[] CacheSnapshot;

        /// <summary>
        /// Constructor for an instance of Student Stroke.
        /// </summary>
        /// <param name="stylusPoints">Stylus point collection which forms the general stroke</param>
        public StudentStroke(StylusPointCollection stylusPoints) : base(stylusPoints)
        {
        }
        
        /// <summary>
        /// This method and its DrawingContext parameter are not testable by default,
        /// so it calls the public DrawCore equivalent with a wrapped DrawingContext
        /// which can be mocked.
        /// </summary>
        /// <param name="drawingContext">Used to draw shapes to the canvas</param>
        /// <param name="drawingAttributes">Can specify pen attributes (not used for our purposes)</param>
        protected override void DrawCore(DrawingContext drawingContext,
            DrawingAttributes drawingAttributes)
        {
            _drawingContextWrapper.WrappedInstance = drawingContext;
            DrawCore(_drawingContextWrapper);
            
            // Invoke the over-ridden method to ensure custom data is saved in the general stroke parent class
            base.DrawCore(drawingContext, drawingAttributes);
        }

        /// <summary>
        /// Processes and renders newly arriving stylus points as dry ink to the canvas. <br />
        /// Invoked whenever an instance of StudentStroke is rendered. <br />
        /// The first time this happens is when a stroke is completed (when the stylus is lifted).
        /// </summary>
        /// <param name="drawingContext">Used to draw shapes to the canvas</param>
        public void DrawCore(DrawingContextWrapper drawingContext)
        {
            // If the stroke is used for the first time,
            // copy PensCache to this stroke's local cache.
            if (CacheSnapshot == null)
            {
                // We need atomic access for clearing PensCache,
                // because it is shared across threads.
                Monitor.Enter(PensCache);

                // Equalize the number of points stored in the cache and local stylus collection
                // to avoid random ink movement when drying wet ink
                while (PensCache.Count > StylusPoints.Count)
                {
                    PensCache.RemoveAt(0);
                }

                CacheSnapshot = new (StylusPoint, Pen)[PensCache.Count];
                PensCache.CopyTo(CacheSnapshot, 0);

                // Clear cache for the next stroke
                PensCache.Clear();
            
                Monitor.Exit(PensCache);
            }

            // Drawing is not possible if no stylus points are given or no tuples were cached
            if (StylusPoints.Count == 0 || CacheSnapshot.Length == 0) return;

            // If we are at the beginning of the stroke, reinforce the first point
            // to ensure that dots and punctuation marks are visible.
            ReinforcePoint(StylusPoints[0],
                CacheSnapshot[0].pen, drawingContext);
            
            Point previousPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
            
            // Draw lines between the stylus points of this stroke, using the cached tuples
            for (int i = 0; i < StylusPoints.Count && i < CacheSnapshot.Length; i++)
            { 
                drawingContext.DrawLine(CacheSnapshot[i].pen,
                    previousPoint, StylusPoints[i].ToPoint());
                previousPoint = StylusPoints[i].ToPoint();
            }
        }

        /// <summary>
        /// Reinforces a point of a stroke by drawing an ellipse with <br />
        /// fixed thickness on top of it.
        /// Can be used to make dots and punctuation easier to see.
        /// </summary>
        /// <param name="firstPoint">The point in the stroke</param>
        /// <param name="firstPen">The pen used to draw the first point</param>
        /// <param name="drawingContext">Used to draw shapes to the canvas</param>
        public static void ReinforcePoint(StylusPoint firstPoint, Pen firstPen,
            DrawingContextWrapper drawingContext)
        {
            // Use fixed thickness that is clearly visible
            double ellipseThickness = 0.20f * ApplicationConfig.Instance.VisualConfig.PenThicknessModifier;

            // Draw reinforcement point
            // Reinforcement point should use the same brush as the first point
            drawingContext.DrawEllipse(firstPen.Brush, new Pen(firstPen.Brush, ellipseThickness),
                            firstPoint.ToPoint(), ellipseThickness, ellipseThickness);
        }
    }
}
