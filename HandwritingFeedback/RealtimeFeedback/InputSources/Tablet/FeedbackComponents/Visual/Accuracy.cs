using System;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;

namespace HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Visual
{
    /// <summary>
    /// Used by the tablet source to generate real-time
    /// feedback based on accuracy (overlap)
    /// </summary>
    public class Accuracy : VisualComponent
    {

        [ThreadStatic] private static SolidColorBrush _deviationBrush;

        /// <summary>
        /// Algorithm for real-time feedback based on accuracy, <br />
        /// i.e. distance between current point and expert's trace.
        /// </summary>
        /// <param name="pen">The pen to be modified</param>
        /// <param name="input">Data for providing feedback</param>
        public override void Synthesize(Pen pen, RTFInputData input)
        {
            var accuracyConfig = ApplicationConfig.Instance.VisualConfig.AccuracyConfig;

            if (accuracyConfig == null)
                throw new InvalidOperationException("Attributes for the accuracy component are not set!");

            // The brush to use when the student is too far away from the expert
            _deviationBrush ??= new SolidColorBrush(accuracyConfig.DeviationColor);

            // The ratio will either be 0, in which case the student was on the expert's trace,
            // or some positive value, in which case the student was not on the expert's trace.
            double ratio = RealtimeFeedback.AccuracyRatio(PracticeMode.ExpertTraceUtils, input.StylusPoint);

            // If the ratio is 0, the student is within MaxDeviationRadius
            // and therefore on the expert's trace.
            if (ratio == 0) return;
            
            // Otherwise, we know the student is too far away and therefore not
            // on the expert's trace.
            // Use the _deviationBrush to indicate this to the student
            pen.Brush = _deviationBrush;
        }
    }
}
