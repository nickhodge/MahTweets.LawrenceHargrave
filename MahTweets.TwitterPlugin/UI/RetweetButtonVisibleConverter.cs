using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MahTweets.TwitterPlugin.Logic;
using MahTweets.TwitterPlugin.Views;

namespace MahTweets.TwitterPlugin.UI
{
    public class RetweetButtonVisibleConverter : IValueConverter
        // If the user is "protected" the policy is to NOT permit RT from their account. This disables (visible=collapsed) for elements
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tv = value as TweetView;
            if (tv == null) return Visibility.Visible;
            Tweet t = tv.Tweet;
            if (t == null) return Visibility.Visible;
            var cc = t.Contact as TwitterContact;

            bool c = cc != null && cc.IsProtected;

            return c ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}