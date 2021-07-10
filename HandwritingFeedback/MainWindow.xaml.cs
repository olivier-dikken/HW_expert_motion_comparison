using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using HandwritingFeedback.Config;
using HandwritingFeedback.View;

namespace HandwritingFeedback
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainPage Page;

        public MainWindow()
        {
            this.InitializeComponent();

            // Maximize window size
            Application.Current.MainWindow.WindowState = WindowState.Maximized;

            // Disable navigation bar
            navFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;

            // Enable select real-time feedback components on application start-up
            // In order to minimize cognitive load, real-time feedback should be balanced over the different feedback
            // modalities (i.e. visual, auditory, haptic)
            Application.Current.Properties[ApplicationPropertyKeys.RealtimeSetting] = 
                FeedbackType.AccuracyColor | FeedbackType.PressureAudio;

            // Navigate to main page
            Page ??= new MainPage();
            navFrame.NavigationService.Navigate(Page);

            // Disable navigation gestures
            navFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, OnBrowse));
            navFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, OnBrowse));
            navFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseHome, OnBrowse));
            navFrame.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseStop, OnBrowse));
        }
                
        // Navigation gestures trigger this (do nothing).
        private void OnBrowse(object sender, ExecutedRoutedEventArgs args)
        {
        }
    }
}
