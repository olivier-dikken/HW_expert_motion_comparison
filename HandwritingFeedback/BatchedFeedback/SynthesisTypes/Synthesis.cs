namespace HandwritingFeedback.BatchedFeedback.SynthesisTypes
{
    /// <summary>
    /// Abstract container object used to visualize the computation of batched feedback components.
    /// </summary>
    public abstract class Synthesis
    {
        // Title of instance
        public string Title { get; set; }

        /// <summary>
        /// Base constructor for abstract synthesis.
        /// </summary>
        protected Synthesis()
        {
            this.Title = "";
        }
    }
}
