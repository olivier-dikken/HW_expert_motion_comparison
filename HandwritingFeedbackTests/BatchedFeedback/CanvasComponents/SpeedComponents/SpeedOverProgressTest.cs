using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.Components;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.SpeedComponents;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Config;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.Util;
using NUnit.Framework;
using OxyPlot;

namespace HandwritingFeedbackTests.BatchedFeedback.CanvasComponents.SpeedComponents
{
    public class SpeedOverProgressTest
    {
        private SpeedOverProgress _component;
        private GeneralStroke _studentStroke;
        private GeneralStroke _expertStroke;
        private StrokeCollection _studentTrace;
        private StrokeCollection _expertTrace;
        private Dictionary<Point, Queue<double>> _testMapping;
        private const int NumberOfNeighbours = 32;

        [SetUp]
        public void Setup()
        {
            ApplicationConfig.Instance.DataPointsAveragingNeighbors = NumberOfNeighbours;
            
            // Construct new temporal mapping to be used as parameter in tests
            _testMapping = ConstructTemporalMapping(10);

            // Instantiate new speed over progress component
            _studentStroke = CreateGeneralStroke(3);
            _expertStroke = CreateGeneralStroke(3);

            _studentTrace = new StrokeCollection { _studentStroke };
            _expertTrace = new StrokeCollection { _expertStroke };

            _component = new SpeedOverProgress(new TraceUtils(_studentTrace), new TraceUtils(_expertTrace));
        }

        /// <summary>
        /// Creates a new general stroke with stylus points at co-ordinates
        /// (0, x) for all x smaller than the provided parameter.
        /// </summary>
        /// <param name="amountOfStylusPoints">Number of new stylus points to create</param>
        /// <returns>List of points</returns>
        private GeneralStroke CreateGeneralStroke(int amountOfStylusPoints)
        {
            var stylusPoints = new StylusPointCollection();
            for (var x = 0; x < amountOfStylusPoints; x++)
            {
                stylusPoints.Add(new StylusPoint(0, x));
            }
            return new GeneralStroke(stylusPoints);
        }

        /// <summary>
        /// Creates a new temporal mapping with stylus point keys at co-ordinates
        /// (0, x) for all x smaller than the provided parameter.
        /// Timestamps for each point start at 1000 and are incremented by 1000 for
        /// each subsequent point.
        /// </summary>
        /// <param name="amountOfStylusPoints">Number of new points to add to mapping</param>
        /// <returns>New temporal mapping</returns>
        private Dictionary<Point, Queue<double>> ConstructTemporalMapping(int amountOfStylusPoints)
        {
            var mapping = new Dictionary<Point, Queue<double>>();
            var timestamp = 1000;

            for (var x = 0; x < amountOfStylusPoints; x++)
            {
                var timestampQueue = new Queue<double>();
                timestampQueue.Enqueue(timestamp);

                mapping.Add(new Point(0, x), timestampQueue);
                timestamp += 1000;
            }

            return mapping;
        }

        [Test]
        public void ConstructorNotNullTest()
        {
            Assert.IsNotNull(_component);
        }

        #region SynthesizeTests

        [Test]
        public void SynthesizeTest()
        {
            var cacheSnapshot = new (StylusPointCollection, long)[1];
            cacheSnapshot[0] = (new StylusPointCollection() { new StylusPoint(0, 0)
                , new StylusPoint(0, 1), new StylusPoint(0, 2) }, 1000L);
            _studentStroke.TemporalCacheSnapshot = cacheSnapshot;
            _expertStroke.TemporalCacheSnapshot = cacheSnapshot;

            var result = (LineGraph) _component.Synthesize();
            Assert.AreEqual(2, result.AllSeries.Count);
        }

        [Test]
        public void SynthesizeNoExpertTemporalDataTest()
        {
            var cacheSnapshot = new (StylusPointCollection, long)[1];
            cacheSnapshot[0] = (new StylusPointCollection { new StylusPoint(0, 0)
                , new StylusPoint(0, 1), new StylusPoint(0, 2) }, 1000L);
            _studentStroke.TemporalCacheSnapshot = cacheSnapshot;

            var result = (LineGraph) _component.Synthesize();
            Assert.AreEqual(1, result.AllSeries.Count);
        }
        
