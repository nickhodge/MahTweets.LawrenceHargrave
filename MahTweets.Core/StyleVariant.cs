using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MahTweets.Core
{
    public class StyleVariant : ResourceDictionary
    {
        public String VariantName { get; set; }
        public BitmapImage Preview { get; set; }
    }
}