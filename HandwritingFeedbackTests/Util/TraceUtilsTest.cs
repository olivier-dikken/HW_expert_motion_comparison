using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.Util;
using NUnit.Framework;

namespace HandwritingFeedbackTests.Util
{
    public class TraceUtilsTest : ApplicationTest
    {
        private TraceUtils _traceUtils;
        private StrokeCollection _strokeCollection;
        private StrokeCollection _oneStrokeCollection;
        private Stroke _stroke1;
        private Stroke _stroke2;
        
        [SetUp]
        public override void InternalSetUp()
        {
            _stroke1 = new Stroke(
                new StylusPointCollection
                {
                    new StylusPoint(0d, 0d), 
                    new StylusPoint(1d, 1d)
                });
            _stroke2 = new Stroke(
                new StylusPointCollection
                {
                    new StylusPoint(1d, 2d), 
                    new StylusPoint(6d, 8d), 
                    new StylusPoint(7d, 8d),
                    new StylusPoint(5d, 10d)
                });
            _strokeCollection = new StrokeCollection { _stroke1, _stroke2 };
            _oneStrokeCollection = new StrokeCollection { _stroke1 };

            ApplicationConfig.MaxDeviationRadius = 20d;
            ApplicationConfig.ClosestPointScanDiameter = 100d;
            
            _traceUtils = new TraceUtils(_strokeCollection);
        }
        
        #region SplitTraceTests
        
        [Test]
        public void NoSplitTest()
        {
            StrokeCollection result = _traceUtils.Split(10);
            Assert.AreEqual(_stroke1.StylusPoints, result[0].StylusPoints);
            Assert.AreEqual(_stroke2.StylusPoints, result[1].StylusPoints);
        }

        [Test]
        public void NoSplitBoundaryTest()
        {
            StrokeCollection result = _traceUtils.Split(4);
            Assert.AreEqual(_stroke1.StylusPoints, result[0].StylusPoints);
            Assert.AreEqual(_stroke2.StylusPoints, result[1].StylusPoints);
        }
        
        [Test]
        public void OneSplitLowerBoundaryTest()
        {
            StrokeCollection result = _traceUtils.Split(3);
            
            Assert.AreEqual(_stroke1.StylusPoints, result[0].StylusPoints);
            
            StylusPointCollection first =
                new StylusPointCollection(_stroke2.StylusPoints.Take(3));
            // second should also contain the last point of the previous stroke
            StylusPointCollection second =
                new StylusPointCollection(_stroke2.StylusPoints.Skip(2).Take(2));
            
            Assert.AreEqual(first, result[1].StylusPoints);
            Assert.AreEqual(second, result[2].StylusPoints);
        }

        [Test]
        public void OneSplitUpperBoundaryTest()
        {
            StrokeCollection result = _traceUtils.Split(2);

            Assert.AreEqual(_stroke1.StylusPoints, result[0].StylusPoints);
            
            StylusPointCollection first =
                new StylusPointCollection(_stroke2.StylusPoints.Take(2));
            // second should also contain the last point of the previous stroke
            StylusPointCollection second =
                new StylusPointCollection(_stroke2.StylusPoints.Skip(1).Take(3));
            
            Assert.AreEqual(first, result[1].StylusPoints);
            Assert.AreEqual(second, result[2].StylusPoints);
        }

        [Test]
        public void ManySplitsTest()
        {
            StrokeCollection result = _traceUtils.Split(1);

            StylusPointCollection first =
                new StylusPointCollection(_stroke1.StylusPoints.Take(1));
            StylusPointCollection second =
                new StylusPointCollection(_stroke1.StylusPoints.Take(2));

            Assert.AreEqual(first, result[0].StylusPoints);
            Assert.AreEqual(second, result[1].StylusPoints);
            
            StylusPointCollection third =
                new StylusPointCollection(_stroke2.StylusPoints.Take(1));
            StylusPointCollection fourth =
                new StylusPointCollection(_stroke2.StylusPoints.Take(2));
            StylusPointCollection fifth =
                new StylusPointCollection(_stroke2.StylusPoints.Skip(1).Take(2));
            StylusPointCollection sixth =
                new StylusPointCollection(_stroke2.StylusPoints.Skip(2).Take(2));
            
            Assert.AreEqual(third, result[2].StylusPoints);
            Assert.AreEqual(fourth, result[3].StylusPoints);
            Assert.AreEqual(fifth, result[4].StylusPoints);
            Assert.AreEqual(sixth, result[5].StylusPoints);
        }
        
        #endregion

        #region GetClosestPointTests

