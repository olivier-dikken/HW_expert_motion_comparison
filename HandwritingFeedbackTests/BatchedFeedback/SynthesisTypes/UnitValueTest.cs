using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using NUnit.Framework;

namespace HandwritingFeedbackTests.BatchedFeedback.SynthesisTypes
{
    class UnitValueTest
    {
        private const string Value = "dummy";
        private UnitValue _synthesis;

        [SetUp]
        public void Setup()
        {
            _synthesis = new UnitValue(Value);
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_synthesis);
        }

        [Test]
        public void GetValueTest()
        {
            Assert.AreEqual(Value, _synthesis.Value);
        }

        [Test]
        public void GetTitle()
        {
            Assert.AreEqual("", _synthesis.Title);
        }

        [Test]
        public void SetTitleTest()
        {
            _synthesis.Title = "title";
            Assert.AreEqual("title", _synthesis.Title);
        }

        [Test]
        public void GetUnitTest()
        {
            Assert.AreEqual("", _synthesis.Unit);
        }

        [Test]
        public void SetUnitTest()
        {
            _synthesis.Unit = "unit";
            Assert.AreEqual("unit", _synthesis.Unit);
        }
    }
}
