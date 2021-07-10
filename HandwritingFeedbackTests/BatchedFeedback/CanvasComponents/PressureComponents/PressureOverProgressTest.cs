using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.PressureComponents;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Util;
using NUnit.Framework;
using OxyPlot;

namespace HandwritingFeedbackTests.BatchedFeedback.CanvasComponents.PressureComponents
{
    public class PressureOverProgressTest : GraphComponentTest
    {
        private Stroke _studentStroke;
        private Stroke _expertStroke;
        private PressureOverProgress _pressureOverProgress;
        
        [SetUp]
        public override void InternalSetUp()
        {
            var studentPoints = new List<StylusPoint>();
            var expertPoints = new List<StylusPoint>();

            for (int i = 0; i <= 20; i++)
            {
                // The student moves 1 unit vertically and increases their pressure with every point
                studentPoints.Add(new StylusPoint(0, i, i / 20f));
                // The expert moves 2 units vertically and decreases their pressure with every point
                expertPoints.Add(new StylusPoint(0, 2 * i, 1f - i / 20f));
            }

            _studentStroke = new Stroke(new StylusPointCollection(studentPoints));
            _expertStroke = new Stroke(new StylusPointCollection(expertPoints));
            _pressureOverProgress = new PressureOverProgress(new TraceUtils(new StrokeCollection {_studentStroke}), 
                new TraceUtils(new StrokeCollection {_expertStroke}));
        }

        protected override void ConfigureComponentAttributes()
        {
            ApplicationConfig.HighPressureCutoff = 0.2f;
            ApplicationConfig.HighPressureStart = 0.1f;
            ApplicationConfig.LowPressureCutoff = 0.2f;
            ApplicationConfig.LowPressureStart = 0.1f;
        }

        [Test]
        public void PressureOverProgressConstructorNotNull()
        {
            Assert.IsNotNull(_pressureOverProgress);
        }

        [Test]
        public void PressureOverProgressSynthesizeTitle()
        {
            LineGraph lineGraph = (LineGraph) _pressureOverProgress.Synthesize();
            Assert.AreEqual("Pressure of Student's Trace Compared to Expert's", lineGraph.Title);
        }

        [Test]
        public void PressureOverProgressSynthesizeCurvedLine()
        {
            LineGraph lineGraph = (LineGraph) _pressureOverProgress.Synthesize();
            Assert.AreEqual(false, lineGraph.CurvedLine);
        }

        [Test]
        public void PressureOverProgressSynthesizeLabels()
        {
            LineGraph lineGraph = (LineGraph) _pressureOverProgress.Synthesize();
            Assert.AreEqual("Pressure (%)", lineGraph.YAxisLabel);
        }

        [Test]
        public void PressureOverProgressExpertLonger()
        {
            LineGraph lineGraph = (LineGraph) _pressureOverProgress.Synthesize();
            Assert.AreEqual("Expert Trace Completion (%)", lineGraph.XAxisLabel);
        }

        [Test]
        public void PressureOverProgressStudentLonger()
        {
            _studentStroke.StylusPoints.Add(_expertStroke.StylusPoints);
            LineGraph lineGraph = (LineGraph) _pressureOverProgress.Synthesize();
            Assert.AreEqual("Student Trace Completion (%)", lineGraph.XAxisLabel);
        }

        [Test]
        public void SynthesizeSeriesTest()
        {
            var expectedStudent = new List<DataPoint>();
            for (int i = 0; i <= 20; i++)
            {
                // We expect the student's line on the graph to be half the length of that of the expert horizontally,
                // because the student moved half of the distance. (1/2) * (100% / 20 points) gives 2.5 units per data point.
                // For the y value we simply multiply the pressure by 100 to get the percentage
                expectedStudent.Add(new DataPoint(i * 2.5d, (i/20d) * 100d));
            }
            var expectedExpert = new List<DataPoint>();
            for (int i = 0; i <= 20; i++)
            {
                // We expect the expert's line on the graph to cover the entire x-axis,
                // because they moved the longest distance. 100% / 20 points gives 5 units per data points.
                // For the y value we simply multiply the pressure by 100 to get the percentage
                expectedExpert.Add(new DataPoint(i * 5d, (1 - i/20d) * 100d));
            }
            
            LineGraph lineGraph = (LineGraph)_pressureOverProgress.Synthesize();

            // Check that all data points are as expected
            for (int i = 0; i <= 20; i++)
            {
                Assert.AreEqual(expectedStudent[i].X, ((List<DataPoint>)lineGraph.AllSeries[0].ItemsSource)[i].X, 0.1d);
                Assert.AreEqual(expectedStudent[i].Y, ((List<DataPoint>)lineGraph.AllSeries[0].ItemsSource)[i].Y, 0.1d);
                Assert.AreEqual(expectedExpert[i].X, ((List<DataPoint>)lineGraph.AllSeries[1].ItemsSource)[i].X, 0.1d);
                Assert.AreEqual(expectedExpert[i].Y, ((List<DataPoint>)lineGraph.AllSeries[1].ItemsSource)[i].Y, 0.1d);
            }
        }
        
        [Test]
        public void SynthesizeErrorZonesTest()
        {
            // The student and expert trace intersect in terms of pressure,
            // so before this intersection we expect an error zone (0 to 30) and after this
            // intersection we expect an error zone (37.5 to 50). The LowPressureStart and the
            // HighPressureStart are set to 0.1, which results in these x values.
            var expectedXValues = new List<double> { 0d, 30d, 37.5d, 50d };
            
            LineGraph lineGraph = (LineGraph)_pressureOverProgress.Synthesize();
            
            Assert.AreEqual(expectedXValues.Count, lineGraph.ErrorZonesXValues.Count);
            
            // Check that all x values are as expected
            for (int i = 0; i < expectedXValues.Count; i++)
            {
                Assert.AreEqual(expectedXValues[i], lineGraph.ErrorZonesXValues[i], 0.01d);
            }
        }

        [Test]
        public void TwoSeriesGeneratedTest()
        {
            LineGraph lineGraph = (LineGraph)_pressureOverProgress.Synthesize();
            Assert.AreEqual(2,lineGraph.AllSeries.Count);
        }
    }
}