        #endregion

        #region CreateTemporalMappingTests

        [Test]
        public void CreateTemporalMappingTest()
        {
            var cacheSnapshot = new (StylusPointCollection, long)[2];
            cacheSnapshot[0] = (new StylusPointCollection { new StylusPoint(0, 0) }, 1000L);
            cacheSnapshot[1] = (new StylusPointCollection { new StylusPoint(1, 0), new StylusPoint(2, 1) }, 2000L);
            _studentStroke.TemporalCacheSnapshot = cacheSnapshot;

            Dictionary<Point, Queue<double>> mapping = SpeedOverProgress.CreateTemporalMapping(_studentTrace);
            
            mapping.TryGetValue(new Point(0, 0), out var result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1000, result.Peek());

            mapping.TryGetValue(new Point(1, 0), out result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2000, result.Peek());

            mapping.TryGetValue(new Point(2, 1), out result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2000, result.Peek());
        }

        [Test]
        public void CreateTemporalMappingMultipleStrokesTest()
        {
            var cacheSnapshotOne = new (StylusPointCollection, long)[1];
            cacheSnapshotOne[0] = (new StylusPointCollection { new StylusPoint(0, 0) }, 1000L);
            _studentStroke.TemporalCacheSnapshot = cacheSnapshotOne;

            var cacheSnapshotTwo = new (StylusPointCollection, long)[1];
            cacheSnapshotTwo[0] = (new StylusPointCollection { new StylusPoint(0, 2) }, 2000L);
            _expertStroke.TemporalCacheSnapshot = cacheSnapshotTwo;

            _studentTrace = new StrokeCollection() { _studentStroke, _expertStroke };

            Dictionary<Point, Queue<double>> mapping = SpeedOverProgress.CreateTemporalMapping(_studentTrace);

            mapping.TryGetValue(new Point(0, 0), out var result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1000, result.Peek());

            mapping.TryGetValue(new Point(0, 2), out result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2000, result.Peek());
        }

        [Test]
        public void CreateTemporalMappingRoundingTest()
        {
            var cacheSnapshot = new (StylusPointCollection, long)[1];
            cacheSnapshot[0] = (new StylusPointCollection { new StylusPoint(0.8, 2.2) }, 1000L);
            _studentStroke.TemporalCacheSnapshot = cacheSnapshot;

            var mapping = SpeedOverProgress.CreateTemporalMapping(_studentTrace);
            
            mapping.TryGetValue(new Point(1, 2), out var result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1000, result.Peek());
        }

        [Test]
        public void CreateTemporalMappingWithEqualCoordinatesTest()
        {
            var cacheSnapshot = new (StylusPointCollection, long)[1];
            cacheSnapshot[0] = (new StylusPointCollection { new StylusPoint(1, 1), new StylusPoint(1, 1) }, 1000L);
            _studentStroke.TemporalCacheSnapshot = cacheSnapshot;

            Dictionary<Point, Queue<double>> mapping = SpeedOverProgress.CreateTemporalMapping(_studentTrace);
            
            mapping.TryGetValue(new Point(1, 1), out var result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1000, result.Dequeue());
            Assert.AreEqual(1000, result.Dequeue());
        }

        [Test]
        public void CreateTemporalMappingFromEmptyCacheTest()
        {
            var cacheSnapshot = new (StylusPointCollection, long)[0];
            _studentStroke.TemporalCacheSnapshot = cacheSnapshot;

            Dictionary<Point, Queue<double>> mapping = SpeedOverProgress.CreateTemporalMapping(_studentTrace);

            Assert.IsEmpty(mapping);
        }
        
        #endregion

        #region GeneratePointsTest

        [Test]
        public void GeneratePointsTest()
        {
            var cacheSnapshot = new (StylusPointCollection, long)[3];
            cacheSnapshot[0] = (new StylusPointCollection { new StylusPoint(0, 0) }, 1000L);
            cacheSnapshot[1] = (new StylusPointCollection { new StylusPoint(0, 1) }, 2000L);
            cacheSnapshot[2] = (new StylusPointCollection { new StylusPoint(0, 2) }, 4000L);
            _studentStroke.TemporalCacheSnapshot = cacheSnapshot;

            List<DataPoint> expectedDataPoints = new List<DataPoint>() { new DataPoint(0, 0), new DataPoint(1, 1), new DataPoint(2, 0.5) };

            Dictionary<Point, Queue<double>> mapping = SpeedOverProgress.CreateTemporalMapping(_studentTrace);
            var (dataPoints, traceLength) = _component.GeneratePoints(_studentTrace, mapping);


            Assert.AreEqual(2, traceLength);
            Assert.AreEqual(expectedDataPoints, dataPoints);
        }

