using System.Windows;
using System.Windows.Controls;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //bibeg leaving comment to initialise the branch.
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }

    }
}
