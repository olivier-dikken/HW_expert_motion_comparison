using System.Collections.Generic;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback.FeedbackTypes
{
    /// <summary>
    /// Test suite for basic methods of the VisualFeedback class.
    /// </summary>
    class VisualFeedbackTest : ApplicationTest
    {
        public override void InternalSetUp() {}
        
        [Test]
        public void GetterSetterTest()
        {
            ApplicationConfig = new ApplicationConfig {VisualConfig = {PenThicknessModifier = 3f}};
            
            Assert.AreEqual(ApplicationConfig.Instance.VisualConfig.PenThicknessModifier, 3f);
        }

        [Test]
        public void InputSourcesGetterSetterTest()
        {
            var testList = new List<InputSource>();
            VisualFeedback.GetInstance().InputSources = testList;
            Assert.AreEqual(VisualFeedback.GetInstance().InputSources, testList);
        }

        [Test]
        public void ScaleColorTest()
        {
            Color c = Color.FromRgb(255, 56, 15);
            double scalar = 0.7d;
            Color scaledColor = VisualFeedback.ScaleColor(c, scalar);

            var expected = Color.FromRgb(178, 39, 10);
            Assert.True(scaledColor.Equals(expected));
        }

        [Test]
        public void AddColorChannelsTest()
        {
            var color1 = Color.FromRgb(200, 50, 10);
            var color2 = Color.FromRgb(20, 5, 7);

            var expected = Color.FromRgb(220, 55, 17);
            Color actual = VisualFeedback.AddColorChannels(color1, color2);
            Assert.True(expected.Equals(actual));
        }

        [Test]
        public void AddColorChannelsOverflowTest()
        {
            var color1 = Color.FromRgb(255, 0, 100);
            var color2 = Color.FromRgb(3, 0, 50);

            var expected = Color.FromRgb(255, 0, 150);
            Color actual = VisualFeedback.AddColorChannels(color1, color2);
            Assert.True(expected.Equals(actual));
        }
    }
}
