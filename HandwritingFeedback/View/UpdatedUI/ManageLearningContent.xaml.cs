using HandwritingFeedback.Models;
using HandwritingFeedback.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;


namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for ManageLearningContent.xaml
    /// </summary>
    public partial class ManageLearningContent : Page, IMenuHeaderControls
    {
        public ObservableCollection<ExerciseItem> ExerciseItems { get; }
        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }


        public ManageLearningContent()
        {            
            ExerciseItems = FileHandler.GetExerciseItems();                     

            HomeCommand = new AnotherCommandImplementation(
                _ =>
                {                                        
                    HomeButton(null, null);
                });
            BackCommand = new AnotherCommandImplementation(
                _ =>
                {
                    Button fakeButton = new Button();
                    fakeButton.Tag = "\\View\\UpdatedUI\\TeacherView.xaml";
                    BackButton(fakeButton, null);
                });
            this.DataContext = this;

            InitializeComponent();
        }

        public void ListDeleteButton(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Task Delete Button pressed:");
            Debug.WriteLine(((Button)sender).Tag.ToString());
        }

        public void ListEditButton(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Task Edit Button pressed:");
            Debug.WriteLine(((Button)sender).Tag.ToString());
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }

        public void AddExercise(object sender, RoutedEventArgs e)
        {

        }

        public void EditExercise(object sender, RoutedEventArgs e)
        {

        }

        public void DeleteExercise(object sender, RoutedEventArgs e)
        {

        }

        


        public void BackButton(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
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

        public void HomeButton(object sender, RoutedEventArgs e)
        {
            ((IMenuHeaderControls)this).GoHome(this);
        }
    }
}
