using System.Collections.ObjectModel;

namespace MahTweets.Core.Interfaces.Settings
{
    public interface IGlobalExcludeSettings
    {
        ObservableCollection<string> GlobalExcludeItems { get; set; }
        void Add(string newAddition);
        void Remove(string deleteAddition);
        void Read();
        void Write();
    }

}
