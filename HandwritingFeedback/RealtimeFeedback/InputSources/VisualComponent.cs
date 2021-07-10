using System.Windows.Media;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.RealtimeFeedback.InputSources
{
    public abstract class VisualComponent : FeedbackComponent
    {
        /// <summary>
        /// Provides visual feedback by modifying the pen based on the data in input.
        /// </summary>
        /// <param name="pen">The pen to modify</param>
        /// <param name="input">The data to provide feedback on</param>
        public abstract void Synthesize(Pen pen, RTFInputData input);
    }
}