        [Test]
        public void GeneratePointsWithNonCachedPointTest()
        {
            var stylusPoints = new StylusPointCollection() { new StylusPoint(0, 0), new StylusPoint(0, 0.4), new StylusPoint(0, 2) };
            _studentStroke.StylusPoints = stylusPoints;

            var cacheSnapshot = new (StylusPointCollection, long)[3];
            cacheSnapshot[0] = (new StylusPointCollection { new StylusPoint(0, 0) }, 1000L);
            cacheSnapshot[1] = (new StylusPointCollection { new StylusPoint(0, 1) }, 2000L);
            cacheSnapshot[2] = (new StylusPointCollection { new StylusPoint(0, 2) }, 5000L);
            _studentStroke.TemporalCacheSnapshot = cacheSnapshot;

            var expectedDataPoints = new List<DataPoint>() { new DataPoint(0, 0), new DataPoint(2, 0.5) };

            Dictionary<Point, Queue<double>> mapping = SpeedOverProgress.CreateTemporalMapping(_studentTrace);
            var (dataPoints, traceLength) = _component.GeneratePoints(_studentTrace, mapping);

            Assert.AreEqual(2, traceLength);
            Assert.AreEqual(expectedDataPoints, dataPoints);
        }
        
        [Test]
        public void GeneratePointsFirstPointNotFoundTest()
        {
            var cacheSnapshot = new (StylusPointCollection, long)[3];
            
            cacheSnapshot[0] = (new StylusPointCollection { new StylusPoint(1, 0) }, 1000L);
            cacheSnapshot[1] = (new StylusPointCollection { new StylusPoint(0, 1) }, 2000L);
            cacheSnapshot[2] = (new StylusPointCollection { new StylusPoint(0, 2) }, 4000L);
            _studentStroke.TemporalCacheSnapshot = cacheSnapshot;

            var expectedDataPoints = new List<DataPoint>
            {
                new DataPoint(0, 0), new DataPoint(2, 1)
            };

            Dictionary<Point, Queue<double>> mapping = SpeedOverProgress.CreateTemporalMapping(_studentTrace);
            
            
            var (dataPoints, traceLength) = _component.GeneratePoints(_studentTrace, mapping);


            Assert.AreEqual(2, traceLength);
            Assert.AreEqual(expectedDataPoints, dataPoints);
        }

        #endregion

        #region TryGetPointTimestampTests

        [Test]
        public void TryGetPointTimestampTest()
        {
            _component.TryGetPointTimestamp(new Point(0, 4), _testMapping, out double result);
            Assert.AreEqual(5000, result);
        }

        [Test]
        public void TryGetPointTimestampRoundedTest()
        {
            var timestampQueue = new Queue<double>();
            timestampQueue.Enqueue(8000);
            _testMapping.Add(new Point(11, 8), timestampQueue);
            
            _component.TryGetPointTimestamp(new Point(10.8, 8.4), _testMapping, out double result);
            Assert.AreEqual(8000, result);
            Assert.IsEmpty(timestampQueue);
        }

        [Test]
        public void TryGetPointTimestampFirstItemInLongQueueTest()
        {
            var timestampQueue = new Queue<double>();
            timestampQueue.Enqueue(8000);
            timestampQueue.Enqueue(10000);
            timestampQueue.Enqueue(14000);
            _testMapping.Add(new Point(11, 8), timestampQueue);
            
            _component.TryGetPointTimestamp(new Point(11, 8), _testMapping, out double result);
            Assert.AreEqual(8000, result);
            Assert.AreEqual(2, timestampQueue.Count);
        }

