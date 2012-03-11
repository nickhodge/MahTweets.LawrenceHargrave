using System.Windows;

namespace MahTweets.Core.Interfaces.ViewModels
{
    public interface IContainerViewModel
    {
        FrameworkElement View { get; set; }
        int Position { get; set; }
    }
}