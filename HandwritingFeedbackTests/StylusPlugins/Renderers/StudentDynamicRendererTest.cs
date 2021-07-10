using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.StylusPlugins.Renderers;
using HandwritingFeedback.StylusPlugins.Wrappers;
using NUnit.Framework;

namespace HandwritingFeedbackTests.StylusPlugins.Renderers
{
    public class StudentDynamicRendererTest : ApplicationTest
    {
        private StudentDynamicRenderer _renderer;

        public override void InternalSetUp()
        {
            _renderer = new StudentDynamicRenderer();
        }

        [Test]
        public void OnDrawTest()
        {
            var wrapper = new DrawingContextWrapper();
            var collection = new StylusPointCollection();
            var geometry = new GeometryGroup();
            var brush = new SolidColorBrush(Colors.Black);
            
            Assert.DoesNotThrow(() => _renderer.OnDraw(wrapper, collection, geometry, brush));
        }
    }
}