using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.Config;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.Util;
using Newtonsoft.Json;
using NUnit.Framework;

namespace HandwritingFeedbackTests.Util
{
    public class FileHandlerTest
    {
        private StrokeCollection _trace;
        private Stroke _strokeOne;
        private Stroke _strokeTwo;
        private (StylusPointCollection, long)[] _temporalCacheOne;
        private (StylusPointCollection, long)[] _temporalCacheTwo;

        [SetUp]
        public void SetUp()
        {
            _temporalCacheOne = new (StylusPointCollection, long)[2];
            _temporalCacheOne[0] = (new StylusPointCollection{ new StylusPoint(0, 0) }, 0);
            _temporalCacheOne[1] = (new StylusPointCollection{ new StylusPoint(0, 1) }, 1);
            _strokeOne = new Stroke(new StylusPointCollection { new StylusPoint(0, 0), new StylusPoint(0, 1) });
            _strokeOne.AddPropertyData(StrokePropertyIds.TemporalCache, JsonConvert.SerializeObject(_temporalCacheOne));

            _temporalCacheTwo = new (StylusPointCollection, long)[1];
            _temporalCacheTwo[0] = (new StylusPointCollection{ new StylusPoint(0, 2) }, 2);
            _strokeTwo = new Stroke(new StylusPointCollection { new StylusPoint(0, 2) });
            _strokeTwo.AddPropertyData(StrokePropertyIds.TemporalCache, JsonConvert.SerializeObject(_temporalCacheTwo));

            _trace = new StrokeCollection { _strokeOne, _strokeTwo };
        }
        
        [Test]
        public void ParseStrokeCollectionContainsTemporalCache()
        {
            FileHandler.ParseStrokeCollection(_trace);
            
            Assert.AreEqual(_temporalCacheOne, ((GeneralStroke) _trace[0]).TemporalCacheSnapshot);
            Assert.AreEqual(_temporalCacheTwo, ((GeneralStroke) _trace[1]).TemporalCacheSnapshot);
        }
        
        [Test]
        public void ParseStrokeCollectionSomeStrokesContainTemporalCache()
        {
            _strokeTwo.RemovePropertyData(StrokePropertyIds.TemporalCache);
            
            FileHandler.ParseStrokeCollection(_trace);
            
            Assert.AreEqual(_temporalCacheOne, ((GeneralStroke) _trace[0]).TemporalCacheSnapshot);
            Assert.IsNull(((GeneralStroke) _trace[1]).TemporalCacheSnapshot);
        }
        
        [Test]
        public void ParseStrokeCollectionDoesNotContainTemporalCache()
        {
            _strokeOne.RemovePropertyData(StrokePropertyIds.TemporalCache);
            _strokeTwo.RemovePropertyData(StrokePropertyIds.TemporalCache);
            
            FileHandler.ParseStrokeCollection(_trace);
            
            Assert.IsNull(((GeneralStroke) _trace[0]).TemporalCacheSnapshot);
            Assert.IsNull(((GeneralStroke) _trace[1]).TemporalCacheSnapshot);
        }
    }
}