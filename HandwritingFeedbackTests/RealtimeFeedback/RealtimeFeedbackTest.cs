using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet;
using HandwritingFeedback.StylusPlugins.Wrappers;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;
using Moq;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback
{
    class RealTimeFeedbackTest : ApplicationTest
    {
        private double _expertValue;
        private double _studentValue;

        private double _lowStart;
        private double _lowCutoff;
        private double _highStart;
        private double _highCutoff;
        
        public override void InternalSetUp()
        {
            _expertValue = 0.5d;
            _studentValue = 0.5d;

            _lowStart = 0.2d;
            _lowCutoff = 0.4d;
            _highStart = 0.3d;
            _highCutoff = 0.5d;
        }

        [Test]
        public void CollectFeedbackTest()
        {
            var stylusPoint = new StylusPoint(0, 0);
            var stylusPoints = new StylusPointCollection {stylusPoint};

            var drawingContextWrapper = 
                new DrawingContextWrapper { WrappedInstance = new DrawingGroup().Open() };

            // Create multiple (2) InputSources to test the foreach loop in CollectFeedback.
            var source1 = new Mock<TabletSource>();
            var source2 = new Mock<TabletSource>();

            PracticeMode.ExpertTraceUtils = new TraceUtils(new StrokeCollection
            {
                new Stroke
                (
                    new StylusPointCollection
                    {
                        new StylusPoint(0, 0, 0.5f)
                    }
                )
            });
            
            source1.Setup(m => m.Components).Returns(new List<FeedbackComponent>());
            source2.Setup(m => m.Components).Returns(new List<FeedbackComponent>());

            source1.Setup(m => m.CollectVisual(It.IsAny<Pen>(), It.IsAny<RTFInputData>()));
            source2.Setup(m => m.CollectVisual(It.IsAny<Pen>(), It.IsAny<RTFInputData>()));

            VisualFeedback.GetInstance().InputSources = new List<InputSource>() { source1.Object, source2.Object };

            // Feedback calls CollectFeedback
            VisualFeedback.GetInstance().Feedback(drawingContextWrapper, stylusPoints);

            source1.Verify(m => m.CollectVisual(It.IsAny<Pen>(), It.IsAny<RTFInputData>()), Times.Once);
            source2.Verify(m => m.CollectVisual(It.IsAny<Pen>(), It.IsAny<RTFInputData>()), Times.Once);
        }
        
        private void AssertThatRatioEquals(double expected)
        {
            Assert.That(HandwritingFeedback.RealtimeFeedback.RealtimeFeedback.Generate(_studentValue, _expertValue, _lowStart,
                _lowCutoff, _highStart, _highCutoff), Is.EqualTo(expected).Within(0.0001));
        }
        
        #region ExpertDefaultValue

        [Test]
        public void RatioZeroTest()
        {
            _studentValue = 0.45;
            AssertThatRatioEquals(0d);
        }

        [Test]
        public void LowValueEqualsCutoff()
        {
            _lowCutoff = 0.35;
            _studentValue = 0.15;
            AssertThatRatioEquals(-1d);
        }

        [Test]
        public void LowValueExceedsCutoff()
        {
            _lowCutoff = 0.4;
            _studentValue = 0.05;
            AssertThatRatioEquals(-1.25d);
        }

        [Test]
        public void HighValueEqualsCutoff()
        {
            _highCutoff = 0.35;
            _studentValue = 0.85;
            AssertThatRatioEquals(1d);
        }

        [Test]
        public void HighValueExceedsCutoff()
        {
            _highCutoff = 0.4;
            _studentValue = 1;
            AssertThatRatioEquals(2d);
        }

        [Test]
        public void LowValueWithStartEqualsCutoffTest()
        {
            _lowStart = 0.2;
            _lowCutoff = 0.2;
            _studentValue = 0.1;
            AssertThatRatioEquals(-1d);
        }

        [Test]
        public void HighValueWithStartEqualsCutoffTest()
        {
            _highStart = 0.3;
            _highCutoff = 0.3;
            _studentValue = 0.9;
            AssertThatRatioEquals(1d);
        }

        [Test]
        public void LowValueAtStartTest()
        {
            _studentValue = 0.3;
            AssertThatRatioEquals(0d);
        }

        [Test]
        public void LowValueInBetweenTest()
        {
            _studentValue = 0.25;
            AssertThatRatioEquals(-0.25d);
        }

        [Test]
        public void HighValueAtStartTest()
        {
            _studentValue = 0.6;
            AssertThatRatioEquals(0d);
        }

        [Test]
        public void HighValueInBetweenTest()
        {
            _studentValue = 0.85;
            AssertThatRatioEquals(0.25d);
        }
        
        #endregion

        #region ExpertHighValue

        [Test]
        public void ExpertHighValueMatchedByStudentTest()
        {
            _expertValue = 0.8;
            _studentValue = 0.8;
            AssertThatRatioEquals(0d);
        }
        
        [Test]
        public void ExpertHighValueStudentLowValueTest()
        {
            _expertValue = 0.8;
            _studentValue = 0.5;
            AssertThatRatioEquals(-0.5d);
        }

        [Test]
        public void ExpertHighValueStudentHigherValueTest()
        {
            _highStart = 0.1;
            _highCutoff = 0.2f;
            _expertValue = 0.8;
            _studentValue = 0.95;
            AssertThatRatioEquals(0.5d);
        }

        [Test]
        public void ExpertHighValueCutoffBeyondMaxTest()
        {
            _expertValue = 0.6;
            _studentValue = 1f;
            AssertThatRatioEquals(0.5d);
        }
        
        [Test]
        public void ExpertAndStudentMaxValueTest()
        {
            _expertValue = 1;
            _studentValue = 1f;
            AssertThatRatioEquals(0d);
        }

        #endregion
        
        #region ExpertLowValue
        
        [Test]
        public void ExpertLowValueMatchedByStudentTest()
        {
            _expertValue = 0.3;
            _studentValue = 0.3;
            AssertThatRatioEquals(0d);
            
        }
        
        [Test]
        public void ExpertLowValueStudentLowerValueTest()
        {
            _expertValue = 0.4;
            _studentValue = 0.1;
            AssertThatRatioEquals(-0.5d);
        }

        [Test]
        public void ExpertLowValueStudentHighValueTest()
        {
            _expertValue = 0.2;
            _studentValue = 0.7;
            AssertThatRatioEquals(1d);
        }

        [Test]
        public void ExpertLowValueCutoffBeyondMinTest()
        {
            _expertValue = 0.3;
            _studentValue = 0;
            AssertThatRatioEquals(-0.5d);
        }
        
        [Test]
        public void ExpertAndStudentMinValueTest()
        {
            _expertValue = 0.001;
            _studentValue = 0.001;
            AssertThatRatioEquals(0d);
        }
        
        #endregion
        
    }
}
