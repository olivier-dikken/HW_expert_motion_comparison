using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.InkCanvases;
using HandwritingFeedback.StylusPlugins.Strokes;
using NUnit.Framework;

namespace HandwritingFeedbackTests.InkCanvases
{
    public class StudentInkCanvasTest : ApplicationTest
    {
        public override void InternalSetUp() {}
        
        [Test]
        public void OnStrokeCollectedTest()
        {
            var canvas = new StudentInkCanvas();
            var stroke = new Stroke(new StylusPointCollection
            {
                new StylusPoint(0, 0, 0.5f)
            });
            var e = new InkCanvasStrokeCollectedEventArgs(stroke);
            canvas.CustomOnStrokeCollected(e);
            
            Assert.False(canvas.Strokes.Contains(stroke));
            
            var toAdd = new StudentStroke(e.Stroke.StylusPoints);
            
            Assert.AreEqual(canvas.Strokes[0].StylusPoints, toAdd.StylusPoints);
            Assert.AreEqual(canvas.Strokes[0].GetType(), typeof(StudentStroke));
        }
    }
}