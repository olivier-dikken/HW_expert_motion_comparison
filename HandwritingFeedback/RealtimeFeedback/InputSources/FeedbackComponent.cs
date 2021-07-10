namespace HandwritingFeedback.RealtimeFeedback.InputSources
{
    /// <summary>
    /// Abstract class for defining a feedback component.<br />
    /// Each input Source can have multiple FeedbackComponents,
    /// e.g. pressure and fluency for a tablet. <br />
    /// Newly added feedback components should extend from this class.
    /// </summary>
    public abstract class FeedbackComponent
    {
    }
}
