using System.Collections.ObjectModel;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IContactsRepository
    {
        ObservableCollection<IContact> ContactsList { get; set; }
        Contact GetOrCreate(string name, IMicroblogSource source);
        T GetOrCreate<T>(string name, IMicroblogSource source) where T : IContact, new();
    }
}