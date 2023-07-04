using HandwritingFeedback.Models;
using HandwritingFeedback.Util;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for CreateExericseInfo.xaml
    /// </summary>
    public partial class CreateExericseInfo : Page
    {
        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }

        public ObservableCollection<FeedbackSelectionGridItem> FeatureSelectionGridData { get; }

        public string TextBoxExerciseName { get; set; }

        public CreateExericseInfo()
        {
            HomeCommand = new AnotherCommandImplementation(
                _ =>
                {
                    HomeButton(null, null);
                });
            BackCommand = new AnotherCommandImplementation(
                _ =>
                {
                    //TODO message "Are you sure you want to quit?"
                    System.Windows.Controls.Button fakeButton = new System.Windows.Controls.Button();
                    fakeButton.Tag = "\\View\\UpdatedUI\\ManageLearningContent.xaml";
                    BackButton(fakeButton, null);
                });

            FeatureSelectionGridData = PrepareFeatureDataForGrid();
            this.TextBoxExerciseName = "";

            this.DataContext = this;

            InitializeComponent();
        }

       
    private void PreviewTextInputNumerical(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    private ObservableCollection<FeedbackSelectionGridItem> PrepareFeatureDataForGrid()
        {
            ObservableCollection<FeedbackSelectionGridItem> griditems = new ObservableCollection<FeedbackSelectionGridItem>();
            foreach (string ftName in GlobalState.FeatureNames)
            {
                FeedbackSelectionGridItem fbsgi = new FeedbackSelectionGridItem(ftName);
                griditems.Add(fbsgi);
            }
            return griditems;
        }

        public void EnableNext(object sender, RoutedEventArgs e)
        {
            if(NameTextBox.Text.Length > 0)
            {
                TextBoxExerciseName = NameTextBox.Text;
                SaveAndContinueButton.IsEnabled = true;
            } else {
                SaveAndContinueButton.IsEnabled = false;
                Debug.WriteLine("Please fill in exercise name to enable next button. Current contents: " + NameTextBox.Text);
            }
        }

        public async void SaveAndContinue_Async(object sender, RoutedEventArgs e)
        {            
            await FileHandler.UpdateConfigInfoView_Async(NameTextBox.Text, DescriptionTextBox.Text, BasicRatingBar.Value, FeatureSelectionGridData, GlobalState.CreateContentsPreviousFolder, Int32.Parse(RepititionsAmount.Text));

            //go to record performance screen and load correct trace with the specified exercise config conditions
            this.NavigationService.Navigate(new Uri("\\View\\UpdatedUI\\CreateEDMView.xaml", UriKind.Relative));
        }


        public void BackButton(object sender, RoutedEventArgs e)
        {
            //TODO add pop up warning message, confirm discard current creation
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
