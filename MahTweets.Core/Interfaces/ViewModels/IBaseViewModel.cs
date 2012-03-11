using System.Windows;

namespace MahTweets.Core.Interfaces.ViewModels
{
    public interface IBaseViewModel
    {
        FrameworkElement View { get; set; }
    }
}