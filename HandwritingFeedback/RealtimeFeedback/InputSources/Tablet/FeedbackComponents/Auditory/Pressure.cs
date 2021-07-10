using System;
using HandwritingFeedback.Config;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;

namespace HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Auditory
{
    /// <summary>
    /// Used by the tablet input source to generate real-time
    /// auditory feedback based on pressure.
    /// </summary>
    public class Pressure : AuditoryComponent
    {
        /// <summary>
        /// Algorithm for real-time auditory feedback based on pressure that changes the frequency of the sine wave.
        /// </summary>
        /// <param name="sineWaveProvider">The sine wave source to modify</param>
        /// <param name="input">The data to provide feedback on</param>
        /// <exception cref="InvalidOperationException">Thrown when the
        /// <see cref="Config.Auditory.SpeedConfig"/> is null</exception>
        public override void Synthesize(SineWaveProvider sineWaveProvider, RTFInputData input)
        {
            var pressureConfig = ApplicationConfig.Instance.AuditoryConfig.PressureConfig;

            if (pressureConfig == null)
                throw new InvalidOperationException("Attributes for this component are not set.");

            // The ratio will dictate how far to advance the color gradient for
            // either the low or high pressure color.
            double ratio = RealtimeFeedback.PressureRatio(PracticeMode.ExpertTraceUtils, input.StylusPoint);
            
            // Student's pen pressure is close to expert's.
            if (ratio == 0)
            {
                // No audio will be played
                sineWaveProvider.PortamentoTime = 0;
                sineWaveProvider.Frequency = 0;
                return;
            }

            sineWaveProvider.PortamentoTime =
                ApplicationConfig.Instance.AuditoryConfig.PressureConfig.PortamentoTime; 
            
            // W.r.t. the expert's pressure:
            // Higher pressure results in a positive ratio and thus a higher frequency.
            // Lower pressure results in a negative ratio and thus a lower frequency.
            ratio = Math.Clamp(ratio, -1, 1);
            // We need a high base frequency for high pressure, and a low base frequency for low pressure
            double baseFrequency = ratio >= 0d ? 600d : 300d;
            // Highest possible frequency: 2 * baseFrequency (octave up),
            // lowest possible frequency: 0.5 * baseFrequency (octave down).
            double newFrequency = ratio >= 0d ? baseFrequency * (1d + ratio) : baseFrequency * (1d + 0.5d * ratio);
            
            if (sineWaveProvider.Frequency != newFrequency)
            {
                sineWaveProvider.Frequency = newFrequency;
            }
        }
    }
}
