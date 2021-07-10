using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet;
using HandwritingFeedback.Util;
using Moq;
using NAudio.Wave;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback.InputSources.Tablet
{
    class TabletSourceTest : ApplicationTest
    {
        private Mock<VisualComponent> _mockVisual;
        private Mock<AuditoryComponent> _mockAuditory;
        private TabletSource _source;

        private Mock<IWavePlayer> _mockPlayer;
        private SineWaveProvider _sineWaveProvider;
        private Pen _pen;
        private StylusPoint _stylusPoint;

        public override void InternalSetUp()
        {
            _mockVisual = new Mock<VisualComponent>(MockBehavior.Strict);
            _mockPlayer = new Mock<IWavePlayer>(MockBehavior.Strict);
            _mockAuditory = new Mock<AuditoryComponent>(MockBehavior.Strict);
            _source = new TabletSource();
            _source.Components.Clear();
            _pen = new Pen(new SolidColorBrush(Colors.Black), 2d);
            _stylusPoint = new StylusPoint(0, 0, 0.5f);
            _sineWaveProvider = new SineWaveProvider();
        }

        [Test]
        public void CollectBasicTest()
        {
            _mockVisual.Setup(m => m.Synthesize(_pen, It.IsAny<RTFInputData>()));
            _source.Components.Add(_mockVisual.Object);

            RTFInputData rtfInput = new RTFInputData() {StylusPoint = _stylusPoint};
            
            _source.CollectVisual(_pen, rtfInput);
            _mockVisual.Verify(m => m.Synthesize(_pen, rtfInput), Times.Once);
        }
        
        [Test]
        public void CollectAuditoryTest()
        {
            _mockAuditory.Setup(m => m.Synthesize(_sineWaveProvider, It.IsAny<RTFInputData>()));
            _source.Components.Add(_mockAuditory.Object);
            _mockPlayer.Setup(m => m.Play());

            RTFInputData rtfInput = new RTFInputData() {StylusPoint = _stylusPoint};
            
            _source.CollectAuditory(_mockPlayer.Object, _sineWaveProvider, rtfInput);
            _mockAuditory.Verify(m => m.Synthesize(_sineWaveProvider, rtfInput), Times.Once);
            _mockPlayer.Verify(m=> m.Play(), Times.Once);
        }

        [Test]
        public void ConfigureTest()
        {
            Application.Current.Properties[ApplicationPropertyKeys.RealtimeSetting] = 0;
            _source = new TabletSource();

            Assert.True(ApplicationConfig.Instance.VisualConfig.PressureConfig == null);
            Assert.True(ApplicationConfig.Instance.VisualConfig.AccuracyConfig == null);
        }
    }
}