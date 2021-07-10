namespace HandwritingFeedback.BatchedFeedback.SynthesisTypes
{
    /// <summary>
    /// Synthesis representing a unit value and properties required to automatically generate unit value visualization (i.e. single value such as average).
    /// </summary>
    public class UnitValue : Synthesis
    {
        // Value of instance (i.e. result of synthesizing batched feedback component)
        public string Value { get; private set; }

        // Unit of value (e.g. N, Pa, ms, %, etc.)
        public string Unit { get; set; } = "";

        /// <summary>
        /// Constructor for unit value synthesis.
        /// </summary>
        /// <param name="unitValue">Value to be visualized on view</param>
        public UnitValue(string unitValue)
        {
            this.Value = unitValue;
        }
    }
}
