using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MahTweets.UI.Controls
{
    /// <summary>
    /// Interaction logic for ProgressIndicator.xaml
    /// </summary>
    public partial class ProgressIndicator
    {
        public static readonly DependencyProperty ProgressColourProperty =
            DependencyProperty.RegisterAttached("ProgressColour", typeof (Brush), typeof (ProgressIndicator),
                                                new UIPropertyMetadata(null));

        public ProgressIndicator()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Brush ProgressColour
        {
            get { return (Brush) GetValue(ProgressColourProperty); }
            set { SetValue(ProgressColourProperty, value); }
        }

        public void Stop()
        {
            var s = Resources["animate"] as Storyboard;
            if (s != null) s.Stop();
            Visibility = Visibility.Collapsed;
        }
    }
}