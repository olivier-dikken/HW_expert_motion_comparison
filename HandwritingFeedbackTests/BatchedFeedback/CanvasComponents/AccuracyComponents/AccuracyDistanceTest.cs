using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Config;
using HandwritingFeedback.Util;
using NUnit.Framework;

namespace HandwritingFeedbackTests.BatchedFeedback.CanvasComponents.AccuracyComponents
{
    public class AccuracyDistanceTest
    {
        private Stroke _stroke1;
        private Stroke _stroke2;
        private AccuracyDistance _accuracyDistance;

        [SetUp]
        public void Setup()
        {
            _stroke1 = new Stroke(new StylusPointCollection(new List<Point> {new Point(0, 2), new Point(0, 10)}));
            _stroke2 = new Stroke(new StylusPointCollection(new List<Point> {new Point(0, 2), new Point(20, 2)}));
            _accuracyDistance = new AccuracyDistance(new TraceUtils(new StrokeCollection {_stroke1}), 
                new TraceUtils(new StrokeCollection{_stroke2}));
            ApplicationConfig.Instance.MaxDeviationRadius = 5;
        }

        #region ConstructorTests
        
        [Test]
        public void ConstructorNotNullTest()
        {
            Assert.IsNotNull(_accuracyDistance);
        }
        
        [Test]
        public void SynthesizeTitleTest()
        {
            LineGraph lineGraph = (LineGraph) _accuracyDistance.Synthesize();
            Assert.AreEqual("Distance to Expert Trace", lineGraph.Title);
        }
        
        [Test]
        public void SynthesizeCurvedLineTest()
        {
            LineGraph lineGraph = (LineGraph) _accuracyDistance.Synthesize();
            Assert.AreEqual(false, lineGraph.CurvedLine);
        }

        [Test]
        public void SynthesizeLabelsTest()
        {
            LineGraph lineGraph = (LineGraph) _accuracyDistance.Synthesize();
            Assert.AreEqual("Student Trace Completion (%)", lineGraph.XAxisLabel);
            Assert.AreEqual("Distance (pixels)", lineGraph.YAxisLabel);
        }
        
        [Test]
        public void SeriesCountTest()
        {
            var result = (LineGraph) _accuracyDistance.Synthesize();
            Assert.AreEqual(2, result.AllSeries.Count);
        }
        #endregion

        #region CreateDataPointsTests

        [Test]
        public void InRangeDataPointTest()
        {
            var result = _accuracyDistance.CreateDataPoints();
            Assert.AreEqual(2, result.inRangeDataPoints.Count);
            Assert.AreEqual(0, result.outOfRangeDataPoints.Count);
        }

        [Test]
        public void OutOfRangeDataPointTest()
        {
            _stroke1.StylusPoints.Add(new StylusPoint(Double.PositiveInfinity, Double.PositiveInfinity));
            var result = _accuracyDistance.CreateDataPoints();
            // 2 regular data points and 2 data points for transitioning to the out-of-range line.
            Assert.AreEqual(4, result.inRangeDataPoints.Count);
            Assert.AreEqual(1, result.outOfRangeDataPoints.Count);
        }
        
        [Test]
        public void ConsecutiveOutOfRangeDataPointTest()
        {
            _stroke1.StylusPoints.Add(new StylusPoint(Double.PositiveInfinity, Double.PositiveInfinity));
            _stroke1.StylusPoints.Add(new StylusPoint(Double.PositiveInfinity, Double.PositiveInfinity));
            _stroke1.StylusPoints.Add(new StylusPoint(Double.PositiveInfinity, Double.PositiveInfinity));

            var result = _accuracyDistance.CreateDataPoints();
            Assert.AreEqual(4, result.inRangeDataPoints.Count);
            
            // The first out of range point is added to the in-range points in order
            // to obtain a smooth transition from the blue to the orange color.
            // Otherwise the line connecting the in- and out of range points will be orange as well.
            Assert.AreEqual(2, result.outOfRangeDataPoints.Count);
        }
        
        [Test]
        public void PreviousOutOfRangeDataPointTest()
        {
            _stroke1.StylusPoints.Add(new StylusPoint(Double.PositiveInfinity, Double.PositiveInfinity));
            _stroke1.StylusPoints.Add(new StylusPoint(Double.PositiveInfinity, Double.PositiveInfinity));
            _stroke1.StylusPoints.Add(new StylusPoint(0, 2));
            var result = _accuracyDistance.CreateDataPoints();
            // 3 regular data points and 2 data points for transitioning to the out-of-range line,
            // + 1 data point for transitioning from the out-of-range line.
            Assert.AreEqual(6, result.inRangeDataPoints.Count);
            Assert.AreEqual(4, result.outOfRangeDataPoints.Count);
        }
        
        [Test]
        public void FirstPointCorrectTest()
        {
            var result = _accuracyDistance.CreateDataPoints();
            Assert.AreEqual(0, result.inRangeDataPoints.First().X);
            Assert.AreEqual(0, result.inRangeDataPoints.First().Y);
        }
        
        [Test]
        public void SecondPointIsOffTest()
        {
            var result = _accuracyDistance.CreateDataPoints();
            Assert.AreEqual(100, result.inRangeDataPoints[1].X);
            Assert.AreEqual(3d, result.inRangeDataPoints[1].Y);
        }
        
        #endregion
    }
}