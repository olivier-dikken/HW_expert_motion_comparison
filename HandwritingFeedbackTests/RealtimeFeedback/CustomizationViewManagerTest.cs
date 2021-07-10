using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using HandwritingFeedback.Config;
using HandwritingFeedback.RealtimeFeedback;
using NUnit.Framework;

namespace HandwritingFeedbackTests.RealtimeFeedback
{
    class CustomizationViewManagerTest : ApplicationTest
    {
        private CustomizationViewManager _manager;
        private ListBox _checkBoxes;
        private ListBox _sliders;
        
        private enum Colors
        {
            [System.ComponentModel.Description("Red")]
            R,
            [System.ComponentModel.Description("Green")]
            G,
            Blue
        }

        public override void InternalSetUp()
        {
            _checkBoxes = new ListBox();
            _sliders = new ListBox();
            
            _manager = new CustomizationViewManager(_checkBoxes, _sliders);
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_manager);
        }

        [Test]
        public void LoadOptionsCheckBoxesTest()
        {
            // Set dummy application instance real-time configuration
            Application.Current.Properties[ApplicationPropertyKeys.RealtimeSetting] = ~0;

            _manager.LoadOptions();
            Assert.IsInstanceOf(typeof(List<CheckBox>), _checkBoxes.ItemsSource);
            
        }
        
        [Test]
        public void LoadOptionsSlidersTest()
        {
            _manager.LoadOptions();
            
            Assert.IsInstanceOf(typeof(List<UIElement>), _sliders.ItemsSource);
            // The number of items inside of the list needs to be even since we always
            // add 2 elements to it, one text block and one slider
            Assert.AreEqual(0, _sliders.Items.Count % 2);
        }

        [Test]
        public void SliderValueChangedAccuracyTest()
        {
            TestSlider(ConfigSliderType.MaxDeviationRadius, 42);
            Assert.AreEqual(42,ApplicationConfig.Instance.MaxDeviationRadius);
        }
        
        [Test]
        public void SliderValueChangedPressureTest()
        {
            TestSlider(ConfigSliderType.LowPressureStart, 34);
            TestSlider(ConfigSliderType.LowPressureCutoff, 98);
            TestSlider(ConfigSliderType.HighPressureStart, 74);
            TestSlider(ConfigSliderType.HighPressureCutoff, 33);
            
            Assert.AreEqual(34, ApplicationConfig.LowPressureStart);
            Assert.AreEqual(98, ApplicationConfig.LowPressureCutoff);
            Assert.AreEqual(74, ApplicationConfig.HighPressureStart);
            Assert.AreEqual(33, ApplicationConfig.HighPressureCutoff);
        }

        [Test]
        public void SliderValueChangedTiltTest()
        {
            TestSlider(ConfigSliderType.MaxAngleDeviation, 68);
            
            Assert.AreEqual(68, ApplicationConfig.AngleDeviation);
        }
        
        [Test]
        public void ExtractEnumDescriptionTest()
        {
            Assert.AreEqual("Red", _manager.ExtractEnumDescriptionOrString(Colors.R));
        }

        [Test]
        public void ExtractEnumStringTest()
        {
            Assert.AreEqual("Blue", _manager.ExtractEnumDescriptionOrString(Colors.Blue));
        }

        [Test]
        public void EnableTest()
        {
            FeedbackType result = _manager.Enable(FeedbackType.AccuracyColor, (FeedbackType)0);
            Assert.IsTrue(result.HasFlag(FeedbackType.AccuracyColor));
        }

        [Test]
        public void EnableMultipleTest()
        {
            FeedbackType temp = _manager.Enable(FeedbackType.AccuracyColor, (FeedbackType)0);
            FeedbackType result = _manager.Enable(FeedbackType.PressureColor, temp);
            Assert.IsTrue(result.HasFlag(FeedbackType.AccuracyColor));
        }

        [Test]
        public void EnableCheckOtherFlagsTest()
        {
            FeedbackType result = _manager.Enable(FeedbackType.AccuracyColor, (FeedbackType)0);
            Assert.IsFalse(result.HasFlag(FeedbackType.PressureColor));
        }

        [Test]
        public void DisableTest()
        {
            FeedbackType result = _manager.Disable(FeedbackType.AccuracyColor, (FeedbackType)~0);
            Assert.IsFalse(result.HasFlag(FeedbackType.AccuracyColor));
        }

        [Test]
        public void DisableCheckOtherFlagsTest()
        {
            FeedbackType result = _manager.Disable(FeedbackType.AccuracyColor, (FeedbackType)~0);
            Assert.IsTrue(result.HasFlag(FeedbackType.PressureColor));
        }

        [Test]
        public void DisableMultipleTest()
        {
            FeedbackType temp = _manager.Disable(FeedbackType.AccuracyColor, (FeedbackType)~0);
            FeedbackType result = _manager.Disable(FeedbackType.PressureColor, temp);
            Assert.IsFalse(result.HasFlag(FeedbackType.AccuracyColor));
        }
        
        private void TestSlider(ConfigSliderType configSliderId, int newValue)
        {
            var slider =  new Slider
            {
                Minimum = 0,
                Maximum = 100,
                Value = newValue,
                Name = configSliderId.ToString(),
                AutoToolTipPlacement = AutoToolTipPlacement.TopLeft,
                AutoToolTipPrecision = 2
            };

            Assert.DoesNotThrow(() => _manager.Slider_ValueChanged(slider, new RoutedEventArgs()));
        }
        
    }
}