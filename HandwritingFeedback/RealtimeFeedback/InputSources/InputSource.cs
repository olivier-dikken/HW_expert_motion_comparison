using System.Collections.Generic;
using System.Windows.Media;
using HandwritingFeedback.Util;
using NAudio.Wave;

namespace HandwritingFeedback.RealtimeFeedback.InputSources
{
    /// <summary>
    /// Abstract class for defining a source of input data. <br />
    /// Hardware that can generate its own data is considered
    /// an input source.<br />
    /// Newly added sensors should extend from this class.
    /// </summary>
    public abstract class InputSource
    {
        // A source has one or more FeedbackComponents.
        // The order of components affects their priority!
        // The Synthesize methods for all components are invoked in list-order,
        // therefore the last component has the highest priority and can
        // override all changes made to the data in previous Synthesize calls.
        public abstract IList<FeedbackComponent> Components { get; set; }

        /// <summary>
        /// Collects visual feedback from all Components attached to this Source
        /// in order to modify the given pen.
        /// </summary>
        /// <param name="pen">The pen to modify</param>
        /// <param name="input">Input data for providing feedback</param>
        public virtual void CollectVisual(Pen pen, RTFInputData input)
        {
            foreach (FeedbackComponent component in Components)
            {
                if (component is VisualComponent visualComponent) {
                    visualComponent.Synthesize(pen, input);
                }
            }
        }

        /// <summary>
        /// Collects auditory feedback from all Components attached to this Source
        /// in order to modify the player.
        /// </summary>
        /// <param name="wavePlayer">The player that plays audio</param>
        /// <param name="sineWaveProvider">The player to modify</param>
        /// <param name="input">Input data for providing feedback</param>
        public virtual void CollectAuditory(IWavePlayer wavePlayer,SineWaveProvider sineWaveProvider, RTFInputData input)
        {
            foreach (FeedbackComponent component in Components)
            {
                if (component is AuditoryComponent auditoryComponent) {
                    auditoryComponent.Synthesize(sineWaveProvider, input);
                    wavePlayer?.Play();
                }
            }
        }
    }
}