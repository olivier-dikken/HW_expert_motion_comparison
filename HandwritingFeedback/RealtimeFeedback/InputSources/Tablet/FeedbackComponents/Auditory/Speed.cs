using System;
using System.Windows;
using System.Windows.Input;
using HandwritingFeedback.Config;
using HandwritingFeedback.StylusPlugins.Renderers;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Auditory
{
    /// <summary>
    /// Used by the tablet input source to generate real-time
    /// auditory feedback based on speed.
    /// </summary>
    public class Speed : AuditoryComponent
    {
        /// <summary>
        /// Cumulative distance covered after last stylus point for which speed was calculated.
        /// </summary>
        private double _distance = 0;
        
        /// <summary>
        /// Timestamp of previous stylus point which was processed by speed component.
        /// </summary>
        private long _previousTimestamp = -1;
        
        /// <summary>
        /// Previous stylus point which was processed by speed component.
        /// </summary>
        private StylusPoint _previousPoint = 
            new StylusPoint(double.NegativeInfinity, double.NegativeInfinity);
        
        /// <summary>
        /// Last speed value that was computed in real-time for the current exercise.
        /// </summary>
        private double _previousSpeed = -1;

        /// <summary>
        /// Algorithm for real-time auditory feedback based on speed that changes the frequency of the sine wave.
        /// Feedback is only provided when the user is writing too fast compared to the configured threshold, never
        /// when the user is writing too slow, in order to minimize cognitive load.
        /// </summary>
        /// <param name="sineWaveProvider">The sine wave source to modify</param>
        /// <param name="input">The data to provide feedback on</param>
        /// <exception cref="InvalidOperationException">Thrown when the
        /// <see cref="Config.Auditory.SpeedConfig"/> is null</exception>
        public override void Synthesize(SineWaveProvider sineWaveProvider, RTFInputData input)
        {
            var speedConfig = ApplicationConfig.Instance.AuditoryConfig.SpeedConfig;
            
            if (speedConfig == null)
                throw new InvalidOperationException("Attributes for this component are not set.");
            
            // Compute time elapsed from the point where the user started the current stroke
            long relativeTime = input.Timestamp - GeneralDynamicRenderer.StrokeStartTime;
            
            // If a new stroke starts, the previous timestamp will be greater than the given one
            // (pen has been lifted), so we reset the speed calculation from this point
            if (_previousTimestamp > relativeTime || _previousTimestamp == -1)
            {
                _distance = 0;
                _previousTimestamp = relativeTime;
                _previousPoint = input.StylusPoint;

                sineWaveProvider.PortamentoTime = 0;
                sineWaveProvider.Frequency = 0;
                return;
            }

            _distance += Point.Subtract(input.StylusPoint.ToPoint(), _previousPoint.ToPoint()).Length;

            // We have reached a new batch of stylus points, within the same stroke
            // (pen hasn't been lifted)
            if (relativeTime > _previousTimestamp)
            {
                double delta = relativeTime - _previousTimestamp;
                delta /= 1000d;
                double rawSpeed = _distance / delta;
                
                // Cache raw speed in order to use for averaging speed for subsequent stylus point
                _previousSpeed = rawSpeed;

                // Average speed if previous speed is available, to obtain a less erratic speed value
                var speed = rawSpeed;
                if (_previousSpeed != -1) speed = (speed + _previousSpeed) / 2d;
                
                // Determine if the speed is above the configured threshold
                double ratio = RealtimeFeedback.Generate(speed, 0,
                    0, 0, ApplicationConfig.Instance.HighSpeedStart,
                    ApplicationConfig.Instance.HighSpeedStart);

                if (ratio == 0)
                {
                    // No audio will be played
                    sineWaveProvider.Frequency = 0;
                }
                else
                {
                    sineWaveProvider.PortamentoTime = 0.3;
                    sineWaveProvider.Frequency =
                        ApplicationConfig.Instance.AuditoryConfig.SpeedConfig.HighSpeedFrequency;
                }
                _distance = 0;
            }
            
            _previousTimestamp = relativeTime;
            _previousPoint = input.StylusPoint;
        }
    }
}
