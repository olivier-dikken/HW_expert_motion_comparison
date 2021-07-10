using NUnit.Framework;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.SpeedComponents;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.Util;

namespace HandwritingFeedbackTests.BatchedFeedback.CanvasComponents.SpeedComponents
{
    public class CompletionTimeTest
    {
        private CompletionTime _component;
        private StylusPointCollection _stylusPoints;
        private GeneralStroke _studentStroke;
        private GeneralStroke _expertStroke;
        private StrokeCollection _studentTrace;
        private StrokeCollection _expertTrace;

        [SetUp]
        public void Setup()
        {
            // Construct student trace
            _stylusPoints = new 
                StylusPointCollection { new StylusPoint(1, 2), new StylusPoint(2, 4) };
            _studentStroke = new GeneralStroke(_stylusPoints)
            {
                TemporalCacheSnapshot =
                    new[] { (_stylusPoints, 1000L), (_stylusPoints, 2000L), (_stylusPoints, 3000L) }
            };
            _studentTrace = new StrokeCollection() { _studentStroke };

            // Construct expert trace
            // Expert stroke temporal cache must be modified to account for test goals
            _expertStroke = new GeneralStroke(_stylusPoints);
            _expertTrace = new StrokeCollection { _expertStroke };

            _component = new CompletionTime(new TraceUtils(_studentTrace), new TraceUtils(_expertTrace));
        }

        [Test]
        public void ConstructorNotNullTest()
        {
            Assert.IsNotNull(_component);
        }

        [Test]
        public void SynthesizeExpertTakesLongerThanStudent()
        {
            _expertStroke.TemporalCacheSnapshot =
                new[] { (_stylusPoints, 500L), (_stylusPoints, 2000L), (_stylusPoints, 3500L) };

            var result = (UnitValue) _component.Synthesize();
            Assert.AreEqual("-1", result.Value);
        }

        [Test]
        public void SynthesizeEqualTimes()
        {

            _expertStroke.TemporalCacheSnapshot = _studentStroke.TemporalCacheSnapshot;

            var result = (UnitValue) _component.Synthesize();
            Assert.AreEqual("+0", result.Value);
        }

        [Test]
        public void SynthesizeStudentTakesLongerThanExpert()
        {
            _expertStroke.TemporalCacheSnapshot =
                new[] { (_stylusPoints, 1000L), (_stylusPoints, 1800L), (_stylusPoints, 2100L) };

            var result = (UnitValue) _component.Synthesize();
            
            // Region sensitive, as system locale may affect parsing of float using , or .
            Assert.That(result.Value, Is.EqualTo("+0.9").Or.EqualTo("+0,9"));
        }

        [Test]
        public void SynthesizeMultipleStroke()
        {
            _expertStroke.TemporalCacheSnapshot =
                new[] { (_stylusPoints, 1000L), (_stylusPoints, 1800L), (_stylusPoints, 2100L) };

            var expertStrokeTwo = new GeneralStroke(_stylusPoints);

            expertStrokeTwo.TemporalCacheSnapshot = 
                new[] { (_stylusPoints, 3100L), (_stylusPoints, 4500L), (_stylusPoints, 10000L) };
            _expertTrace.Add(expertStrokeTwo);

            var result = (UnitValue) _component.Synthesize();
            Assert.AreEqual("-7", result.Value);
        }

        [Test]
        public void SynthesizeExpertTemporalDataUnavailable()
        {
            var result = (UnitValue) _component.Synthesize();
            Assert.AreEqual("Completion Time", result.Title);
            Assert.AreEqual("+2", result.Value);
        }
    }
}
