using System.ComponentModel.Composition;
using System.Windows;

namespace MahTweets.TwitterPlugin.Resources
{
    [Export(typeof (ResourceDictionary))]
    public partial class Resources
    {
        public Resources()
        {
            InitializeComponent();
        }
    }
}