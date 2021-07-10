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
    /// Test suite related to real-time visual feedback on pressure.
    /// </summary>
    class PressureTest : VisualComponentTest
    {
        protected override VisualComponent VisualComponent => new Pressure();

        public override void InternalSetUp() {}
        
        protected override void ConfigureComponentAttributes()
        {
            ApplicationConfig.HighPressureCutoff = 0.3f;
            ApplicationConfig.HighPressureStart = 0.1f;
            ApplicationConfig.LowPressureCutoff = 0.4f;
            ApplicationConfig.LowPressureStart = 0.2f;
            
            ApplicationConfig.VisualConfig.PressureConfig = new PressureConfig()
            {
                HighPressureColor = Colors.Red,
                LowPressureColor = Colors.Blue,
            };
        }

        #region ExpertDefaultPressure

        [Test]
        public void NormalPressureTest()
        {
            StylusPoint.PressureFactor = 0.45f;
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
        }

        [Test]
        public void LowPressureEqualsCutoff()
        {
            ApplicationConfig.LowPressureCutoff = 0.35f;
            StylusPoint.PressureFactor = 0.15f;
            RunAndCompareEquals(Colors.Blue, Pen, StylusPoint);
        }

        [Test]
        public void LowPressureExceedsCutoff()
        {
            ApplicationConfig.LowPressureCutoff = 0.35f;
            StylusPoint.PressureFactor = 0.05f;
            RunAndCompareEquals(Colors.Blue, Pen, StylusPoint);
        }

        [Test]
        public void HighPressureEqualsCutoff()
        {
            ApplicationConfig.HighPressureCutoff = 0.35f;
            StylusPoint.PressureFactor = 0.85f;
            RunAndCompareEquals(Colors.Red, Pen, StylusPoint);
        }

        [Test]
        public void HighPressureExceedsCutoff()
        {
            ApplicationConfig.HighPressureCutoff = 0.35f;
            StylusPoint.PressureFactor = 1f;
            RunAndCompareEquals(Colors.Red, Pen, StylusPoint);
        }

        [Test]
        public void LowPressureWithStartEqualsCutoffTest()
        {
            ApplicationConfig.LowPressureStart = 0.2f;
            ApplicationConfig.LowPressureCutoff = 0.2f;
            StylusPoint.PressureFactor = 0.1f;
            RunAndCompareEquals(Colors.Blue, Pen, StylusPoint);
        }

        [Test]
        public void HighPressureWithStartEqualsCutoffTest()
        {
            ApplicationConfig.HighPressureStart = 0.3f;
            ApplicationConfig.HighPressureCutoff = 0.3f;
            StylusPoint.PressureFactor = 0.9f;
            RunAndCompareEquals(Colors.Red, Pen, StylusPoint);
        }

        [Test]
        public void LowPressureAtStartTest()
        {
            StylusPoint.PressureFactor = 0.3f;
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
        }

        [Test]
        public void LowPressureInbetweenTest()
        {
            StylusPoint.PressureFactor = 0.25f;
            RunAndCompareEquals(Color.FromRgb(0, 0, 63), Pen, StylusPoint);
        }

        [Test]
        public void HighPressureAtStartTest()
        {
            StylusPoint.PressureFactor = 0.6f;
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
        }

        [Test]
        public void HighPressureInbetweenTest()
        {
            StylusPoint.PressureFactor = 0.666666667f;
            RunAndCompareEquals(Color.FromRgb(85, 0, 0), Pen, StylusPoint);
        }

        // Test applying pressure feedback to a pen modified by previous feedback
        [Test]
        public void ChainFeedbackAcceptablePressureTest()
        {
            Color previousColor = Color.FromRgb(7, 42, 4);
            ((SolidColorBrush)Pen.Brush).Color = previousColor;
            StylusPoint.PressureFactor = 0.5f;
            RunAndCompareEquals(previousColor, Pen, StylusPoint);
        }
        
        [Test]
        public void ChainFeedbackHighPressureTest()
        {
            Color previousColor = Color.FromRgb(13, 37, 42);
            ((SolidColorBrush)Pen.Brush).Color = previousColor;
            StylusPoint.PressureFactor = 0.7f;
            RunAndCompareEquals(Color.FromRgb(133, 18, 21), Pen, StylusPoint);
        }
        
        [Test]
        public void ChainFeedbackLowPressureTest()
        {
            Color previousColor = Color.FromRgb(67, 34, 100);
            ((SolidColorBrush)Pen.Brush).Color = previousColor;
            StylusPoint.PressureFactor = 0.2f;
            RunAndCompareEquals(Color.FromRgb(33, 17, 177), Pen, StylusPoint);
        }
        
        #endregion

        #region ExpertHighPressure

        [Test]
        public void ExpertHighPressureMatchedByStudentTest()
        {
            StylusPoint expertPoint = new StylusPoint(0, 0, 0.8f);
            AddTraceFromStylusPoints(new StylusPointCollection{ expertPoint });
            StylusPoint.PressureFactor = 0.8f;
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
        }
        
        [Test]
        public void ExpertHighPressureStudentLowPressureTest()
        {
            StylusPoint expertPoint = new StylusPoint(0, 0, 0.8f);
            AddTraceFromStylusPoints(new StylusPointCollection{ expertPoint });
            StylusPoint.PressureFactor = 0.5f;
            RunAndCompareEquals(Color.FromRgb(0, 0, 127), Pen, StylusPoint);
        }

        [Test]
        public void ExpertHighPressureStudentHigherPressureTest()
        {
            StylusPoint expertPoint = new StylusPoint(0, 0, 0.8f);
            AddTraceFromStylusPoints(new StylusPointCollection{ expertPoint });
            StylusPoint.PressureFactor = 0.95f;
            RunAndCompareEquals(Color.FromRgb(63, 0, 0), Pen, StylusPoint);
        }

        [Test]
        public void ExpertHighPressureCutoffBeyondMaxTest()
        {
            StylusPoint expertPoint = new StylusPoint(0, 0, 0.85f);
            AddTraceFromStylusPoints(new StylusPointCollection{ expertPoint });
            StylusPoint.PressureFactor = 1f;
            RunAndCompareEquals(Color.FromRgb(63, 0, 0), Pen, StylusPoint);
        }
        
        [Test]
        public void ExpertAndStudentMaxPressureTest()
        {
            StylusPoint expertPoint = new StylusPoint(0, 0, 1f);
            AddTraceFromStylusPoints(new StylusPointCollection{ expertPoint });
            StylusPoint.PressureFactor = 1f;
            RunAndCompareEquals(Color.FromRgb(0, 0, 0), Pen, StylusPoint);
        }

        #endregion
        
        #region ExpertLowPressure
        
        [Test]
        public void ExpertLowPressureMatchedByStudentTest()
        {
            StylusPoint.PressureFactor = 0.3f;
            RunAndCompareEquals(Colors.Black, Pen, StylusPoint);
            
        }
        
        [Test]
        public void ExpertLowPressureStudentLowerPressureTest()
        {
            StylusPoint expertPoint = new StylusPoint(0, 0, 0.4f);
            AddTraceFromStylusPoints(new StylusPointCollection{ expertPoint });
            StylusPoint.PressureFactor = 0.1f;
            RunAndCompareEquals(Color.FromRgb(0, 0, 127), Pen, StylusPoint);
        }

        [Test]
        public void ExpertLowPressureStudentHighPressureTest()
        {
            StylusPoint expertPoint = new StylusPoint(0, 0, 0.4f);
            AddTraceFromStylusPoints(new StylusPointCollection{ expertPoint });
            StylusPoint.PressureFactor = 0.6f;
            RunAndCompareEquals(Color.FromRgb(127, 0, 0), Pen, StylusPoint);
        }

        [Test]
        public void ExpertLowPressureCutoffBeyondMinTest()
        {
            StylusPoint expertPoint = new StylusPoint(0, 0, 0.3f);
            AddTraceFromStylusPoints(new StylusPointCollection{ expertPoint });
            StylusPoint.PressureFactor = 0f;
            RunAndCompareEquals(Color.FromRgb(0, 0, 127), Pen, StylusPoint);
        }
        
        [Test]
        public void ExpertAndStudentMinPressureTest()
        {
            StylusPoint expertPoint = new StylusPoint(0, 0, 0.001f);
            AddTraceFromStylusPoints(new StylusPointCollection{ expertPoint });
            StylusPoint.PressureFactor = 0.001f;
            RunAndCompareEquals(Color.FromRgb(0, 0, 0), Pen, StylusPoint);
        }
        
        #endregion
        
        [Test]
        public void AttributesNullTest()
        {
            ApplicationConfig.VisualConfig.PressureConfig = null;
            Assert.Throws<InvalidOperationException>(
                () => RunAndCompareEquals(Colors.Black, Pen, StylusPoint));
        }
    }
}
