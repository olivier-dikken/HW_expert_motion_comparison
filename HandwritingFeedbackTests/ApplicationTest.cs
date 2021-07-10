using System.Threading;
using System.Windows;
using System.Windows.Input;
using HandwritingFeedback.Config;
using HandwritingFeedback.Config.Auditory;
using HandwritingFeedback.Config.Visual;
using NUnit.Framework;

namespace HandwritingFeedbackTests
{
    /// <summary>
    /// Any test that uses the Application or its properties should extend from this class.
    /// Running in single-threaded apartment mode allows for testing UI elements.
    /// </summary>
    [Apartment(ApartmentState.STA)]
    public abstract class ApplicationTest
    {
        protected ApplicationConfig ApplicationConfig;

        [SetUp]
        public void SetUp()
        {
            // If current application instance is null, generate a new application environment for unit testing
            if (Application.Current == null)
            {
                new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
            }

            // Set test application settings
            Application.Current.Properties[ApplicationPropertyKeys.RealtimeSetting] = ~0;

            ApplicationConfig = new ApplicationConfig
            {
                StylusPointDescription = new StylusPointDescription(),
                VisualConfig = new VisualConfig
                {
                    PressureConfig = new HandwritingFeedback.Config.Visual.PressureConfig(),
                    AccuracyConfig = new HandwritingFeedback.Config.Visual.AccuracyConfig()
                },
                AuditoryConfig = new AuditoryConfig
                {
                    PressureConfig = new HandwritingFeedback.Config.Auditory.PressureConfig()
                }
            };

            ApplicationConfig.Instance = ApplicationConfig;
        }

        // Child classes override this to implement any additional setup that may be required
        [SetUp]
        public abstract void InternalSetUp();
    }
}
