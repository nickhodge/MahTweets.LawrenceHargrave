using System.ComponentModel;

namespace MahTweets.Core.Interfaces
{
    public interface IAutoNotifyPropertyChanged : INotifyPropertyChanged
    {
        void OnPropertyChanged(string propertyName);
    }
}