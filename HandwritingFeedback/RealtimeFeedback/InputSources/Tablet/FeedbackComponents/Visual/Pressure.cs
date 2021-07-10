using System;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;

namespace HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Visual
{
    /// <summary>
    /// Used by the tablet input Source to generate real-time
    /// feedback based on pressure.
    /// </summary>
    public class Pressure : VisualComponent
    {

        [ThreadStatic] private static SolidColorBrush _highPressureBrush;

        [ThreadStatic] private static SolidColorBrush _lowPressureBrush;

        /// <summary>
        /// Algorithm for real-time feedback based on pressure in the given point.
        /// </summary>
        /// <param name="pen">The pen to modify</param>
        /// <param name="input">The data to provide feedback on</param>
        public override void Synthesize(Pen pen, RTFInputData input)
        {
            var pressureConfig = ApplicationConfig.Instance.VisualConfig.PressureConfig;
            
            if (pressureConfig == null)
                throw new InvalidOperationException("Attributes for this component are not set.");
            
            _highPressureBrush ??= new SolidColorBrush(pressureConfig.HighPressureColor);
            _lowPressureBrush ??= new SolidColorBrush(pressureConfig.LowPressureColor);
            
            // The ratio will dictate how far to advance the color gradient for
            // either the low or high pressure color.
            double ratio = RealtimeFeedback.PressureRatio(PracticeMode.ExpertTraceUtils, input.StylusPoint);
            
            // Clamp the ratio between -1 and 1, as we do not use any values beyond those limits
            ratio = Math.Clamp(ratio, -1d, 1d);

            // A ratio of 0 means that the pressure was within the acceptable range, so do not modify the pen
            if (ratio == 0d) return;

            // The previousBrush is required to calculate the final gradient
            SolidColorBrush previousBrush = (SolidColorBrush) pen.Brush;
            
            // A positive ratio means that the student's pressure is too high, whereas
            // a negative ratio means that the student's pressure is too low.
            SolidColorBrush feedbackBrush = ratio < 0 ? _lowPressureBrush : _highPressureBrush;

            // At this point, the ratio is nonzero and between -1 and 1
            
            // Make the ratio positive in case it was multiplied by -1
            // This happens when the student is below (expertPressure - lowPressureStart)
            ratio = Math.Abs(ratio);
            
            // If studentValue is beyond the cutoff, set the brush to feedbackBrush,
            // which will give the pen the maximum color (for either low or high pressure)
            if (ratio == 1d)
            {
                pen.Brush = feedbackBrush;
                return;
            }
            
            // Calculate the gradient color by multiplying the feedbackBrush color with
            // the ratio and adding this to the previousBrush color, which is multiplied by
            // the remainder of 1 - ratio.
            Color finalColor = VisualFeedback.AddColorChannels(
                VisualFeedback.ScaleColor(previousBrush.Color, 1d - ratio),
                VisualFeedback.ScaleColor(feedbackBrush.Color, ratio));
            pen.Brush = new SolidColorBrush(finalColor);
        }
    }
}
