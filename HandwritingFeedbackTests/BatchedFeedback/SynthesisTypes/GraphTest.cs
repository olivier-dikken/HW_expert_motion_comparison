using System;
using System.Collections.Generic;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using NUnit.Framework;
using OxyPlot;
using OxyPlot.Series;

namespace HandwritingFeedbackTests.BatchedFeedback.SynthesisTypes
{
    public class GraphTest
    {
        private Graph _synthesis;

        [SetUp]
        public void Setup()
        {
            _synthesis = new LineGraph()
            {
                Title = "title",
                XAxisLabel = "x",
                YAxisLabel = "y",
            };
        }
        
        [Test]
        public void AddSeriesTest()
        {
            var dataPoints = new List<DataPoint>()
            {
                new DataPoint(0, 0),
                new DataPoint(1, 1)
            };
            var barSeries = new BarSeries()
            {
                ItemsSource = dataPoints
            };
            _synthesis.AddSeries(barSeries);
            Assert.AreEqual(new List<Series>() { 
                barSeries
            }, _synthesis.AllSeries);
        }
        
        [Test]
        public void AddSeriesWithNullDataTest()
        {
            Assert.Throws<ArgumentNullException>(() => _synthesis.AddSeries(null));
        }
    }
}