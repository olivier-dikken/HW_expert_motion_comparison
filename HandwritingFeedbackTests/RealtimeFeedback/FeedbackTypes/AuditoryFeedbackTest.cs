using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;
using Moq;
using NAudio.Wave;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback.FeedbackTypes
{
    public class AuditoryFeedbackTest : ApplicationTest
    {
        public override void InternalSetUp()
        {
        }

        [Test]
        public void CollectAuditoryFeedbackTest()
        {
            var stylusPoint = new StylusPoint(0, 0);
            var stylusPoint2 = new StylusPoint(int.MaxValue, int.MinValue);
            var stylusPoints = new StylusPointCollection {stylusPoint, stylusPoint2};

            var source1 = new Mock<TabletSource>();

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
            source1.Setup(m =>
                m.CollectAuditory(It.IsAny<IWavePlayer>(), It.IsAny<SineWaveProvider>(), It.IsAny<RTFInputData>()));

            AuditoryFeedback.GetInstance().InputSources = new List<InputSource> {source1.Object};

            Mock<IWavePlayer> playerMock = new Mock<IWavePlayer>(MockBehavior.Strict);
            playerMock.Setup(m => m.Pause());

            // Feedback calls CollectAuditory
            AuditoryFeedback.GetInstance().Feedback(playerMock.Object, stylusPoints);

            source1.Verify(
                m => m.CollectAuditory(It.IsAny<IWavePlayer>(), It.IsAny<SineWaveProvider>(), It.IsAny<RTFInputData>()),
                Times.Once);
            source1.Verify(m => m.CollectVisual(It.IsAny<Pen>(), It.IsAny<RTFInputData>()), Times.Never);

            // stylusPoint2 is outside range
            playerMock.Verify(m => m.Pause(), Times.Once);
        }
    }
}