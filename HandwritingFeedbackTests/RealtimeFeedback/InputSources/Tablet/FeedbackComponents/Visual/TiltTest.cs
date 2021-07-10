using System;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.Config.Visual;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Visual;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback.InputSources.Tablet.FeedbackComponents.Visual
{
    class TiltTest : VisualComponentTest
    {
        // DISCLAIMER. For this test file, the angles are going to be reversed. That is because how
        // we are using the angle in code (90 - provided value). That means that when we give the 
        // value 6000, we are actually testing for 3000 ~ 30 degrees.
        
        protected override VisualComponent VisualComponent => new Tilt();
        protected override void ConfigureComponentAttributes()
        {
            ApplicationConfig.AngleDeviation = 15;
            ApplicationConfig.VisualConfig.TiltConfig = new TiltConfig
            {
                HighTiltColor = Colors.Red,
                LowTiltColor = Colors.Blue
            };
            ApplicationConfig.StylusPointDescription = new StylusPointDescription(new[]
            {
                new StylusPointPropertyInfo(StylusPointProperties.X),
                new StylusPointPropertyInfo(StylusPointProperties.Y),
                new StylusPointPropertyInfo(StylusPointProperties.NormalPressure),
                // These are the default values for the XTiltOrientation but they are different
                // in the testing environment, so we set them to real world values
                new StylusPointPropertyInfo(StylusPointProperties.XTiltOrientation, -9000, 9000, StylusPointPropertyUnit.Degrees, 100)
            });
        }
        
        public override void InternalSetUp()
        {
            StylusPoint = new StylusPoint(42, 42, 0.5f, ApplicationConfig.StylusPointDescription, new[] {4500});
            
            // Add a default expert trace (with tilt information)
            // All expert points have a tilt of 45 degrees
            var start = new StylusPoint(43, 43, 0.5f, ApplicationConfig.StylusPointDescription, new[] {4500});
            var end = new StylusPoint(45, 45, 0.5f, ApplicationConfig.StylusPointDescription, new[] {4500});
            
            var stroke = new Stroke(new StylusPointCollection(new[] {start, end}));

            PracticeMode.ExpertTraceUtils = new TraceUtils(new StrokeCollection(new[] {stroke}));
        }

        [Test]
        public void NormalTiltTest()
        {
            StylusPoint.SetPropertyValue(StylusPointProperties.XTiltOrientation, 4500);
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
        }

        [Test]
        public void LowTiltTest()
        {
            // Testing for 29 degrees
            StylusPoint.SetPropertyValue(StylusPointProperties.XTiltOrientation, 6100);
            RunAndCompareEquals(Colors.Blue, Pen, StylusPoint);
        }

        [Test]
        public void ExtremelyLowTiltTest()
        {
            // Testing for 5 degrees
            StylusPoint.SetPropertyValue(StylusPointProperties.XTiltOrientation, 8500);
            RunAndCompareEquals(Colors.Blue, Pen, StylusPoint);
        }

        [Test]
        public void LowTiltStillWithinBounds()
        {
            // Testing for 30 degrees
            StylusPoint.SetPropertyValue(StylusPointProperties.XTiltOrientation, 6000);
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
        }

        [Test]
        public void HighTiltTest()
        {
            // Testing for 61 degrees
            StylusPoint.SetPropertyValue(StylusPointProperties.XTiltOrientation, 2900);
            RunAndCompareEquals(Colors.Red, Pen, StylusPoint);
        }
        
        [Test]
        public void HighTiltWithinBoundsTest()
        {
            // Testing for 60 degrees
            StylusPoint.SetPropertyValue(StylusPointProperties.XTiltOrientation, 3000);
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
        }

        [Test]
        public void ExtremelyHighTiltTest()
        {
            // Testing for 85 degrees
            StylusPoint.SetPropertyValue(StylusPointProperties.XTiltOrientation, 1500);
            RunAndCompareEquals(Colors.Red, Pen, StylusPoint);
        }

        [Test]
        public void AttributesNullTest()
        {
            ApplicationConfig.VisualConfig.TiltConfig = null;
            Assert.Throws<InvalidOperationException>(
                () => RunAndCompareEquals(Colors.Black, Pen, StylusPoint));
        }
    }
}