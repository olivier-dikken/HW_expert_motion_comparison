using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace HandwritingFeedback.Models
{
    

    public class ConfirmationDialogViewModel 
    {
        private bool _confirmed = false;


        public void SetConfirmed(object sender, RoutedEventArgs e)
        {
            _confirmed = true;
        }

        public void SetCanceled(object sender, RoutedEventArgs e)
        {
            _confirmed = false;
        }


        public bool getStatus()
        {
            return _confirmed;
        }
    }

    
}