        // For all these tests, the scan radius will be at least 100 (set in GetClosestPoint),
        // and can therefore only be customized for radii > 100.
        
        [Test]
        public void OutOfRangeTest()
        {
            Point point = new Point(200, 50);
            _traceUtils = new TraceUtils(_oneStrokeCollection);
            var (stylusPoint, distance) = _traceUtils.GetClosestPoint(point);
            StylusPoint expectedPoint = new StylusPoint(double.NegativeInfinity, double.NegativeInfinity);
            Assert.AreEqual(expectedPoint, stylusPoint);
            Assert.AreEqual(double.PositiveInfinity, distance);
        }

        [Test]
        public void OutOfRangeBoundaryTest()
        {
            // The scan radius is always at least 100
            // Both points have a thickness of 1, which is considered by the HitTest
            // The boundary is therefore at 52
            Point point = new Point(52.1, 1);
            _traceUtils = new TraceUtils(_oneStrokeCollection);
            var (stylusPoint, distance) = _traceUtils.GetClosestPoint(point);
            StylusPoint expectedPoint = new StylusPoint(double.NegativeInfinity, double.NegativeInfinity);
            Assert.AreEqual(expectedPoint, stylusPoint);
            Assert.AreEqual(double.PositiveInfinity, distance);
        }

        [Test]
        public void InRangeBoundaryTest()
        {
            Point point = new Point(52, 1);
            _traceUtils = new TraceUtils(_oneStrokeCollection);
            var (stylusPoint, distance) = _traceUtils.GetClosestPoint(point);
            StylusPoint expectedPoint = new StylusPoint(1, 1);
            Assert.AreEqual(expectedPoint, stylusPoint);
            // For the distance, the thickness of 1 is ignored (distance from center)
            Assert.That(distance, Is.EqualTo(51d).Within(0.0001d));
        }

        [Test]
        public void InRangeTest()
        {
            Point point = new Point(20, 5);
            _traceUtils = new TraceUtils(_oneStrokeCollection);
            var (stylusPoint, distance) = _traceUtils.GetClosestPoint(point);
            StylusPoint expectedPoint = new StylusPoint(1, 1);
            Assert.AreEqual(expectedPoint, stylusPoint);
            // For the distance, the thickness of 1 is ignored (distance from center)
            Assert.That(distance, Is.EqualTo(19.4165d).Within(0.0001d));
        }
        
        [Test]
        public void InRangeManyCandidatesTest()
        {
            Point point = new Point(8, 8);
            var (stylusPoint, distance) = _traceUtils.GetClosestPoint(point);
            StylusPoint expectedPoint = new StylusPoint(7, 8);
            Assert.AreEqual(expectedPoint, stylusPoint);
            // For the distance, the thickness of 1 is ignored (distance from center)
            Assert.That(distance, Is.EqualTo(1d).Within(0.0001d));
        }

        [Test]
        public void TiedCandidatesReturnsFirstTest()
        {
            Point point = new Point(1, 1.5);
            var (stylusPoint, distance) = _traceUtils.GetClosestPoint(point);
            // In a tie, we choose the first point
            StylusPoint expectedPoint = new StylusPoint(1, 1);
            Assert.AreEqual(expectedPoint, stylusPoint);
            // For the distance, the thickness of 1 is ignored (distance from center)
            Assert.That(distance, Is.EqualTo(0.5d).Within(0.0001d));
        }

        #endregion
        
        #region InterpolatePoint

        [Test]
        public void InterpolatePointDefaultPointTest()
        {
            // We set this because it is used inside of the application
            ApplicationConfig.StylusPointDescription = new StylusPointDescription(new[]
            {
                new StylusPointPropertyInfo(StylusPointProperties.X),
                new StylusPointPropertyInfo(StylusPointProperties.Y),
                new StylusPointPropertyInfo(StylusPointProperties.NormalPressure),
            });
            
            var stylusPointFrom = new StylusPoint(0, 2, 0.6f);
            var stylusPointTo = new StylusPoint(0, 4, 0.8f);

            var result = TraceUtils.InterpolatePoint(stylusPointFrom, stylusPointTo, 0.5f);
            
            Assert.AreEqual(0, result.X);
            Assert.AreEqual(3, result.Y);
            Assert.AreEqual(0.7f, result.PressureFactor, 0.01);
        }

