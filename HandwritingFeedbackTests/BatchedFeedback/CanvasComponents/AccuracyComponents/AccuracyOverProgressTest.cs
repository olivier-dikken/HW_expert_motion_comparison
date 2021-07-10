using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Util;
using NUnit.Framework;

namespace HandwritingFeedbackTests.BatchedFeedback.CanvasComponents.AccuracyComponents
{
    public class AccuracyOverProgressTest : GraphComponentTest
    {
        private Stroke _studentStroke;
        private Stroke _expertStroke;
        private TraceUtils _studentTraceUtils;
        private TraceUtils _expertTraceUtils;
        private AccuracyOverProgress _accuracyOverProgress;
        
        [SetUp]
        public override void InternalSetUp()
        {
            var studentPoints = new List<Point>();
            var expertPoints = new List<Point>();
            
            for (int i = 0; i <= 20; i++)
            {
                studentPoints.Add(new Point(0, 2 * i));
                expertPoints.Add(new Point(0, i));
            }

            _studentStroke = new Stroke(new StylusPointCollection(studentPoints));
            _expertStroke = new Stroke(new StylusPointCollection(expertPoints));
            _studentTraceUtils = new TraceUtils(new StrokeCollection {_studentStroke});
            _expertTraceUtils = new TraceUtils(new StrokeCollection {_expertStroke});
            _accuracyOverProgress = new AccuracyOverProgress(_studentTraceUtils, 
                _expertTraceUtils, new CalculationHelper());
        }

        protected override void ConfigureComponentAttributes()
        {
            ApplicationConfig.MaxDeviationRadius = 5d;
        }

        [Test]
        public void AccuracyOverProgressConstructorNotNull()
        {
            Assert.IsNotNull(_accuracyOverProgress);
        }
        
        [Test]
        public void AccuracyOverProgressSynthesizeTitle()
        {
            LineGraph lineGraph = (LineGraph) _accuracyOverProgress.Synthesize();
            Assert.AreEqual("Cumulative Accuracy of Student's Trace Compared to Expert's", lineGraph.Title);
        }
        
        [Test]
        public void AccuracyOverProgressSynthesizeCurvedLine()
        {
            LineGraph lineGraph = (LineGraph) _accuracyOverProgress.Synthesize();
            Assert.AreEqual(false, lineGraph.CurvedLine);
        }

        [Test]
        public void AccuracyOverProgressSynthesizeLabels()
        {
            // Switch student trace and expert trace, so that expert trace is longer
            _accuracyOverProgress = new AccuracyOverProgress(_expertTraceUtils, 
                _studentTraceUtils, new CalculationHelper());
            
            LineGraph lineGraph = (LineGraph) _accuracyOverProgress.Synthesize();
            Assert.AreEqual("Expert Trace Completion (%)", lineGraph.XAxisLabel);
            Assert.AreEqual("Accuracy (%)", lineGraph.YAxisLabel);
        }

        [Test]
        public void AccuracyOverProgressExpertShorter()
        {
            var points1 = new List<Point>();
            var points2 = new List<Point>();
            
            foreach (var y in Enumerable.Range(0, 20))
            {
                points1.Add(new Point(0, y));
                points2.Add(new Point(0, 2 * y));
            }
            
            // Student trace has more points
            points1.Add(new Point(4,0));

            _studentStroke = new Stroke(new StylusPointCollection(points1));
            _expertStroke = new Stroke(new StylusPointCollection(points2));
            _accuracyOverProgress = new AccuracyOverProgress(new TraceUtils(new StrokeCollection {_studentStroke}), 
                new TraceUtils(new StrokeCollection {_expertStroke}), new CalculationHelper());

            LineGraph lineGraph = (LineGraph) _accuracyOverProgress.Synthesize();
            Assert.AreEqual("Student Trace Completion (%)", lineGraph.XAxisLabel);
            Assert.AreEqual("Accuracy (%)", lineGraph.YAxisLabel);
        }
        
        [Test]
        public void AccuracyOverProgressSynthesizeSeries()
        {
            LineGraph lineGraph = (LineGraph) _accuracyOverProgress.Synthesize();
            
            var enum1 = lineGraph.AllSeries[0].ItemsSource.GetEnumerator();
            var count1 = 0;
            while (enum1.MoveNext())
            {
                count1++;
            }
            
            Assert.AreEqual(21, count1);
        }
        
        [Test]
        public void SynthesizeErrorZonesTest()
        {
            // The student moves a distance of 2 with every new point. At the half way point, the student
            // starts moving off the expert's trace (the expert's trace is half as long as the student trace).
            // This means tha from the 13th point (out of 20), the student is more than 5 (MaxDeviationRadius=5),
            // units of distance away from the expert's trace resulting in an error being highlighted from x value
            // 65 = 13/20 * 100
            var expectedXValues = new List<double> { 65d, 100d };
            
            LineGraph lineGraph = (LineGraph)_accuracyOverProgress.Synthesize();
            
            Assert.AreEqual(expectedXValues.Count, lineGraph.ErrorZonesXValues.Count);
            
            // Check that all x values are as expected
            for (int i = 0; i < expectedXValues.Count; i++)
            {
                Assert.AreEqual(expectedXValues[i], lineGraph.ErrorZonesXValues[i], 0.01d);
            }
        }
    }
}