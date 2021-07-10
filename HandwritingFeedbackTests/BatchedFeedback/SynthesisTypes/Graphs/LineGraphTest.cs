using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using NUnit.Framework;
using OxyPlot;
using System;
using System.Collections.Generic;
using OxyPlot.Series;

namespace HandwritingFeedbackTests.BatchedFeedback.SynthesisTypes.Graphs
{
    class LineGraphTest
    {
        private LineGraph _synthesis;

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
        public void ConstructorTest()
        {
            Assert.IsNotNull(_synthesis);
        }

        [Test]
        public void ConstructorSeriesTest()
        {
            Assert.IsNotNull(_synthesis.AllSeries);
        }

        [Test]
        public void GetTitle()
        {
            Assert.AreEqual("title", _synthesis.Title);
        }

        [Test]
        public void SetTitleTest()
        {
            _synthesis.Title = "new-title";
            Assert.AreEqual("new-title", _synthesis.Title);
        }

        [Test]
        public void GetXAxisLabelTest()
        {
            Assert.AreEqual("x", _synthesis.XAxisLabel);
        }

        [Test]
        public void SetXAxisLabelTest()
        {
            _synthesis.XAxisLabel = "new-x";
            Assert.AreEqual("new-x", _synthesis.XAxisLabel);
        }

        [Test]
        public void GetYAxisLabelTest()
        {
            Assert.AreEqual("y", _synthesis.YAxisLabel);
        }

        [Test]
        public void SetYAxisLabelTest()
        {
            _synthesis.XAxisLabel = "new-y";
            Assert.AreEqual("new-y", _synthesis.XAxisLabel);
        }

        [Test]
        public void AddSeriesTest()
        {
            var dataPoints = new List<DataPoint>()
            {
                new DataPoint(0, 0),
                new DataPoint(1, 1)
            };
            var newLineSeries = new LineSeries
            {
                Color = OxyColors.Black,
                ItemsSource = dataPoints
            };
            _synthesis.AddSeries(newLineSeries);
            Assert.AreEqual(new List<LineSeries>() { 
                newLineSeries
            }, _synthesis.AllSeries);
        }

        [Test]
        public void AddSeriesWithNullDataTest()
        {
            var emptyLineSeries = new LineSeries
            {
                Color = OxyColors.Black,
                ItemsSource = null
            };
            Assert.Throws<ArgumentNullException>(delegate { _synthesis.AddSeries(emptyLineSeries); });
        }
    }
}
