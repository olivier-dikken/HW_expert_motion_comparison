using HandwritingFeedback.Models;
using HandwritingFeedback.Util;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for StudentView.xaml
    /// </summary>
    public partial class StudentView : Page, IMenuHeaderControls
    {
        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }

        ObservableCollection<ExerciseItem> exerciseItems;        

        public StudentView()
        {
            HomeCommand = new AnotherCommandImplementation(
                _ =>
                {
                    HomeButton(null, null);
                });
            BackCommand = new AnotherCommandImplementation(
                _ =>
                {
                    BackButton(null, null);
                });

            this.DataContext = this;

            InitializeComponent();

            LoadExercises();
            
        }


        private void LoadExercises()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string path = Directory.GetParent(workingDirectory).Parent.FullName + "\\SavedData\\Exercises\\";
            //FindAvailableExercises(path);
            exerciseItems = FileHandler.GetExerciseItems();
            foreach(ExerciseItem exerciseItem in exerciseItems)
            {
                Card exerciseCard = ExerciseToCard(exerciseItem);
                ExercisePanel.Children.Add(exerciseCard);
            }
        }

        public Card ExerciseToCard(ExerciseItem exerciseItem)
        {
            PaletteHelper _paletteHelper = new PaletteHelper();
            ITheme theme = _paletteHelper.GetTheme();
            MaterialDesignColors.ColorPair playBtnColor = theme.SecondaryMid;

            Card exerciseCard = new Card();            
            exerciseCard.Width = 200;
            exerciseCard.Margin = new Thickness(8);

            Grid cardGrid = new Grid();
            RowDefinition rd1 = new RowDefinition();
            rd1.Height = new GridLength(140);
            RowDefinition rd2 = new RowDefinition();
            rd2.Height = new GridLength(1, GridUnitType.Star);
            RowDefinition rd3 = new RowDefinition();
            rd3.Height = GridLength.Auto;

            cardGrid.RowDefinitions.Add(rd1);
            cardGrid.RowDefinitions.Add(rd2);
            cardGrid.RowDefinitions.Add(rd3);

            Image exerciseImage = new Image();
            exerciseImage.Source = exerciseItem.targetTraceImage;
            Grid.SetRow(exerciseImage, 0);
            cardGrid.Children.Add(exerciseImage);

            StackPanel textPanel = new StackPanel();
            textPanel.Margin = new Thickness(8, 24, 8, 0);
            TextBlock titleText = new TextBlock() { Text = exerciseItem.Title, FontWeight = FontWeights.Bold };
            TextBlock descriptionText = new TextBlock() { Text = exerciseItem.Description, TextWrapping = TextWrapping.Wrap };
            textPanel.Children.Add(titleText);
            textPanel.Children.Add(descriptionText);
            Grid.SetRow(textPanel, 1);
            cardGrid.Children.Add(textPanel);

            
            Button startExerciseButton = new Button();
            PackIcon playIcon = new PackIcon();
            playIcon.Kind = PackIconKind.Play;
            startExerciseButton.Content = playIcon;
            startExerciseButton.Width = 30;
            startExerciseButton.Padding = new Thickness(2,0,2,0);
            startExerciseButton.Background = new SolidColorBrush(playBtnColor.Color);
            startExerciseButton.Click += CardOnClick;
            startExerciseButton.Tag = exerciseItem.path;
            StackPanel bottomPanel = new StackPanel();
            bottomPanel.HorizontalAlignment = HorizontalAlignment.Right;
            bottomPanel.Orientation = Orientation.Horizontal;
            bottomPanel.Children.Add(startExerciseButton);
            bottomPanel.Margin = new Thickness(8);
            Grid.SetRow(bottomPanel, 2);            
            cardGrid.Children.Add(bottomPanel);

            exerciseCard.Content = cardGrid;

            return exerciseCard;
        }

        public void CardOnClick(object sender, RoutedEventArgs e)
        {
            //start corresponding ex
            GlobalState.SelectedExercisePath = ((Button)sender).Tag.ToString();
            Button fakeButton = new Button();
            fakeButton.Tag = "\\View\\UpdatedUI\\StudentPracticeView.xaml";
            CommonUtils.Navigate(fakeButton, null, this);
        }

        private void StartSelectedExercise(object sender, RoutedEventArgs e)
        {
            //Button b = (Button)sender;
            //String text = (string)ListExercises.SelectedItems[0];

            //Debug.WriteLine($"Selected exercise: {text}");

            //string workingDirectory = Environment.CurrentDirectory;
            //string path = Directory.GetParent(workingDirectory).Parent.FullName + "\\SavedData\\Exercises\\";
            //GlobalState.SelectedExercisePath = path + text;
            //b.Tag = "\\View\\PracticeMode.xaml";
            //CommonUtils.Navigate(b, e, this);
        }


        /// <summary>
        /// archived function. moved to filehandler
        /// </summary>
        /// <param name="exerciseFolderPath"></param>
        private void FindAvailableExercises(string exerciseFolderPath)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(exerciseFolderPath, "*", SearchOption.TopDirectoryOnly);
                Debug.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
                foreach (string dir in dirs)
                {
                    string fileName = System.IO.Path.GetFileName(dir);
                    Debug.WriteLine(fileName);
                    //ListExercises.Items.Add(fileName);                    
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("The process failed: {0}", e.ToString());
            }            
        }

        

        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            Button fakeButton = new Button();
            fakeButton.Tag = "\\View\\UpdatedUI\\StudentView.xaml";
            CommonUtils.Navigate(fakeButton, null, this);
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
    }
}
