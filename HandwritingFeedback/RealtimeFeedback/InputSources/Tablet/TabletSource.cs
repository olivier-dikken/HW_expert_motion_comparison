using System.Collections.Generic;
using System.Windows;
using HandwritingFeedback.Config;
using HandwritingFeedback.Config.Auditory;
using HandwritingFeedback.Config.Visual;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Auditory;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Visual;
using Pressure = HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Visual.Pressure;
using PressureConfig = HandwritingFeedback.Config.Visual.PressureConfig;

namespace HandwritingFeedback.RealtimeFeedback.InputSources.Tablet
{
    /// <summary>
    /// Governs feedback components that make use of data
    /// coming from a tablet (and stylus)
    /// </summary>
    public class TabletSource : InputSource
    {
        // The list of components attached to the source instance,
        // sorted from lowest priority to highest priority
        public override IList<FeedbackComponent> Components { get; set; }

        /// <summary>
        /// Constructor for a tablet source instance.
        /// </summary>
        public TabletSource()
        {
            Components = new List<FeedbackComponent>();

            // Obtain current real-time feedback configuration from global view
            var currentSettings = (FeedbackType)Application.Current.Properties[ApplicationPropertyKeys.RealtimeSetting];
            var visualConfig = ApplicationConfig.Instance.VisualConfig;
            var auditoryConfig = ApplicationConfig.Instance.AuditoryConfig;
            
            // Depending on whether each real-time feedback component has been enabled, add the component to the components list
            // Add any further tablet source components below
            if (currentSettings.HasFlag(FeedbackType.PressureColor))
            {
                Components.Add(new Pressure());
                visualConfig.PressureConfig ??= new PressureConfig();
            }
            else
            {
                visualConfig.PressureConfig = null;
            }
            
            
            if (currentSettings.HasFlag(FeedbackType.PenTiltColor))
            {
                Components.Add(new Tilt());
                visualConfig.TiltConfig ??= new TiltConfig();
            }
            else
            {
                visualConfig.TiltConfig = null;
            }

            if (currentSettings.HasFlag(FeedbackType.AccuracyColor))
            {
                Components.Add(new Accuracy());
                visualConfig.AccuracyConfig ??= new AccuracyConfig();
            }
            else
            {
                visualConfig.AccuracyConfig = null;
            }
            
            if (currentSettings.HasFlag(FeedbackType.PressureAudio))
            {
                Components.Add(new FeedbackComponents.Auditory.Pressure());
                auditoryConfig.PressureConfig ??= new Config.Auditory.PressureConfig();
            }
            else
            {
                auditoryConfig.PressureConfig = null;
            }
            
            if (currentSettings.HasFlag(FeedbackType.SpeedAudio))
            {
                Components.Add(new Speed());
                auditoryConfig.SpeedConfig ??= new SpeedConfig();
            }
            else
            {
                auditoryConfig.SpeedConfig = null;
            }
        }
    }
}
