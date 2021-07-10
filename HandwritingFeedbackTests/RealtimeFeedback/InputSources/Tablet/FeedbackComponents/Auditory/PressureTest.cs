using System;
using System.Windows.Input;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Auditory;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Auditory
{
    class AuditoryPressureTest : AuditoryComponentTest
    {
        protected override AuditoryComponent AuditoryComponent => new Pressure();

        private StylusPoint _stylusPoint;

        protected override void ConfigureComponentAttributes()
        {
            ApplicationConfig.HighPressureCutoff = 0.3f;
            ApplicationConfig.HighPressureStart = 0.1f;
            ApplicationConfig.LowPressureCutoff = 0.4f;
            ApplicationConfig.LowPressureStart = 0.2f;
        }

        public override void InternalSetUp()
        {
            _stylusPoint = new StylusPoint(0, 0, 0.5f);
            
            SineProvider.Frequency = 300;
        }
        
        [Test]
        public void AttributesNullTest()
        {
            ApplicationConfig.AuditoryConfig.PressureConfig = null;
            Assert.Throws<InvalidOperationException>(
                () => Run(_stylusPoint));
        }
        
        [Test]
        public void MaxFrequencyTest()
        {
            _stylusPoint.PressureFactor = 1f;
            Run(_stylusPoint);
            Assert.LessOrEqual(SineProvider.Frequency, 1200);
        }
        
        [Test]
        public void LowFrequencyTest()
        {
            _stylusPoint.PressureFactor = 0f;
            Run(_stylusPoint);
            Assert.AreEqual(SineProvider.Frequency, 150);
        }
        
        [Test]
        public void GoodPressureTest()
        {
            _stylusPoint.PressureFactor = 0.5f;
            Run(_stylusPoint);
            Assert.AreEqual(0, SineProvider.Frequency);
        }
        
        [Test]
        public void OnLowerBoundaryStartPressureTest()
        {
            _stylusPoint.PressureFactor = 0.5f - ApplicationConfig.LowPressureStart;
            Run(_stylusPoint);
            Assert.AreEqual(0, SineProvider.Frequency);
        }
        
        [Test]
        public void OnUpperBoundaryStartPressureTest()
        {
            _stylusPoint.PressureFactor = 0.5f + ApplicationConfig.HighPressureStart - 0.01f;
            Run(_stylusPoint);
            Assert.AreEqual(0, SineProvider.Frequency);
        }
        
        [Test]
        public void AboveUpperBoundaryStartPressureTest()
        {
            _stylusPoint.PressureFactor = 0.5f + ApplicationConfig.HighPressureStart;
            Run(_stylusPoint);
            Assert.AreEqual(600d, SineProvider.Frequency, 0.001d);
        }
        
        [Test]
        public void DefaultVolumeTest()
        {
            Assert.True(SineProvider.Volume.Equals(0.25f));
        }
        
        [Test]
        public void ContinuousPressureTest()
        {
            _stylusPoint.PressureFactor = 1f;
            Run(_stylusPoint);
            var previousFrequency = SineProvider.Frequency;
            _stylusPoint.PressureFactor = 1f;
            Run(_stylusPoint);
            Assert.AreEqual(previousFrequency, SineProvider.Frequency);
        }
        
        [Test]
        public void PlayOnceEqualPressureTest()
        {
            _stylusPoint.PressureFactor = 1f;
            Run(_stylusPoint);
            var previousFrequency = SineProvider.Frequency;
            _stylusPoint.PressureFactor = 1f;
            Run(_stylusPoint);
            Assert.AreEqual(previousFrequency, SineProvider.Frequency);
        }
        
        [Test]
        public void ChangePressureTest()
        {
            _stylusPoint.PressureFactor = 1f;
            Run(_stylusPoint);
            var previousFrequency = SineProvider.Frequency;
            _stylusPoint.PressureFactor = 0.2f;
            Run(_stylusPoint);
            Assert.AreNotEqual(previousFrequency, SineProvider.Frequency);
        }
        
        [Test]
        public void IncreasingFrequencyTest()
        {
            _stylusPoint.PressureFactor = 0f;
            Run(_stylusPoint);
            var previousFrequency = SineProvider.Frequency;
            _stylusPoint.PressureFactor = 0.2f;
            Run(_stylusPoint);
            Assert.Less(previousFrequency, SineProvider.Frequency);
        }
    }
}