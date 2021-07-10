using System.Windows.Media;

namespace HandwritingFeedback.Config.Visual
{
    /// <summary>
    /// Contains fields for each visual real-time feedback attribute.<br />
    /// A VisualConfig instance by default does not have any feedback component configs <br /> associated with it,
    /// but they can manually be instantiated and their properties modified.
    /// </summary>
    public class VisualConfig
    {
        #region Feedback Component Configs

        public PressureConfig PressureConfig;
        public AccuracyConfig AccuracyConfig;
        public TiltConfig TiltConfig;

        #endregion

        /// <summary>
        /// The default color of the pen.
        /// </summary>
        public Color DefaultColor = Colors.Black;

        /// <summary>
        /// The interval at which gaps are drawn. Gap length is equal to this interval.
        /// </summary>
        public double DashedLineGapLength = 10;

        /// <summary>
        /// The amount by which to multiply the pen's pressure value.
        /// </summary>
        public double PenThicknessModifier = 3d;
    }
}