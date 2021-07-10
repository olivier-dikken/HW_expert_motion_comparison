using HandwritingFeedback.Util;

namespace HandwritingFeedback.RealtimeFeedback.InputSources
{
    public abstract class AuditoryComponent : FeedbackComponent
    {
        /// <summary>
        /// Provides auditory feedback by modifying the frequency of the sine wave.
        /// </summary>
        /// <param name="sineWaveProvider">The player to modify</param>
        /// <param name="input">The data to provide feedback on</param>
        public abstract void Synthesize(SineWaveProvider sineWaveProvider, RTFInputData input);
    }
}