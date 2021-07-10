using System;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.Config.Visual;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Visual;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Visual
{
    /// <summary>
    /// Test suite related to real-time visual feedback on accuracy.
    /// </summary>
    class AccuracyTest : VisualComponentTest
    {
        protected override VisualComponent VisualComponent => new Accuracy();

        public override void InternalSetUp() {}
        
        protected override void ConfigureComponentAttributes()
        {
            ApplicationConfig.MaxDeviationRadius = 10d;
            
            ApplicationConfig.VisualConfig.AccuracyConfig = new AccuracyConfig()
            {
                DeviationColor = Colors.Green
            };
        }

        [Test]
        public void AccuracyMissTest()
        {
            StylusPoint = new StylusPoint(40, 75);
            RunAndCompareEquals(Colors.Green, Pen, StylusPoint);
        }

        [Test]
        public void AccuracyMissBoundaryTest()
        {
            StylusPoint = new StylusPoint(4d, 15.01d);
            RunAndCompareEquals(Colors.Green, Pen, StylusPoint);
        }

        [Test]
        public void AccuracyHitBoundaryTest()
        {
            StylusPoint = new StylusPoint(4d, 15d);
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
        }

        [Test]
        public void AccuracyHitTest()
        {
            StylusPoint = new StylusPoint(2, 5);
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
        }

        [Test]
        public void AttributesNullTest()
        {
            ApplicationConfig.VisualConfig.AccuracyConfig = null;
            Assert.Throws<InvalidOperationException>(
                () => RunAndCompareEquals(Colors.Black, Pen, StylusPoint));
        }
    }
}
