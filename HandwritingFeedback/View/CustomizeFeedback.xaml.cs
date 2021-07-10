using HandwritingFeedback.RealtimeFeedback;
using HandwritingFeedback.Util;
using System.Windows;
using System.Windows.Controls;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for CustomizeFeedback.xaml
    /// </summary>
    public partial class CustomizeFeedback : Page
    {
        private readonly CustomizationViewManager _manager;
        
        public CustomizeFeedback()
        {
            InitializeComponent();

            // Delegate customization duties to a manager class
            _manager = new CustomizationViewManager(CheckBoxListBox, SliderListBox);

            // Load all real-time feedback options as check-boxes
            _manager.LoadOptions();
        }

        /// <summary>
        /// Navigates to the page indicated by the button tag.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void Navigate(object sender, RoutedEventArgs e)
        {
            // Save new configuration of real-time feedback
            _manager.SaveConfiguration();

            CommonUtils.Navigate(sender, e, this);
        }
    }
}
