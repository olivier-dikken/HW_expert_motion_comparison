using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.Config.Visual;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback.InputSources
{
    /// <summary>
    /// Any test for a VisualComponent of an InputSource extends this class.
    /// </summary>
    abstract class VisualComponentTest : ApplicationTest
    {
        protected Pen Pen;
        protected StylusPoint StylusPoint;

        protected abstract VisualComponent VisualComponent { get; }

        /// <summary>
        /// Use this method to configure the FeedbackComponent that the child class is testing. <br />
        /// Remember to instantiate the Config class of the component inside of the ApplicationConfig.
        /// </summary>
        protected abstract void ConfigureComponentAttributes();

        [SetUp]
        public void FeedbackComponentSetUp()
        {
            // Set a config with set custom values to ensure that tests don't
            // start failing when the user changes the values in VisualFeedbackConfig.
            ApplicationConfig.VisualConfig = new VisualConfig
            {
                PenThicknessModifier = 5f,
                DefaultColor = Colors.Black
            };

            ApplicationConfig.ClosestPointScanDiameter = 100d;
            
            // Configure the component's attributes
            ConfigureComponentAttributes();

            Pen = new Pen(new SolidColorBrush(Colors.Black), 1d);
            StylusPoint = new StylusPoint(0, 0, 0.5f);

            PracticeMode.ExpertTraceUtils = new TraceUtils(new StrokeCollection());
            
            // Add a default expert trace
            StylusPoint start = new StylusPoint(0, 5, 0.5f);
            StylusPoint end = new StylusPoint(4, 5, 0.5f);
            AddTraceFromStylusPoints(new StylusPointCollection{ start, end });
        }

        /// <summary>
        /// Takes a StylusPointCollection and adds it to the ExpertTrace collection
        /// as a Stroke. Used for testing.
        /// </summary>
        /// <param name="stylusPoints">The given StylusPointCollection</param>
        protected void AddTraceFromStylusPoints(StylusPointCollection stylusPoints)
        {
            Stroke stroke = new Stroke(stylusPoints);
            PracticeMode.ExpertTraceUtils.Trace.Add(stroke);
        }

        // Helper method to compare two colors based strictly on their ARGB values
        protected static bool ColorEquals(Color c1, Color c2)
        {
            return c1.A == c2.A && c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
        }

        /// <summary>
        /// Helper method to assert that an expected color is equal to the result of calling
        /// the Synthesize method in the <see cref="VisualComponent"/> of the calling class with a
        /// specific Pen and StylusPoint.
        /// </summary>
        /// <param name="expected">The expected color of the pen after feedback has been given</param>
        /// <param name="pen">The pen to be modified</param>
        /// <param name="stylusPoint">The StylusPoint to base the feedback on</param>
        protected void RunAndCompareEquals(Color expected, Pen pen, StylusPoint stylusPoint)
        {
            Color actual = Run(pen, stylusPoint);
            Assert.True(ColorEquals(expected, actual), "Expected " + expected + " - but got " + actual);
        }

        /// <summary>
        /// Helper method to assert that an expected color is equal to the result of calling
        /// the Synthesize method in the <see cref="VisualComponent"/> of the calling class with a
        /// specific Pen and StylusPoint.
        /// </summary>
        /// <param name="expected">The expected color of the pen after feedback has been given</param>
        /// <param name="pen">The pen to be modified</param>
        /// <param name="stylusPoint">The StylusPoint to base the feedback on</param>
        protected void RunAndCompareNotEquals(Color expected, Pen pen, StylusPoint stylusPoint)
        {
            Color actual = Run(pen, stylusPoint);
            Assert.False(ColorEquals(expected, actual), "Was not expecting " + expected + " to equal " + actual);
        }

        /// <summary>
        /// Calls the Synthesize method in the <see cref="VisualComponent"/> of the calling class,
        /// using the provided pen and stylusPoint.
        /// </summary>
        /// <param name="pen">The given pen to modify</param>
        /// <param name="stylusPoint">The StylusPoint to base feedback on</param>
        /// <returns>The pen which was modified by calling Synthesize</returns>
        private Color Run(Pen pen, StylusPoint stylusPoint)
        {
            var input = new RTFInputData {StylusPoint = stylusPoint};

            VisualComponent.Synthesize(pen, input);

            return ((SolidColorBrush) pen.Brush).Color;
        }
    }
}
