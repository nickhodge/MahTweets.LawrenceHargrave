using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MahTweets.UI.Controls
{
    public class ColourableContentPresenter : ContentPresenter
    {
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background",
                                                                                                   typeof (Brush),
                                                                                                   typeof (
                                                                                                       ColourableContentPresenter
                                                                                                       ));

        public Brush Background
        {
            get { return GetValue(BackgroundProperty) as Brush; }
            set { SetValue(BackgroundProperty, value); }
        }
    }
}