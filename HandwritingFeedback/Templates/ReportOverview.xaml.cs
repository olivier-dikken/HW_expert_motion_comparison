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

namespace HandwritingFeedback.Templates
{
    /// <summary>
    /// Interaction logic for ReportOverview.xaml
    /// </summary>
    public partial class ReportOverview : UserControl
    {

        public static readonly DependencyProperty ReportOverviewParameterDataProperty =
    DependencyProperty.Register("ReportOverviewParameterData", typeof(ObservableCollection<ParameterData>), typeof(ReportOverview), new PropertyMetadata(null));

        public ObservableCollection<ParameterData> ReportOverviewParameterData
        {
            get { return (ObservableCollection<ParameterData>)GetValue(ReportOverviewParameterDataProperty); }
            set { SetValue(ReportOverviewParameterDataProperty, value); }
        }


        public ReportOverview()
        {
            InitializeComponent();
           
            DataContext = this;
        }        
    }

    public class ParameterData
    {
        public string Parameter { get; set; }
        public float BigError { get; set; }
        public float Correctness { get; set; }
    }
}
