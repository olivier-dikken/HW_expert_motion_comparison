using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents;
using NUnit.Framework;

namespace HandwritingFeedbackTests.BatchedFeedback.CanvasComponents.AccuracyComponents
{
    public class CalculationHelperTest : ApplicationTest
    {
        private CalculationHelper _calculationHelper;

        [SetUp]
        public override void InternalSetUp()
        {
            _calculationHelper = new CalculationHelper();
            ApplicationConfig.MaxDeviationRadius = 0.5d;
        }

        [Test]
        public void SameLine100Percent()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(2, 4), new Point(10, 12)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(2, 4), new Point(10, 12)}));
            Assert.AreEqual(100, _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, 1, 0));
        }
        
        [Test]
        public void CloseButNotTheSame0Percent()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 4), new Point(10, 4)}));
            Assert.AreEqual(0, _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, 1, 0));
        }

        [Test]
        public void SameLineDifferentLengths50Percent()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<StylusPoint> {new StylusPoint(0, 2), new StylusPoint(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<StylusPoint> {new StylusPoint(0, 2), new StylusPoint(20, 2)}));
            Assert.AreEqual(50, _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, 1, 0));
        }
        
        [Test]
        public void HitFractionExpertTest()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<StylusPoint> {new StylusPoint(0, 2), new StylusPoint(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<StylusPoint> {new StylusPoint(0, 2), new StylusPoint(20, 2)}));
            _calculationHelper.HitsOnExpertTrace = new int[2];
            Assert.AreEqual(0.5, _calculationHelper.HitFractionExpert(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, 0));
        }
        
        [Test]
        public void HitFractionStudentTest()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<StylusPoint> {new StylusPoint(0, 2), new StylusPoint(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<StylusPoint> {new StylusPoint(0, 2), new StylusPoint(20, 2)}));
            _calculationHelper.HitsOnStudentTrace = new int[2];
            _calculationHelper.PointsHit = new List<(Stroke, StylusPoint)>();
            Assert.AreEqual(0.5, _calculationHelper.HitFractionStudent(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, 0));
        }
        
        [Test]
        public void GetFinalAccuracyTest()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<StylusPoint> {new StylusPoint(0, 2), new StylusPoint(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<StylusPoint> {new StylusPoint(0, 2), new StylusPoint(20, 2)}));
            Assert.AreEqual(50, _calculationHelper.GetFinalAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}));
        }

        [Test]
        public void DifferentLines0Percent()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(10, 10), new Point(10, 20)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 0), new Point(-5, -10)}));
            Assert.AreEqual(0, _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, 1, 0));
        }
        
        [Test]
        public void ParallelLinesButTooFarApart0Percent()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 6.1), new Point(10, 6.1)}));
            Assert.AreEqual(0, _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, 1, 0));
        }
        
        [Test]
        public void HitArrayInitializedTest()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 6.1), new Point(10, 6.1)}));
            _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, 1, 0);
            Assert.AreEqual(1,_calculationHelper.HitsOnExpertTrace.Length);
        }
        
        [Test]
        public void StudentHitArrayInitializedTest()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 6.1), new Point(10, 6.1)}));
            int sections = 2;
            _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, sections, 0);
            Assert.AreEqual(sections,_calculationHelper.HitsOnStudentTrace.Length);
        }
        
        [Test]
        public void StudentPointCountTest()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 6.1)}));
            int sections = 2;
            _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, sections, 0);
            Assert.AreEqual(stroke1.StylusPoints.Count,_calculationHelper.StudentPointsCount);
        }
        
        [Test]
        public void ReturnFinalAccuracyNotSetTest()
        {
            var stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            var stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 6.1), new Point(10, 6.1)}));
            Assert.AreEqual(-1.0, _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke1}, new StrokeCollection {stroke2}, 1, 1));
        }
        
        [Test]
        public void OverlappingLines100Percent()
        {
            var stroke1 = new Stroke(new StylusPointCollection{new StylusPoint(2, 4), new StylusPoint(10, 12)});
            var stroke2 = new Stroke(new StylusPointCollection{new StylusPoint(10, 12), new StylusPoint(2, 4)});
            var stroke3 = new Stroke(new StylusPointCollection{new StylusPoint(2, 4), new StylusPoint(10, 12)});
            Assert.AreEqual(100, _calculationHelper.CalculateAccuracy(new StrokeCollection {stroke3}, new StrokeCollection {stroke1, stroke2}, 1, 0));
        }
    }
}