namespace MahTweets.Core.Interfaces.Application
{
    public interface IShell
    {
        bool Visible { get; }
        void Start();
        void SetWidth(double width);
        void SetHeight(double height);
        void SetTop(double top);
        void SetLeft(double left);
        void DisplayWindow();
        void HideWindow();
    }
}