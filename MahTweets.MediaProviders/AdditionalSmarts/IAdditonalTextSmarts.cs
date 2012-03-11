using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace MahTweets.TweetProcessors.AdditionalSmarts
{
    [InheritedExport]
    public interface IAdditonalTextSmarts
    {
        string Name { get; }
        int Priority { get; }
        bool CanPageTitle(string url);
        Task<string> Process(String url);
    }
}