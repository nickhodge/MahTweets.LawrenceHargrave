using System.Collections.Generic;
using System.Windows;

namespace MahTweets.Core.Interfaces.ViewModels
{
    public interface IResourcesViewModel
    {
        IEnumerable<ResourceDictionary> Views { get; }
    }
}