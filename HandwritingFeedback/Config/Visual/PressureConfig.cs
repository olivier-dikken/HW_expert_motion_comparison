using System.Windows.Media;

namespace HandwritingFeedback.Config.Visual
{
    /// <summary>
    /// Contains the attributes related to real-time visual pressure feedback.
    /// </summary>
    public class PressureConfig
    {
        /// <summary>
        /// Color used to draw the line when user applies too much pressure.
        /// </summary>
        public Color HighPressureColor = Colors.Orange;

        /// <summary>
        /// Color used to draw the line when user applies too little pressure.
        /// </summary>
        public Color LowPressureColor = Color.FromRgb(176, 0, 252); // Bright purple
    }
}