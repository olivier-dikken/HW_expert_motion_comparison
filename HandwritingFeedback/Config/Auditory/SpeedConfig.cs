namespace HandwritingFeedback.Config.Auditory
{
    /// <summary>
    /// Contains the attributes related to real-time auditory speed feedback.
    /// </summary>
    public class SpeedConfig
    {
        /// <summary>
        /// The frequency of the sine wave emitted when the speed of the user exceeds <br />
        /// the threshold determined by <see cref="ApplicationConfig.HighSpeedStart"/>.
        /// This value has to be greater than or equal to zero.
        /// </summary>
        public int HighSpeedFrequency = 2500;
    }
}