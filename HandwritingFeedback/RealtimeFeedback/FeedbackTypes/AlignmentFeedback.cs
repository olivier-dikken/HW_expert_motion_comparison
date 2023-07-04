using System;
using System.Collections.Generic;
using System.Text;

namespace HandwritingFeedback.RealtimeFeedback.FeedbackTypes
{
    /// <summary>
    /// Singleton class
    /// Checks alignment in real time setting
    /// Checks if a stroke is a serious attempt (or accidental stroke)
    /// </summary>
    public class AlignmentFeedback : RealtimeFeedback
    {

        private static AlignmentFeedback _instance;

        /// <summary>
        /// Getter for the class instance, which makes this class a Singleton.
        /// </summary>
        /// <returns>The instance of the class</returns>
        public static AlignmentFeedback GetInstance()
        {
            _instance ??= new AlignmentFeedback();
            return _instance;
        }
    }
}
