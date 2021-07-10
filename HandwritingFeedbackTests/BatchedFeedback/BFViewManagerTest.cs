using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.BatchedFeedback;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.Util;
using NUnit.Framework;

namespace HandwritingFeedbackTests.BatchedFeedback
{
    internal class BFViewManagerTest : ApplicationTest
    {
        private BFViewManager _bfViewManager;
        private TraceUtils _expertTraceUtils;
        private TraceUtils _studentTraceUtils;
        
        [SetUp]
        public override void InternalSetUp()
        {
            var stylusPointsCollection = new StylusPointCollection(new List<Point> {new Point(3, 4)});
            
            var strokeTeacher = new Stroke(stylusPointsCollection);
            var strokeStudent = new StudentStroke(stylusPointsCollection);
            
            _expertTraceUtils = new TraceUtils(new StrokeCollection {strokeTeacher});
            _studentTraceUtils = new TraceUtils(new StrokeCollection {strokeStudent});
            
            var unitDock = new StackPanel();
            var graphDock = new StackPanel();

            var transferDataContainer = new BFInputData()
            {
                GraphDock = graphDock,
                UnitValueDock = unitDock,
                StudentTraceUtils = _studentTraceUtils,
                ExpertTraceUtils = _studentTraceUtils
            };

            strokeStudent.CacheSnapshot = new []{(new StylusPoint(3,4), new Pen())};
            strokeStudent.TemporalCacheSnapshot = new(StylusPointCollection, long)[]
            {
                (new StylusPointCollection { new StylusPoint(3, 4) }, 0)
            };
            _bfViewManager = new BFViewManager(transferDataContainer);
        }

        [Test]
        public void OverallAccuracyIsLastTest()
        {
            Assert.AreEqual(typeof(OverallAccuracy), _bfViewManager.Components.Last().GetType());
        }

        [Test]
        public void PopulateDocksTest()
        {
            Assert.DoesNotThrow(() => _bfViewManager.PopulateDocks());
        }

        [Test]
        public void LoadCanvasTest()
        {
            Assert.DoesNotThrow(() => _bfViewManager.LoadCanvasComponents(_studentTraceUtils, _expertTraceUtils));
            // We cannot know for sure how many components we are initializing since that might
            // change in the future as components get added, but at least one element should be
            // in the list. !Note! The count will be 2 * the expected count since
            // LoadCanvasComponents is called once while constructing the BFViewManager and once
            // while running the test above.
            Assert.GreaterOrEqual(_bfViewManager.Components.Count, 1);
        }
    }
}