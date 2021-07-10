using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using NUnit.Framework;

using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.Util;

namespace HandwritingFeedbackTests.BatchedFeedback.CanvasComponents.AccuracyComponents
{
    public class OverallAccuracyTest
    {
        private Stroke _stroke1;
        private Stroke _stroke2;
        private OverallAccuracy _overallAccuracy;
        
        [SetUp]
        public void Setup()
        {
            _stroke1 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(10, 2)}));
            _stroke2 = new Stroke(new StylusPointCollection( new List<Point> {new Point(0, 2), new Point(20, 2)}));
            _overallAccuracy = new OverallAccuracy(new TraceUtils(new StrokeCollection {_stroke1}), 
                new TraceUtils(new StrokeCollection {_stroke2}), new CalculationHelper());
        }

        [Test]
        public void OverallAccuracyConstructorNotNull()
        {
            Assert.IsNotNull(_overallAccuracy);
        }

        [Test]
        public void OverallAccuracySynthesizeValue()
        {
            UnitValue unitValue = (UnitValue) _overallAccuracy.Synthesize();
            Assert.AreEqual("50",unitValue.Value);
        }
        
        [Test]
        public void OverallAccuracySynthesizeUnit()
        {
            UnitValue unitValue = (UnitValue) _overallAccuracy.Synthesize();
            Assert.AreEqual("%",unitValue.Unit);
        }
        
        [Test]
        public void OverallAccuracySynthesizeTitle()
        {
            UnitValue unitValue = (UnitValue) _overallAccuracy.Synthesize();
            Assert.AreEqual("Overall accuracy", unitValue.Title);
        }
    }
}