        [Test]
        public void TryGetPointTimestampSecondItemInLongQueueTest()
        {
            var timestampQueue = new Queue<double>();
            timestampQueue.Enqueue(5000);
            timestampQueue.Enqueue(9500);
            timestampQueue.Enqueue(1200);
            _testMapping.Add(new Point(11, 8), timestampQueue);
            
            _component.TryGetPointTimestamp(new Point(11, 8), _testMapping, out _);
            _component.TryGetPointTimestamp(new Point(11, 8), _testMapping, out double result);
            Assert.AreEqual(9500, result);
            Assert.AreEqual(1, timestampQueue.Count);
        }

        [Test]
        public void TryGetPointTimestampPeekTest()
        {
            var timestampQueue = new Queue<double>();
            timestampQueue.Enqueue(5000);
            _testMapping.Add(new Point(11, 8), timestampQueue);
            
            _component.TryGetPointTimestamp(new Point(11, 8), _testMapping, out double result, peek: true);
            Assert.AreEqual(5000, result);
            Assert.AreEqual(1, timestampQueue.Count);
        }

        [Test]
        public void TryGetPointTimestampValueNotInMappingTest()
        {
            _component.TryGetPointTimestamp(new Point(100, 200), _testMapping, out double result);
            Assert.AreEqual(double.PositiveInfinity, result);
        }

        [Test]
        public void TryGetPointTimestampQueueEmptyTest()
        {
            _component.TryGetPointTimestamp(new Point(0, 4), _testMapping, out _);
            _component.TryGetPointTimestamp(new Point(0, 4), _testMapping, out double result);
            Assert.AreEqual(double.PositiveInfinity, result);
        }
        
        #endregion

        #region CalculateSpeedTests

        [Test]
        public void CalculateSpeedTest()
        {
            Assert.AreEqual(100, _component.CalculateSpeed(100, 1000));
        }

        [Test]
        public void CalculateSpeedInvalidTimeTest()
        {
            Assert.AreEqual(0, _component.CalculateSpeed(100, -10));
        }

        [Test]
        public void CalculateSpeedInvalidTimeBoundaryTest()
        {
            Assert.AreEqual(0, _component.CalculateSpeed(100, 0));
        }
        
        #endregion

        #region NormalizeAndAverageDataPointsTests

        [Test]
        public void NormalizeAndAverageDataPointsTest()
        {
            var numberOfNeighboursOnSingleSide = NumberOfNeighbours / 2;
            
            var dataPoints = new List<DataPoint>
            {
                new DataPoint(0, 0),
                new DataPoint(2, 2),
                new DataPoint(5, 5)
            };

            var result = new List<DataPoint>
            {
                new DataPoint(0, (2d + (numberOfNeighboursOnSingleSide - 1d) * 5d) / 33d),
                new DataPoint(20, (2d + numberOfNeighboursOnSingleSide * 5d) / 33d),
                new DataPoint(50, (2d + (numberOfNeighboursOnSingleSide + 1) * 5d) / 33d)
            };

            _component.NormalizeAndAverageDataPoints(dataPoints, 10);
            Assert.AreEqual(result, dataPoints);
        }

        [Test]
        public void NormalizeAndAverageEmptyDataPointsTest()
        {
            var dataPoints = new List<DataPoint>();

            _component.NormalizeAndAverageDataPoints(dataPoints, 10);
            Assert.IsEmpty(dataPoints);
        }

        [Test]
        public void NormalizeAndAverageSingleDataPointTest()
        {
            var dataPoints = new List<DataPoint>
            {
                new DataPoint(1, 1),
            };

            var result = new List<DataPoint>
            {
                new DataPoint(50, 1),
            };

            _component.NormalizeAndAverageDataPoints(dataPoints, 2);
            Assert.AreEqual(result, dataPoints);
        }

        [Test]
        public void NormalizeAndAverageDataPointsLongestTraceTest()
        {
            var dataPoints = new List<DataPoint>
            {
                new DataPoint(0, 1),
                new DataPoint(1, 1),
                new DataPoint(2, 1),
            };

            var result = new List<DataPoint>
            {
                new DataPoint(0, 1),
                new DataPoint(50, 1),
                new DataPoint(100, 1),
            };

            _component.NormalizeAndAverageDataPoints(dataPoints, 2);
            Assert.AreEqual(result, dataPoints);
        }

        #endregion
    }
}
