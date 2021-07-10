using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.TiltComponents;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.Util;
using NUnit.Framework;

namespace HandwritingFeedbackTests.BatchedFeedback.CanvasComponents.TiltComponents
{
    /// <summary>
    /// Test class for tilt range component of batched feedback.
    /// </summary>
    public class TiltRangeTest : ApplicationTest
    {
        private StudentStroke _stroke1;
        private StudentStroke _stroke2;
        private TiltRange _component;

        [SetUp]
        public override void InternalSetUp()
        {
            ApplicationConfig.StylusPointDescription = new StylusPointDescription(new[]
            {
                new StylusPointPropertyInfo(StylusPointProperties.X),
                new StylusPointPropertyInfo(StylusPointProperties.Y),
                new StylusPointPropertyInfo(StylusPointProperties.NormalPressure),
                new StylusPointPropertyInfo(StylusPointProperties.XTiltOrientation)
            });
            
            // Create first stroke with all positive values for tilt angles
            int[] tiltValues1 = { 4800, 2400, 5200, 800, 2000 };
            _stroke1 = new StudentStroke(CreatePoints(tiltValues1));

            // Create second stroke with a mix of positive and negative values for tilt angles
            int[] tileValues2 = { -2900, 4800, -400, 1500, 800 };
            _stroke2 = new StudentStroke(CreatePoints(tileValues2));
            
        }

        /// <summary>
        /// Creates a list of new stylus points at co-ordinates (0, x), where x is an index in the
        /// provided array. These points contain the given tilt values.
        /// </summary>
        /// <param name="tiltValues">The tilt values for the produced points</param>
        /// <returns>List of stylus points containing tilt</returns>
        private StylusPointCollection CreatePoints (IEnumerable<int> tiltValues)
        {
            return new StylusPointCollection(tiltValues.Select((tilt, index) =>
                new StylusPoint(0, index, 0.5f, ApplicationConfig.StylusPointDescription, new[] {tilt})));
        }

        [Test]
        public void ConstructorNotNull()
        {
            _component = new TiltRange(new TraceUtils(new StrokeCollection { _stroke1 }), null);

            Assert.IsNotNull(_component);
        }

        [Test]
        public void SynthesizeTest()
        {
            _component = new TiltRange(new TraceUtils(new StrokeCollection { _stroke1 }), null);

            Assert.AreEqual("38 to 82", ((UnitValue) _component.Synthesize()).Value);
        }

        [Test]
        public void SynthesizeTestNegativeValues()
        {
            _component = new TiltRange(new TraceUtils(new StrokeCollection { _stroke2 }), null);

            Assert.AreEqual("42 to 86", ((UnitValue) _component.Synthesize()).Value);
        }

        [Test]
        public void SynthesizeTestMultipleStrokes()
        {
            _component = new TiltRange(new TraceUtils(new StrokeCollection { _stroke1, _stroke2 }), null);

            Assert.AreEqual("38 to 86", ((UnitValue) _component.Synthesize()).Value);
        }

        [Test]
        public void SynthesizeTestTiltUnavailable()
        {
            var pointsWithoutTilt = new StylusPointCollection();
            
            foreach (var x in Enumerable.Range(1, 10))
            {
                pointsWithoutTilt.Add(new StylusPoint(0, x));
            }
            
            _component = new TiltRange(new TraceUtils(new StrokeCollection { new StudentStroke(pointsWithoutTilt) }), null);

            Assert.AreEqual("N/A", ((UnitValue) _component.Synthesize()).Value);
        }

        [Test]
        public void FormatRangeTest()
        {
            _component = new TiltRange(new TraceUtils(new StrokeCollection { _stroke1 }), null);
            Assert.AreEqual("5 to 10", ((UnitValue) _component.FormatRange(500, 1000)).Value);
        }

        [Test]
        public void FormatRangeTestTiltUnavailable()
        {
            _component = new TiltRange(new TraceUtils(new StrokeCollection { _stroke1 }), null);
            Assert.AreEqual("N/A", ((UnitValue) _component.FormatRange(0, 0, available: false)).Value);
        }
    }
}
