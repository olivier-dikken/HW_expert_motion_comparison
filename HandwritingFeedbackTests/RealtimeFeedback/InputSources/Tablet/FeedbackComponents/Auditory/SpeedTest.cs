using System;
using System.Windows.Input;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Auditory;
using HandwritingFeedback.StylusPlugins.Renderers;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Auditory
{
    class SpeedTest : AuditoryComponentTest
    {
        protected override AuditoryComponent AuditoryComponent => _component;

        private AuditoryComponent _component;
        
        private StylusPoint _stylusPoint;

        public override void InternalSetUp()
        {
            _component = new Speed();
            GeneralDynamicRenderer.StrokeStartTime = 0;

            var previousPoint = new StylusPoint(0, 0);
            Run(previousPoint, timestamp: 0);
            
            _stylusPoint = new StylusPoint(0, 10);
        }
        
        [Test]
        public void AttributesNullTest()
        {
            ApplicationConfig.AuditoryConfig.SpeedConfig = null;
            Assert.Throws<InvalidOperationException>(
                () => Run(_stylusPoint));
        }

        protected override void ConfigureComponentAttributes()
        {
            ApplicationConfig.HighSpeedStart = 100;
            ApplicationConfig.AuditoryConfig.SpeedConfig.HighSpeedFrequency = 2500;
        }

        [Test]
        public void LowSpeedTest()
        {
            Run(_stylusPoint, 1000);
            Assert.AreEqual(0, SineProvider.Frequency);
        }

        [Test]
        public void LowSpeedBoundaryTest()
        {
            Run(_stylusPoint, 100);
            Assert.AreEqual(0, SineProvider.Frequency);
        }

        [Test]
        public void HighSpeedTest()
        {
            Run(_stylusPoint, 1);
            Assert.AreEqual(2500, SineProvider.Frequency);
        }

        [Test]
        public void HighSpeedBoundaryTest()
        {
            Run(_stylusPoint, 99);
            Assert.AreEqual(2500, SineProvider.Frequency);
        }

        [Test]
        public void AverageSpeedTest()
        {
            Run(_stylusPoint, 1000);

            var nextStylusPoint = new StylusPoint(0, 20);
            Run(nextStylusPoint, 1001);
            
            Assert.AreEqual(2500, SineProvider.Frequency);
        }

        [Test]
        public void NewStrokeSpeedTest()
        {
            Run(_stylusPoint, 1);
            Assert.AreEqual(2500,SineProvider.Frequency);
            
            // Run speed component on new stroke, which will reset frequency
            var nextStylusPoint = new StylusPoint(0, 0);
            Run(nextStylusPoint, 0);
            Assert.AreEqual(0,SineProvider.Frequency);
            
            Run(_stylusPoint, 1);
            Assert.AreEqual(2500,SineProvider.Frequency);
        }
    }
}