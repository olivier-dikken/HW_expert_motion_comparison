using NUnit.Framework;
using System.Windows.Input;
using Newtonsoft.Json;
using HandwritingFeedback.Config;
using HandwritingFeedback.StylusPlugins.Strokes;

namespace HandwritingFeedbackTests.StylusPlugins.Strokes
{
    public class GeneralStrokeTest
    {
        private GeneralStroke _generalStroke;
        private (StylusPointCollection, long)  _temporalCacheElementOne;

        [SetUp]
        public void Setup()
        {
            var stylusPoints = new 
                StylusPointCollection { new StylusPoint(1, 2), new StylusPoint(2, 4) };
            _temporalCacheElementOne = (stylusPoints, 1000);

            _generalStroke = new GeneralStroke(stylusPoints);
            GeneralStroke.TemporalCache.Clear();
            GeneralStroke.TemporalCache.Add(_temporalCacheElementOne);
        }

        [Test]
        public void DrawCoreLocalTemporalCacheNullTest()
        {
            _generalStroke.CustomDrawCore();
            Assert.AreEqual(_temporalCacheElementOne, _generalStroke.TemporalCacheSnapshot[0]);
        }

        [Test]
        public void DrawCoreLocalTemporalCacheNullWithMultipleElementsTest()
        {
            (StylusPointCollection, long) temporalCacheElementTwo = (new
                StylusPointCollection { new StylusPoint(2, 4), new StylusPoint(4, 8) }, 2000);
            GeneralStroke.TemporalCache.Add(temporalCacheElementTwo);

            _generalStroke.CustomDrawCore();
            Assert.AreEqual(_temporalCacheElementOne, _generalStroke.TemporalCacheSnapshot[0]);
            Assert.AreEqual(temporalCacheElementTwo, _generalStroke.TemporalCacheSnapshot[1]);
        }

        [Test]
        public void DrawCoreLocalTemporalCacheAlreadySetTest()
        {
            _generalStroke.TemporalCacheSnapshot = new[] { _temporalCacheElementOne };

            Assert.AreEqual(_temporalCacheElementOne, _generalStroke.TemporalCacheSnapshot[0]);
        }

        [Test]
        public void DrawCoreLocalTemporalCacheAlreadySetMultipleElementsTest()
        {
            (StylusPointCollection, int) temporalCacheElementTwo = (new
                StylusPointCollection() { new StylusPoint(2, 4), new StylusPoint(4, 8) }, 2000);
            _generalStroke.TemporalCacheSnapshot = new[] { _temporalCacheElementOne, temporalCacheElementTwo };

            _generalStroke.CustomDrawCore();
            Assert.AreEqual(_temporalCacheElementOne, _generalStroke.TemporalCacheSnapshot[0]);
            Assert.AreEqual(temporalCacheElementTwo, _generalStroke.TemporalCacheSnapshot[1]);
        }

        [Test]
        public void DrawCorePropertyTest()
        {
            (StylusPointCollection, long)[] expectedCache = new[] { _temporalCacheElementOne };
            string expectedProperty = JsonConvert.SerializeObject(expectedCache);

            _generalStroke.CustomDrawCore();

            Assert.AreEqual(expectedProperty, _generalStroke.GetPropertyData(StrokePropertyIds.TemporalCache));
        }
    }
}
