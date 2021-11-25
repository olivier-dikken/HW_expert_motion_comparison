using System;
using System.Diagnostics;
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
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {            
            CommonUtils.Navigate(sender, e, this);
        }

        private void StartExercise(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            GlobalState.SelectedExercise = Int32.Parse((string)b.Tag);          
            b.Tag = "\\View\\PracticeMode.xaml";
            CommonUtils.Navigate(b, e, this);
        }

    }
}
