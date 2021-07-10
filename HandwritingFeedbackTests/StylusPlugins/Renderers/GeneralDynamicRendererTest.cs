using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.StylusPlugins.Renderers;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.StylusPlugins.Wrappers;
using Moq;
using NUnit.Framework;

namespace HandwritingFeedbackTests.StylusPlugins.Renderers
{
    public class GeneralDynamicRendererTest : ApplicationTest
    {
        private GeneralDynamicRenderer _renderer;
        private StylusPointCollection _stylusPoints;
        private readonly Mock<StopwatchWrapper> _stopwatchMock = new Mock<StopwatchWrapper>();
        private readonly DrawingContextWrapper _drawingContext = new DrawingContextWrapper();

        public override void InternalSetUp()
        {
            _stylusPoints = new StylusPointCollection() {new StylusPoint(1, 2), new StylusPoint(2, 4)};

            _renderer = new GeneralDynamicRenderer(_stopwatchMock.Object);

            GeneralStroke.TemporalCache.Clear();
        }

        #region ConstructorTests

        [Test]
        public void ConstructorNotNullTest()
        {
            _renderer = new GeneralDynamicRenderer();
            Assert.IsNotNull(_renderer);
        }
        
        [Test]
        public void ConstructorValuesSetCorrectlyTest()
        {
            _renderer = new GeneralDynamicRenderer();
            Assert.AreEqual(-1, _renderer.InputStartTime);
            Assert.AreEqual(-1, GeneralDynamicRenderer.StrokeStartTime);
            Assert.AreEqual(-1, _renderer.StrokeEndTime);
            Assert.IsFalse(_renderer.IsStylusDown);
        }
        
        [Test]
        public void ConstructorStartsStopwatchTest()
        {
            _stopwatchMock.CallBase = true;
            _renderer = new GeneralDynamicRenderer();
            Assert.IsTrue(_stopwatchMock.Object.WrappedInstance.IsRunning);
        }

        #endregion

        #region OnDrawTests

        [Test]
        public void OnDrawWithStylusDownTest()
        {
            _renderer.IsStylusDown = true;
            _renderer.InputStartTime = 4;
            _stopwatchMock.Setup(m => m.GetElapsedMilliseconds()).Returns(50);
            const long expectedElapsedTime = 50 - 4;

            _renderer.CustomOnDraw(_drawingContext, _stylusPoints, Geometry.Empty, new SolidColorBrush());
            
            Assert.AreEqual( (_stylusPoints, expectedElapsedTime), GeneralStroke.TemporalCache[0]);
            Assert.AreEqual(1, GeneralStroke.TemporalCache.Count);
        }
        
        [Test]
        public void OnDrawWithStylusUpTest()
        {
            _renderer.IsStylusDown = false;
            _renderer.StrokeEndTime = 32;
            _renderer.InputStartTime = 4;
            const long expectedElapsedTime = 32 - 4;
            
            _renderer.CustomOnDraw(_drawingContext, _stylusPoints, Geometry.Empty, new SolidColorBrush());
            
            Assert.AreEqual( (_stylusPoints, expectedElapsedTime), GeneralStroke.TemporalCache[0]);
            Assert.AreEqual(1, GeneralStroke.TemporalCache.Count);
        }

        [Test]
        public void OnDrawInvalidStylusPointsTest()
        {
            _renderer.IsStylusDown = true;
            _renderer.InputStartTime = 4;
            _stopwatchMock.Setup(m => m.GetElapsedMilliseconds()).Returns(50);

            GeneralStroke.TemporalCache.Add((_stylusPoints, 10L));
            _renderer.CustomOnDraw(_drawingContext, new StylusPointCollection(), Geometry.Empty, new SolidColorBrush());
            
            // Cache should have remained the same
            Assert.AreEqual( (_stylusPoints, 10L), GeneralStroke.TemporalCache[0]);
            Assert.AreEqual(1, GeneralStroke.TemporalCache.Count);
        }

        [Test]
        public void OnDrawSkipsDuplicatePointTest()
        {
            _renderer.IsStylusDown = true;
            _renderer.InputStartTime = 4;
            _stopwatchMock.Setup(m => m.GetElapsedMilliseconds()).Returns(50);
            const long expectedElapsedTime = 50 - 4;

            GeneralStroke.TemporalCache.Add((_stylusPoints, 10L));
            _renderer.CustomOnDraw(_drawingContext, _stylusPoints, Geometry.Empty, new SolidColorBrush());
            
            Assert.AreEqual((_stylusPoints, 10L), GeneralStroke.TemporalCache[0]);
            Assert.AreEqual( (new StylusPointCollection(_stylusPoints.Skip(1)), expectedElapsedTime), GeneralStroke.TemporalCache[1]);
            Assert.AreEqual(2, GeneralStroke.TemporalCache.Count);
        }
        
        #endregion

        #region OnStylusDownTests

        [Test]
        public void OnStylusDownSetToTrue()
        {
            _renderer.IsStylusDown = false;
            _renderer.CustomOnStylusDown();
            Assert.IsTrue(_renderer.IsStylusDown);
        }

        [Test]
        public void StrokeStartTimeGetsUpdated()
        {
            _stopwatchMock.Setup(m => m.GetElapsedMilliseconds()).Returns(50);
            _renderer.CustomOnStylusDown();
            Assert.AreEqual(GeneralDynamicRenderer.StrokeStartTime, 50);
        }

        [Test]
        public void StartTimeUpdatedWhenNegativeOne()
        {
            _renderer.InputStartTime = -1;
            _stopwatchMock.Setup(m => m.GetElapsedMilliseconds()).Returns(50);
            _renderer.CustomOnStylusDown();
            Assert.AreEqual(_renderer.InputStartTime, 50);
        }
        
        [Test]
        public void StartTimeNotUpdatedWhenAlreadySet()
        {
            _renderer.InputStartTime = 10;
            _stopwatchMock.Setup(m => m.GetElapsedMilliseconds()).Returns(50);
            _renderer.CustomOnStylusDown();
            Assert.AreEqual(_renderer.InputStartTime, 10);
        }

        #endregion

        #region OnStylusUpTests

        [Test]
        public void IsStylusDownGetsUpdated()
        {
            _renderer.IsStylusDown = true;
            _renderer.CustomOnStylusUp();
            Assert.IsFalse(_renderer.IsStylusDown);
        }

        [Test]
        public void StrokeEndTimeGetsUpdated()
        {
            _renderer.StrokeEndTime = -1;
            _stopwatchMock.Setup(m => m.GetElapsedMilliseconds()).Returns(50);
            _renderer.CustomOnStylusUp();
            Assert.AreEqual(50, _renderer.StrokeEndTime);
        }
        

        #endregion

        #region MiscalleneousTests

        [Test]
        public void ElapsedTimeTest()
        {
            _stopwatchMock.Setup(m => m.GetElapsedMilliseconds()).Returns(50);
            const long expected = 50L;
            long actual = GeneralDynamicRenderer.GetElapsedTimeInMilliseconds();
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
