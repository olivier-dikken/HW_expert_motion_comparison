using HandwritingFeedback.Config;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace HandwritingFeedback.StylusPlugins.Strokes
{
    /// <summary>
    /// Customizes data saved in dry ink.
    /// </summary>
    public class GeneralStroke : Stroke
    {
        /// <summary>
        /// The final stylus point collections with the associated time-stamps in the current <br />
        /// stroke are cached here. <br />
        /// After completion of a stroke this cache is cleared <br />
        /// to make room for the containers of the next stroke.
        /// </summary>
        public static readonly IList<(StylusPointCollection stylusPointCollection, long timestamp)> TemporalCache =
            new List<(StylusPointCollection, long)>();

        /// <summary>
        /// Each instance of a GeneralStroke stores the state of the global cache
        /// (<see cref="TemporalCache"/>) when used for the first time. <br />
        /// This local copy allows for clearing of the global cache when a stroke is completed, <br />
        /// without losing the cached containers for this stroke.
        /// Whenever a GeneralStroke is rendered it uses the containers in this local cache.
        /// Each item in the cache associates a collection of stylus points with an event time-stamp.
        /// </summary>
        public (StylusPointCollection stylusPointCollection, long timestamp)[] TemporalCacheSnapshot { get; set; }

        /// <summary>
        /// Constructor for an instance of General Stroke.
        /// </summary>
        /// <param name="stylusPoints">Stylus point collection which forms the general stroke</param>
        public GeneralStroke(StylusPointCollection stylusPoints) : base(stylusPoints)
        {
        }

        /// <summary>
        /// This method and its DrawingContext parameter are not testable by default,
        /// so it calls the public DrawCore equivalent.
        /// </summary>
        /// <param name="drawingContext">Used to draw shapes to the canvas</param>
        /// <param name="drawingAttributes">Can specify pen attributes (not used for our purposes)</param>
        protected override void DrawCore(DrawingContext drawingContext,
                                         DrawingAttributes drawingAttributes)
        {
            CustomDrawCore();

            // If current stroke is an instance of student stroke,
            // do not render default ink canvas stroke because
            // custom rendering will be required to provide feedback
            if (!(this is StudentStroke))
            {
                // Invoke basic drying of ink if in expert mode
                base.DrawCore(drawingContext, drawingAttributes);
            }
        }

        /// <summary>
        /// Processes and renders newly arriving stylus points as dry ink to the canvas. <br />
        /// Invoked whenever an instance of GeneralStroke is rendered. <br />
        /// The first time this happens is when a stroke is completed (when the stylus is lifted).
        /// </summary>
        public void CustomDrawCore()
        {
            // If the stroke is used for the first time,
            // copy TemporalCache to this stroke's local cache
            if (TemporalCacheSnapshot == null)
            {
                // We need atomic access for clearing TemporalCache,
                // because it is shared across threads
                Monitor.Enter(TemporalCache);

                TemporalCacheSnapshot = new (StylusPointCollection, long)[TemporalCache.Count];
                TemporalCache.CopyTo(TemporalCacheSnapshot, 0);

                // Clear cache for the next stroke
                TemporalCache.Clear();

                // Release the lock, allowing other threads to use the resource
                Monitor.Exit(TemporalCache);
            }

            // Convert temporal cache to json serialized string and store as custom stroke property
            string jsonTemporalCache = JsonConvert.SerializeObject(TemporalCacheSnapshot);
            AddPropertyData(StrokePropertyIds.TemporalCache, jsonTemporalCache);
        }
    }
}
