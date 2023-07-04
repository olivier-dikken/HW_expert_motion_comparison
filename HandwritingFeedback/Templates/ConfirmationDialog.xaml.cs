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

namespace HandwritingFeedback.Templates
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : UserControl
    {
        public ConfirmationDialog()
        {
            InitializeComponent();
        }

        private bool _confirmed = false;


        public void SetConfirmed(object sender, RoutedEventArgs e)
        {
            _confirmed = true;
        }

        public void SetCanceled(object sender, RoutedEventArgs e)
        {
            _confirmed = false;
        }


        public bool GetStatus()
        {
            return _confirmed;
        }
    }
}
