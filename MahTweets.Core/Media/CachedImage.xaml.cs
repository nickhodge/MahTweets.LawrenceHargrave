using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MahTweets.Core.Media
{
    public partial class CachedImage
    {
        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register("Url", typeof (Uri),
                                                                                            typeof (CachedImage),
                                                                                            new PropertyMetadata(
                                                                                                OnUrlPropertyChanged));

        public CachedImage()
        {
            InitializeComponent();
        }

        public Uri Url
        {
            get { return (Uri) GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        private static async void OnUrlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var image = d as CachedImage;
            if (image == null) return;

            var url = e.NewValue as Uri;
            if (url == null) return;

            image.RootImage.Source = await GetBitmapAsync(url);
        }

        public static Task<BitmapImage> GetBitmapAsync(Uri url)
        {
            var tcs = new TaskCompletionSource<BitmapImage>();
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.Default;
            bitmap.UriSource = url;
            bitmap.EndInit();
            tcs.SetResult(bitmap);
            return tcs.Task;
        }
    }
}