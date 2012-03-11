namespace MahTweets.Core.Views
{
    public interface IView<T>
    {
        T ViewModel { get; }
    }
}