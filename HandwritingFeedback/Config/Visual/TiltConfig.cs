using System.Windows.Media;

namespace HandwritingFeedback.Config.Visual
{
    /// <summary>
    /// Contains the attributes related to real-time tilt feedback. <br />
    /// The colors used in this configuration were chosen to be color blind friendly.
    /// </summary>
    public class TiltConfig
    {
        /// <summary>
        /// Color used to draw the line when the user's angle is too high compared to the expert.
        /// </summary>
        public Color HighTiltColor = Color.FromRgb(255, 192, 255);
        
        /// <summary>
        /// Color used to draw the line when the user's angle is too low compared to the expert.
        /// </summary>
        public Color LowTiltColor = Color.FromRgb(133, 192, 249);
    }
}