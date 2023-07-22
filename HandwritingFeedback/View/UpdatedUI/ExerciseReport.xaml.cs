using HandwritingFeedback.Models;
using HandwritingFeedback.Templates;
using HandwritingFeedback.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static HandwritingFeedback.Util.ReportUtils;

namespace HandwritingFeedback.View.UpdatedUI
{
    /// <summary>
    /// Interaction logic for ExerciseReport.xaml
    /// </summary>
    public partial class ExerciseReport : Page, IMenuHeaderControls
    {
        public AnotherCommandImplementation HomeCommand { get; }
        public AnotherCommandImplementation BackCommand { get; }

        private ReportUtils reportUtils;
        private List<EDMComparisonResult> _comparisonResult;

        private List<ParameterData> ReportOverviewParameterData;

        public ExerciseReport(BFInputData input, EDMData edmData)
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

            reportUtils = new ReportUtils(input, edmData);
            _comparisonResult = reportUtils.ComparisonResult;
            LoadReportOverviewData();
        }

        private void LoadReportOverviewData()
        {
            ReportOverviewParameterData = new List<ParameterData>();
            foreach (EDMComparisonResult result in _comparisonResult)
            {
                ParameterData parameterData = new ParameterData();
                parameterData.Parameter = result.title;
                parameterData.BigError = result.Value_scores_overall;
                parameterData.Correctness = result.Value_scores_overall;
                ReportOverviewParameterData.Add(parameterData);
            }
            ObservableCollection<ParameterData> observableCollection = new ObservableCollection<ParameterData>(ReportOverviewParameterData);
            IncludedReportOverview.ReportOverviewParameterData = observableCollection;
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            Button fakeButton = new Button();
            fakeButton.Tag = "\\View\\UpdatedUI\\MainMenu.xaml";
            CommonUtils.Navigate(fakeButton, null, this);
        }

        public void HomeButton(object sender, RoutedEventArgs e)
        {
            ((IMenuHeaderControls)this).GoHome(this);
        }

        public void CloseApplicationButton(object sender, RoutedEventArgs e)
        {
            ((IMenuHeaderControls)this).CloseApplication();
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
    }
}
