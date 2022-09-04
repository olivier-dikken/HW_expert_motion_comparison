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
    /// Interaction logic for TeacherView.xaml
    /// </summary>
    public partial class TeacherView : Page, IMenuHeaderControls
    {
        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }

        public TeacherView()
        {
            HomeCommand = new AnotherCommandImplementation(
                _ =>
                {
                    HomeButton(null, null);
                });
            BackCommand = new AnotherCommandImplementation(
                _ =>
                {
                    HomeButton(null, null);
                });

            this.DataContext = this;
            InitializeComponent();
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }

        public void CloseApplicationButton(object sender, RoutedEventArgs e)
        {
            ((IMenuHeaderControls)this).CloseApplication();
        }

        public void HelpButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void HomeButton(object sender, RoutedEventArgs e)
        {
            ((IMenuHeaderControls)this).GoHome(this);
        }

        public void ReportBugButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SettingsButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }
    }
}
