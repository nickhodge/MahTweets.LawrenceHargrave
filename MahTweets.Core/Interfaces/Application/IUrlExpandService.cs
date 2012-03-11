using System.Threading.Tasks;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IUrlExpandService
    {
        Task<string> ExpandUrl(string shortUrl);
        bool IsShortUrl(string shortUrl);
    }
}