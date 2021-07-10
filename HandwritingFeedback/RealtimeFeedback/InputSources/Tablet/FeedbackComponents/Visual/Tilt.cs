using System;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;

namespace HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Visual
{
    /// <summary>
    /// Used by the tablet input Source to generate real-time
    /// feedback based on pen angle.
    /// </summary>
    public class Tilt : VisualComponent
    {
        [ThreadStatic] private static SolidColorBrush _highTiltBrush;
        [ThreadStatic] private static SolidColorBrush _lowTiltBrush;
        
        /// <summary>
        /// Algorithm for real-time feedback based on tilt angle in the given point.
        /// </summary>
        /// <param name="pen">The pen to modify</param>
        /// <param name="input">The data to provide feedback on</param>
        public override void Synthesize(Pen pen, RTFInputData input)
        {
            var tiltConfig = ApplicationConfig.Instance.VisualConfig.TiltConfig;
            
            if (tiltConfig == null)
                throw new InvalidOperationException("Attributes for the the tilt component are not set!");
            
            // The brushes to use when the student's angle is too far away from the expert's angle
            _highTiltBrush ??= new SolidColorBrush(tiltConfig.HighTiltColor);
            _lowTiltBrush ??= new SolidColorBrush(tiltConfig.LowTiltColor);
            
            double ratio = RealtimeFeedback.TiltRatio(PracticeMode.ExpertTraceUtils, input.StylusPoint);
                
            // If ratio is 0, no visual feedback needs to be provided as user is within the maximum angle deviation from the expert's stylus tilt.
            if (ratio == 0) return;

            // We change the brush color based on the ratio
            pen.Brush = ratio < 0 ? _lowTiltBrush : _highTiltBrush;
        }
    }
}