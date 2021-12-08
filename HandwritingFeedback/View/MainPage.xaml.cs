using System;
using System.Diagnostics;
using System.IO;
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

            LoadExercises();
        }

        private void LoadExercises()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string path = Directory.GetParent(workingDirectory).Parent.FullName + "\\SavedData\\Exercises\\";
            FindAvailableExercises(path);
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

        private void StartSelectedExercise(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            String text = (string)ListExercises.SelectedItems[0];

            Debug.WriteLine($"Selected exercise: {text}");

            string workingDirectory = Environment.CurrentDirectory;
            string path = Directory.GetParent(workingDirectory).Parent.FullName + "\\SavedData\\Exercises\\";
            GlobalState.SelectedExercisePath = path + text;
            b.Tag = "\\View\\PracticeMode.xaml";
            CommonUtils.Navigate(b, e, this);
        }

        private void FindAvailableExercises(string exerciseFolderPath)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(exerciseFolderPath, "*" , SearchOption.TopDirectoryOnly);
                Debug.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
                foreach (string dir in dirs)
                {
                    string fileName = Path.GetFileName(dir);
                    Debug.WriteLine(fileName);
                    ListExercises.Items.Add(fileName);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("The process failed: {0}", e.ToString());
            }
        }

    }
}
