using System;
using System.Windows;
using System.Windows.Controls;

namespace HandwritingFeedback.Util
{
    public class CommonUtils
    {
        /// <summary>
        /// Takes a page as target and navigates it to the page provided in the tag of the calling Button object.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        /// <param name="target">The page to navigate</param>
        public static void Navigate(object sender, RoutedEventArgs e, Page target)
        {
            string className = ((Button)sender).Tag.ToString();
            target.NavigationService.Navigate(new Uri(className, UriKind.Relative));
        }
    }
}
