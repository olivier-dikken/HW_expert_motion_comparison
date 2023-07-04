using HandwritingFeedback.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for CreateContentConfigMode.xaml
    /// </summary>
    public partial class CreateContentConfigMode : Page
    {


        public CreateContentConfigMode()
        {
            InitializeComponent();
            LoadUI();
        }

        private void LoadUI()
        {
            //both mouse and stylus enter should clear the text
            this.TextBoxDescription.MouseEnter += new MouseEventHandler(TextBoxDescription_Enter);
            this.TextBoxDescription.StylusEnter += new StylusEventHandler(TextBoxDescription_Enter);

        }

        private void TextBoxDescription_Enter(object sender, EventArgs e)
        {
            if(TextBoxDescription.Text.Trim() == "Enter Description Here")
            {
                TextBoxDescription.Text = "";
            }
        }

        /// <summary>
        /// Navigates to the page indicated by the button tag.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }

        private async void ButtonNext_Async(object Sender, RoutedEventArgs e)
        {            
            //description
            string description = TextBoxDescription.Text;

            //type
            int type = RBTypeTrace.IsChecked == true ? 0 : 1;

            //lines, indexes taken fom TraceUtils.cs
            int lineType = 0;
            if ((bool)RBLinesBase.IsChecked)
                lineType = 1;
            else if ((bool)RBLinesSquare.IsChecked)
                lineType = 2;
            else if ((bool)RBLinesStaved.IsChecked)
                lineType = 3;
            else if ((bool)RBLinesNone.IsChecked)
                lineType = 0;

            //starting point
            int startingPoint = (bool)RBShowStartHidden.IsChecked ? 0 : 1;

            //save config to a text file
            await WriteConfigAsync(description, type, lineType, startingPoint);

            //go to record performance screen and load correct trace with the specified exercise config conditions
            this.NavigationService.Navigate(new Uri("\\View\\CreateContentRecordPerformanceMode.xaml", UriKind.Relative));            
        }


        public static async Task WriteConfigAsync(string desc, int type, int lineType, int startingPoint)
        {
            //    string[] lines =
            //    {
            //        desc,
            //        type.ToString(),
            //        lineType.ToString(),
            //        startingPoint.ToString()
            //    };

            //0=title
            //1=description
            //2=creationDate
            //3=attempts
            //4=bestScore
            string[] lines =
            {                
                "ExerciseTitle",
                desc,
                DateTime.Now.ToString(),
                "0",
                "0",
                 type.ToString(),
                lineType.ToString(),
                startingPoint.ToString()
            };


            string path = GlobalState.CreateContentsPreviousFolder;

            await File.WriteAllLinesAsync(path + "\\exerciseConfig.txt", lines);
        }


    }
}
