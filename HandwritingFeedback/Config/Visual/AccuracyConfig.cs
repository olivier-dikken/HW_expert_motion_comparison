using System.Windows.Media;

namespace HandwritingFeedback.Config.Visual
{
    /// <summary>
    /// Contains the attributes related to real-time visual accuracy feedback.
    /// </summary>
    public class AccuracyConfig
    {
        /// <summary>
        /// Color used to draw the line when user deviates too far from the expert trace.
        /// </summary>
        public Color DeviationColor = Colors.Red;
    }
}