using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using HandwritingFeedback.Config;
using HandwritingFeedback.RealtimeFeedback.FeedbackTypes;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet;

namespace HandwritingFeedback.RealtimeFeedback
{
    /// <summary>
    /// Manages the real-time feedback options and populates the list-box in the customize feedback view.
    /// </summary>
    public class CustomizationViewManager
    {
        private readonly ListBox _checkBoxListBox;
        private readonly ListBox _sliderListBox;
        private readonly ApplicationConfig _config;
        private FeedbackType _currSetting;
        

        /// <summary>
        /// Constructor for a CustomizationViewManager.
        /// </summary>
        /// <param name="checkBoxListBox">List-box in which options for real-time feedback should be displayed </param>
        /// <param name="sliderListBox">List-box in which the sliders for the configuration should be displayed </param>
        public CustomizationViewManager(ListBox checkBoxListBox, ListBox sliderListBox)
        {
            this._checkBoxListBox = checkBoxListBox;
            this._sliderListBox = sliderListBox;
            this._config = ApplicationConfig.Instance;
        }

        /// <summary>
        /// Renders all check-boxes and sliders in the Customize Feedback View.
        /// </summary>
        public void LoadOptions()
        {
            // Set item source of checkBoxListBox to newly created list of check-boxes
            _checkBoxListBox.ItemsSource = LoadCheckBoxes();
            // Set item source of sliderListBox to newly created list of sliders
            _sliderListBox.ItemsSource = LoadSliders();
        }
        /// <summary>
        /// Creates all the check boxes by using the <see cref="FeedbackType"/>.
        /// </summary>
        /// <returns>A List of the created check boxes</returns>
        private List<CheckBox> LoadCheckBoxes()
        {
            // Extract current configuration of real-time feedback
            this._currSetting = (FeedbackType)Application.Current.Properties[ApplicationPropertyKeys.RealtimeSetting];

            var options = new List<CheckBox>();

            // Instantiate and render a check-box for each real-time feedback component
            foreach (FeedbackType option in Enum.GetValues(typeof(FeedbackType)))
            {
                CheckBox box = new CheckBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Height = 30.0,
                    FontSize = 20,
                    FontFamily = new FontFamily("Consolas"),
                    // Set text alongside check-box to string representation of feedback option
                    Content = this.ExtractEnumDescriptionOrString(option),
                    // Check box depending on whether it is enabled in current setting
                    IsChecked = _currSetting.HasFlag(option),
                };

                // Configure event handlers for check-box
                box.Checked += (o, e) => Enable(option, _currSetting);
                box.Unchecked += (o, e) => Disable(option, _currSetting);

                // Add new check-box to docking list-box on view
                options.Add(box);
            }

            return options;
        }

        /// <summary>
        /// Creates all the sliders for the config values that should be adjustable in the UI.
        /// </summary>
        /// <returns>A List of the created sliders and their titles</returns>
        private List<UIElement> LoadSliders()
        {
            var titledSliders = new List<UIElement>();
            
            // Add a slider for each UI-adjustable config value below
            titledSliders.AddRange(ConstructTitledSlider(2, 30, _config.MaxDeviationRadius,
                ConfigSliderType.MaxDeviationRadius));
            titledSliders.AddRange(ConstructTitledSlider(0, 1, _config.HighPressureStart,
                ConfigSliderType.HighPressureStart));
            titledSliders.AddRange(ConstructTitledSlider(0, 1, _config.HighPressureCutoff,
                ConfigSliderType.HighPressureCutoff));
            titledSliders.AddRange(ConstructTitledSlider(0, 1, _config.LowPressureStart,
                ConfigSliderType.LowPressureStart));
            titledSliders.AddRange(ConstructTitledSlider(0, 1, _config.LowPressureCutoff,
                ConfigSliderType.LowPressureCutoff));
            titledSliders.AddRange(ConstructTitledSlider(500, 10000, _config.HighSpeedStart,
                ConfigSliderType.HighSpeedStart));
            titledSliders.AddRange(ConstructTitledSlider(0, 90, _config.AngleDeviation, 
                ConfigSliderType.MaxAngleDeviation));
            
            return titledSliders;
        }
        
