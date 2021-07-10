using HandwritingFeedback.Config;
using NUnit.Framework;

namespace HandwritingFeedbackTests.BatchedFeedback.CanvasComponents
{
    public abstract class GraphComponentTest : ApplicationTest
    {
        /// <summary>
        /// Use this method to configure the BFComponent that the child class is testing. <br />
        /// Remember to instantiate the Attributes class of the component inside of the ApplicationConfig.
        /// </summary>
        protected abstract void ConfigureComponentAttributes();

        [SetUp]
        public void FeedbackComponentSetUp()
        {
            ConfigureComponentAttributes();
        }
    }
}