using HandwritingFeedback.BatchedFeedback.SynthesisTypes;

namespace HandwritingFeedback.BatchedFeedback.Components
{
    /// <summary>
    /// Abstract class represents a general batched feedback component (i.e. type of batched feedback) based on any input type.
    /// </summary>
    public abstract class BFComponent
    {
        // Container object for synthesis that component generates
        protected Synthesis Synthesis { get; set; }

        /// <summary>
        /// Method to be over-ridden that generates associated data of batched feedback component to be used for visualization.
        /// </summary>
        /// <returns>Data used to generate visualizations for associated type of batched feedback</returns>
        public abstract Synthesis Synthesize();
    }
}
