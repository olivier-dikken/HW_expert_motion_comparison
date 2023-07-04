using HandwritingFeedback.Util;
using HandwritingFeedback.View.UpdatedUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HandwritingFeedback.Models
{
    internal interface IMenuHeaderControls
    {
        
        public void BackButton(object sender, RoutedEventArgs e);
        public void HomeButton(object sender, RoutedEventArgs e);                
        public void CloseApplicationButton(object sender, RoutedEventArgs e);

        public void SettingsButton(object sender, RoutedEventArgs e);
        public void HelpButton(object sender, RoutedEventArgs e);
        public void ReportBugButton(object sender, RoutedEventArgs e);


        public void CloseApplication()
        {
            Console.WriteLine("Closing Application");
            App.Current.Shutdown();
        }

        public void GoHome(Page calledFromPage)
        {
            Button fakeButton = new Button();
            fakeButton.Tag = "\\View\\UpdatedUI\\MainMenu.xaml";
            CommonUtils.Navigate(fakeButton, null, calledFromPage);
        }
    }
}
