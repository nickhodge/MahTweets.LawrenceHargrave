using System.Windows.Controls;

namespace MahTweets.Core.Interfaces
{
    public interface IStatusUpdateAttachment
    {
        // should this be something more low level - Visual, UIElement, FrameworkElement?
        UserControl NewView();
        string MappedToUrl();
    }
}