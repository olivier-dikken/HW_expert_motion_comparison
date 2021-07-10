using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.InkCanvases;
using HandwritingFeedback.StylusPlugins.Strokes;
using NUnit.Framework;

namespace HandwritingFeedbackTests.InkCanvases
{
    public class ExpertInkCanvasTest : ApplicationTest
    {

        private ExpertInkCanvas _canvas;
        public override void InternalSetUp()
        {
            _canvas = new ExpertInkCanvas();
        }

        [Test]
        public void OnStrokeCollectedTest()
        {
            Stroke stroke = new Stroke(new StylusPointCollection
            {
                new StylusPoint(0, 0, 0.5f)
            });
            InkCanvasStrokeCollectedEventArgs e = new InkCanvasStrokeCollectedEventArgs(stroke);
            _canvas.CustomOnStrokeCollected(e);

            Assert.False(_canvas.Strokes.Contains(stroke));
            GeneralStroke toAdd = new GeneralStroke(e.Stroke.StylusPoints);
            Assert.AreEqual(_canvas.Strokes[0].StylusPoints, toAdd.StylusPoints);
            Assert.AreEqual(_canvas.Strokes[0].GetType(), typeof(GeneralStroke));
        }

        [Test]
        public void ResetTest()
        {
            _canvas.Strokes = new StrokeCollection
            {
                new Stroke
                (
                    new StylusPointCollection
                    {
                        new StylusPoint(1, 2),
                        new StylusPoint(4, 5)
                    }
                )
            };
            
            _canvas.Reset();
            
            Assert.AreEqual(0, _canvas.Strokes.Count);
            Assert.AreEqual(new StrokeCollection(), _canvas.Strokes);
        }
    }
}