        /// <summary>
        /// Constructs each slider and the its title by using the provided parameters.
        /// </summary>
        /// <param name="min">The minimum value of the slider</param>
        /// <param name="max">The maximum value of the slider</param>
        /// <param name="currentValue">The current value of the slider</param>
        /// <param name="configSliderId">The unique identifier of the slider. The description of this enum element <br />
        /// will be used for setting the title</param>
        /// <returns>A list containing a TextBlock and a Slider</returns>
        private List<UIElement> ConstructTitledSlider(double min, double max, double currentValue, ConfigSliderType configSliderId)
        {
            var title = new TextBlock
            {
                Text = ExtractEnumDescriptionOrString(configSliderId),
                VerticalAlignment = VerticalAlignment.Center,
                Height = 30.0,
                FontSize = 20,
                FontFamily = new FontFamily("Consolas")
            };
            
            var slider =  new Slider
            {
                Minimum = min,
                Maximum = max,
                Value = currentValue,
                Name = configSliderId.ToString(),
                AutoToolTipPlacement = AutoToolTipPlacement.TopLeft,
                AutoToolTipPrecision = 2
            };
            
            // When the value of the slider is changed, adjust the corresponding config value
            slider.ValueChanged += Slider_ValueChanged;
            
            // Return a list containing the TextBlock and the Slider
            return new List<UIElement> {title, slider};
        }
        
        /// <summary>
        /// Adjust the config value using the current value of the slider.
        /// </summary>
        /// <param name="sender">The slider that is calling the event</param>
        /// <param name="e">Event arguments</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a slider name does <br/>
        /// not match any configurable value</exception>
        public void Slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            var slider = (Slider)sender;
            float sliderValue = (float) slider.Value;
            
            switch ((ConfigSliderType)Enum.Parse(typeof(ConfigSliderType), slider.Name))
            {
                // Add a case for each slider
                case ConfigSliderType.MaxDeviationRadius:
                    _config.MaxDeviationRadius = sliderValue;
                    break;
                case ConfigSliderType.HighPressureStart:
                    _config.HighPressureStart = sliderValue;
                    break;
                case ConfigSliderType.HighPressureCutoff:
                    _config.HighPressureCutoff = sliderValue;
                    break;
                case ConfigSliderType.LowPressureStart:
                    _config.LowPressureStart = sliderValue;
                    break;
                case ConfigSliderType.LowPressureCutoff:
                    _config.LowPressureCutoff = sliderValue;
                    break;
                case ConfigSliderType.HighSpeedStart:
                    _config.HighSpeedStart = sliderValue;
                    break;
                case ConfigSliderType.MaxAngleDeviation:
                    _config.AngleDeviation = (int) sliderValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Given an Enum, converts values to description attribute or string representation of value.
        /// </summary>
        /// <param name="enumVal">Enum type to extract descriptions from</param>
        /// <returns>String representation of enum description or enum type if description annotation unavailable</returns>
        /// <exception cref="InvalidOperationException">Thrown if there is no field information for enumVal</exception>
        public string ExtractEnumDescriptionOrString(Enum enumVal)
        {
            // Obtain all field information from type of the enumVal
            FieldInfo field = enumVal.GetType().GetField(enumVal.ToString());
            
            if (field is null) throw new InvalidOperationException("The enum does not contain the requested field!");
            
            // Obtain an array of description attributes
            var attrs = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            
            // Return the description if attributes obtained were valid
            // else return the result of the ToString method if no attributes were returned
            return attrs.Length > 0 ? attrs[0].Description :  enumVal.ToString();
        }

        /// <summary>
        /// Sets the flag for a given feedback type in the application properties
        /// </summary>
        /// <param name="enumVal">Real-time feedback type to enable</param>
        /// <param name="current">Current setting of real-time feedback</param>
        /// <returns>Updated configuration of real-time feedback</returns>
        public FeedbackType Enable(FeedbackType enumVal, FeedbackType current)
        {
            FeedbackType updated = current | enumVal;
            this._currSetting = updated;
            return updated;
        }

        /// <summary>
        /// Disables the flag for a given feedback type in the application properties
        /// </summary>
        /// <param name="enumVal">Real-time feedback type to disable</param>
        /// <param name="current">Current setting of real-time feedback</param>
        /// <returns>Updated configuration of real-time feedback</returns>
        public FeedbackType Disable(FeedbackType enumVal, FeedbackType current)
        {
            FeedbackType updated = current & ~enumVal;
            _currSetting = updated;
            return updated;
        }

        /// <summary>
        /// Saves the current configuration of real-time feedback components in the visual feedback singleton.
        /// </summary>
        public void SaveConfiguration()
        {
            // Update the feedback configuration in the global view 
            Application.Current.Properties[ApplicationPropertyKeys.RealtimeSetting] = this._currSetting;

            // Destroy current list of input sources in visual feedback singleton
            VisualFeedback.GetInstance().InputSources.Clear();
            AuditoryFeedback.GetInstance().InputSources.Clear();

            // Load any additional input sources below
            VisualFeedback.GetInstance().InputSources.Add(new TabletSource());
            AuditoryFeedback.GetInstance().InputSources.Add(new TabletSource());
        }
    }
}