        [Test]
        public void InterpolatePointPointWithTiltTest()
        {
            // We set this because it is used inside of the application
            ApplicationConfig.StylusPointDescription = new StylusPointDescription(new[]
            {
                new StylusPointPropertyInfo(StylusPointProperties.X),
                new StylusPointPropertyInfo(StylusPointProperties.Y),
                new StylusPointPropertyInfo(StylusPointProperties.NormalPressure),
                // These are the default values for the XTiltOrientation but they are different
                // in the testing environment, so we set them to real world values
                new StylusPointPropertyInfo(StylusPointProperties.XTiltOrientation, -9000, 9000, StylusPointPropertyUnit.Degrees, 100)
            });
            
            var stylusPointFrom = new StylusPoint(0, 2, 0.5f, ApplicationConfig.StylusPointDescription, new[] {4000});
            var stylusPointTo = new StylusPoint(0, 4, 0.5f, ApplicationConfig.StylusPointDescription, new[] {6000});

            var result = TraceUtils.InterpolatePoint(stylusPointFrom, stylusPointTo, 0.5f);
            
            Assert.AreEqual(5000, result.GetPropertyValue(StylusPointProperties.XTiltOrientation));
        }
        
        #endregion
        
        [Test]
        public void GetAllPointsTest()
        {
            StylusPointCollection stylusPointCollection = new StylusPointCollection(new List<Point> {new Point(0, 2), new Point(10, 2)});
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            StrokeCollection strokeCollection = new StrokeCollection {stroke1};
            _traceUtils = new TraceUtils(strokeCollection);
            Assert.AreEqual(stylusPointCollection, _traceUtils.GetAllPoints());
        }
        
        [Test]
        public void GetAllPointsMultipleStrokesTest()
        {
            StylusPointCollection stylusPointCollection = new StylusPointCollection(new List<Point> {new Point(0, 2), new Point(10, 2), new Point(4, 4), new Point(2, 8)});
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(4, 4), new Point(2, 8)}));
            StrokeCollection strokeCollection = new StrokeCollection {stroke1, stroke2};
            _traceUtils = new TraceUtils(strokeCollection);
            Assert.AreEqual(stylusPointCollection, _traceUtils.GetAllPoints());
        }
        
        [Test]
        public void GetNumberOfStylusPointsTest()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            StrokeCollection strokeCollection = new StrokeCollection {stroke1};
            _traceUtils = new TraceUtils(strokeCollection);
            Assert.AreEqual(2, _traceUtils.GetNumberOfStylusPoints());
        }
        
        [Test]
        public void GetNumberOfStylusPointsMultipleStrokesTest()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(4, 4), new Point(2, 8)}));
            StrokeCollection strokeCollection = new StrokeCollection {stroke1, stroke2};
            _traceUtils = new TraceUtils(strokeCollection);
            Assert.AreEqual(4, _traceUtils.GetNumberOfStylusPoints());
        }
        
        [Test]
        public void CalculateTraceLengthTest()
        {
            var point1 = new Point(0, 2);
            var point2 = new Point(10, 2);
            
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {point1, point2}));
            StrokeCollection strokeCollection = new StrokeCollection {stroke1};
            double length = Point.Subtract(point1, point2).Length;
            _traceUtils = new TraceUtils(strokeCollection);
            Assert.AreEqual(length, _traceUtils.CalculateTraceLengths(null));
        }
        
        [Test]
        public void CalculateTraceLength3PointsTest()
        {
            var point1 = new Point(0, 2);
            var point2 = new Point(10, 2);
            var point3 = new Point(12, 2);
            
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {point1, point2, point3}));
            StrokeCollection strokeCollection = new StrokeCollection {stroke1};
            double length = Point.Subtract(point1, point2).Length + Point.Subtract(point2, point3).Length;
            _traceUtils = new TraceUtils(strokeCollection);
            Assert.AreEqual(length, _traceUtils.CalculateTraceLengths(null));
        }
        
        [Test]
        public void CalculateTraceLength3PointsListTest()
        {
            var point1 = new Point(0, 2);
            var point2 = new Point(10, 2);
            var point3 = new Point(12, 2);
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {point1, point2, point3}));
            StrokeCollection strokeCollection = new StrokeCollection {stroke1};

            List<double> actualList = new List<double>();
            List<double> expectedList = new List<double>();
            double length1 = Point.Subtract(point1, point1).Length;
            double length2 = Point.Subtract(point1, point2).Length;
            double length3 = Point.Subtract(point2, point3).Length;
            
            expectedList.Add(length1);
            expectedList.Add(length1 + length2);
            expectedList.Add(length1 + length2 + length3);
            _traceUtils = new TraceUtils(strokeCollection);
            _traceUtils.CalculateTraceLengths(actualList);
            Assert.AreEqual(expectedList, actualList);
        }
    }
}