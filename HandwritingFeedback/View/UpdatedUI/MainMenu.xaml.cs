using HandwritingFeedback.Models;
using HandwritingFeedback.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page, IMenuHeaderControls
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Pressed back button while in main menu. Cannot go back further.");
        }

        /// <summary>
        /// override to avoid reloading main menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HomeButton(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Pressed home button while in main menu. Cannot go back further.");
        }
        
        public void SettingsButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void HelpButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void ReportBugButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void CloseApplicationButton(object sender, RoutedEventArgs e)

        {
            ((IMenuHeaderControls)this).CloseApplication();
        }
    }

    
}
