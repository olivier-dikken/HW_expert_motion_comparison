using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.Config;
using HandwritingFeedback.Config.Auditory;
using HandwritingFeedback.Config.Visual;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;
using NUnit.Framework;
using PressureConfig = HandwritingFeedback.Config.Visual.PressureConfig;

namespace HandwritingFeedbackTests.RealtimeFeedback.InputSources
{
    /// <summary>
    /// Any test for a VisualComponent of an InputSource extends this class.
    /// </summary>
    abstract class AuditoryComponentTest : ApplicationTest
    {
        protected abstract AuditoryComponent AuditoryComponent { get; }
        
        protected readonly SineWaveProvider SineProvider = new SineWaveProvider();

        /// <summary>
        /// Use this method to configure the FeedbackComponent that the child class is testing. <br />
        /// </summary>
        protected abstract void ConfigureComponentAttributes();

        [SetUp]
        public void FeedbackComponentSetUp()
        {
            // Set a config with set custom values to ensure that tests don't
            // start failing when the developer changes the values in the ApplicationConfig.
            ApplicationConfig = new ApplicationConfig { 
                VisualConfig = new VisualConfig 
                {
                    PressureConfig = new PressureConfig()
                }, 
                AuditoryConfig = new AuditoryConfig
                {
                    PressureConfig = new HandwritingFeedback.Config.Auditory.PressureConfig(),
                    SpeedConfig =  new SpeedConfig()
                }};
            
            // Configure the component's attributes
            ConfigureComponentAttributes();

            ApplicationConfig.Instance = ApplicationConfig;

            PracticeMode.ExpertTraceUtils = new TraceUtils(new StrokeCollection());
            
            // Add a default expert trace
            StylusPoint start = new StylusPoint(0, 5, 0.5f);
            StylusPoint end = new StylusPoint(4, 5, 0.5f);
            AddTraceFromStylusPoints(new StylusPointCollection{ start, end });
        }

        /// <summary>
        /// Takes a StylusPointCollection and adds it to the ExpertTrace collection
        /// as a Stroke. Used for testing.
        /// </summary>
        /// <param name="stylusPoints">The given StylusPointCollection</param>
        private void AddTraceFromStylusPoints(StylusPointCollection stylusPoints)
        {
            Stroke stroke = new Stroke(stylusPoints);
            PracticeMode.ExpertTraceUtils.Trace.Add(stroke);
        }

        /// <summary>
        /// Calls the Synthesize method in <see cref="AuditoryComponent"/>.
        /// </summary>
        protected void Run(StylusPoint stylusPoint, long timestamp = 0)
        {
            var input = new RTFInputData {StylusPoint = stylusPoint, Timestamp =  timestamp};
            AuditoryComponent.Synthesize(SineProvider, input);
        }
    }
}
