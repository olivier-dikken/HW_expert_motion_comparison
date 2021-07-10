using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.Config.Visual;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.StylusPlugins.Wrappers;
using Moq;
using NUnit.Framework;

namespace HandwritingFeedbackTests.StylusPlugins.Strokes
{
    public class StudentStokeTest : ApplicationTest
    {
        private StudentStroke _stroke;
        private DrawingContextWrapper _drawingContext;
        private (StylusPoint, Pen) _tuple;
        private Pen _pen;
        private Mock<DrawingContextWrapper> _contextMock;

        public override void InternalSetUp()
        {
            var collection = new StylusPointCollection();
            var stylusPoint = new StylusPoint(0, 0);
            collection.Add(stylusPoint);
            _stroke = new StudentStroke(collection);
            
            StudentStroke.PensCache.Clear();
            _drawingContext = new DrawingContextWrapper {WrappedInstance = new DrawingGroup().Open()};
            
            _pen = new Pen(new SolidColorBrush(Colors.Black), 2d);
            _tuple = (stylusPoint, _pen);

            _contextMock = new Mock<DrawingContextWrapper>(MockBehavior.Strict);

            ApplicationConfig.VisualConfig = new VisualConfig
            {
                PenThicknessModifier = 3f
            };

        }

        [Test]
        public void DrawCoreLocalCacheNullTest()
        {
            StudentStroke.PensCache.Add(_tuple);
            _stroke.DrawCore(_drawingContext);
            Assert.AreEqual(_stroke.CacheSnapshot[0].pen, _pen);
        }

        [Test]
        public void DrawCoreLocalCacheAlreadySetTest()
        {
            StudentStroke.PensCache.Add(_tuple);
            _stroke.CacheSnapshot = new[] { _tuple };
            
            // When the local cache is not null, the global cache should not be cleared
            _stroke.DrawCore(_drawingContext);
            Assert.AreEqual(_stroke.CacheSnapshot[0].pen, StudentStroke.PensCache[0].pen);
        }

        [Test]
        public void DrawCoreLocalCacheEmptyTest()
        {
            _stroke.CacheSnapshot = new (StylusPoint, Pen)[0];
            var prev = new Point(double.NegativeInfinity, double.NegativeInfinity);
            
            _contextMock.Setup(m =>
                m.DrawLine(_pen, It.IsAny<Point>(),
                    It.IsAny<Point>()));
            
            _stroke.DrawCore(_contextMock.Object);
          
            // No point should be reinforced
            _contextMock.Verify(m =>
                    m.DrawEllipse(It.IsAny<Brush>(),
                        It.IsAny<Pen>(),
                        It.IsAny<Point>(),
                        It.IsAny<double>(),
                        It.IsAny<double>()),
                Times.Never);
            
            // No point should be drawn
            _contextMock.Verify(m =>
                m.DrawLine(_pen, It.Is<Point>(p => p.Equals(prev)),
                _stroke.StylusPoints[0].ToPoint()),
                Times.Never);
        }

        [Test]
        public void DrawCoreOneStylusPoint()
        {
            _stroke.CacheSnapshot = new[] { _tuple };
            Point prev = new Point(double.NegativeInfinity, double.NegativeInfinity);
            
            // For DrawEllipse invoked to reinforce point
            double ellipseThickness = 0.20f * ApplicationConfig.VisualConfig.PenThicknessModifier;
            Pen ellipsePen = new Pen(_pen.Brush, ellipseThickness);
            _contextMock.Setup(m =>
                m.DrawEllipse(It.IsAny<Brush>(),
                    It.IsAny<Pen>(),
                    It.IsAny<Point>(),
                    It.IsAny<double>(),
                    It.IsAny<double>()));
            
            // For DrawLine invoked inside for loop
            _contextMock.Setup(m =>
                m.DrawLine(_pen, It.IsAny<Point>(),
                    It.IsAny<Point>()));
            
            _stroke.DrawCore(_contextMock.Object);
            
            _contextMock.Verify(m =>
                m.DrawEllipse(It.Is<Brush>(b => b.Equals(ellipsePen.Brush)),
                    It.Is<Pen>(p => Math.Abs(p.Thickness - ellipseThickness) < 0.1f),
                    _stroke.StylusPoints[0].ToPoint(),
                    It.Is<double>(radX => Math.Abs(radX - ellipseThickness) < 0.1f),
                    It.Is<double>(radY => Math.Abs(radY - ellipseThickness) < 0.1f)),
                Times.Once);

            _contextMock.Verify(m =>
                m.DrawLine(_pen, It.Is<Point>(p => p.Equals(prev)),
                    _stroke.StylusPoints[0].ToPoint()),
                Times.Once);
        }

        [Test]
        public void DrawCoreMultipleStylusPoints()
        {
            _stroke.CacheSnapshot = new[] { _tuple, _tuple, _tuple };
            Point prev = new Point(double.NegativeInfinity, double.NegativeInfinity);

            StylusPoint second = new StylusPoint(2, 3, 0.6f);
            StylusPoint third = new StylusPoint(3, 8, 0.2f);
            _stroke.StylusPoints.Add(second);
            _stroke.StylusPoints.Add(third);

            // For DrawEllipse invoked to reinforce point
            double ellipseThickness = 0.20f * ApplicationConfig.VisualConfig.PenThicknessModifier;
            Pen ellipsePen = new Pen(_pen.Brush, ellipseThickness);
            _contextMock.Setup(m =>
                m.DrawEllipse(It.IsAny<Brush>(),
                    It.IsAny<Pen>(),
                    It.IsAny<Point>(),
                    It.IsAny<double>(),
                    It.IsAny<double>()));
            
            // For DrawLine invoked inside for loop
            _contextMock.Setup(m =>
                m.DrawLine(_pen, It.IsAny<Point>(),
                    It.IsAny<Point>()));
            
            _stroke.DrawCore(_contextMock.Object);
            
            // Reinforcement happens only once, even when multiple points are contained in the stroke
            _contextMock.Verify(m =>
                    m.DrawEllipse(It.Is<Brush>(b => b.Equals(ellipsePen.Brush)),
                        It.Is<Pen>(p => Math.Abs(p.Thickness - ellipseThickness) < 0.1f),
                        _stroke.StylusPoints[0].ToPoint(),
                        It.Is<double>(radX => Math.Abs(radX - ellipseThickness) < 0.1f),
                        It.Is<double>(radY => Math.Abs(radY - ellipseThickness) < 0.1f)),
                Times.Once);

            // Verify that all points are drawn
            
            _contextMock.Verify(m =>
                    m.DrawLine(_pen, It.Is<Point>(p => p.Equals(prev)),
                        _stroke.StylusPoints[0].ToPoint()),
                Times.Once);
            
            _contextMock.Verify(m =>
                    m.DrawLine(_pen, It.Is<Point>(p => p.Equals(_stroke.StylusPoints[0].ToPoint())),
                        _stroke.StylusPoints[1].ToPoint()),
                Times.Once);
            
            _contextMock.Verify(m =>
                    m.DrawLine(_pen, It.Is<Point>(p => p.Equals(_stroke.StylusPoints[1].ToPoint())),
                        _stroke.StylusPoints[2].ToPoint()),
                Times.Once);
        }
    }
}
