using System.Threading.Tasks;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IManualLongUrlRetrieverService
    {
        Task<string> ExpandUrl(string shortUrl);
        bool IsShortUrl(string shortUrl);
    }
}