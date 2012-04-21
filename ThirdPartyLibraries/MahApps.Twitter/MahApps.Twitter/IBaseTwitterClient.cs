using MahApps.RESTBase;
using MahApps.Twitter.Methods;

namespace MahApps.Twitter
{
    public interface IBaseTwitterClient : IRestClientBase
    {
        //ITwitterResponse Deserialise<T>(string content) where T : ITwitterResponse;
        bool Encode { get; set; }

        Account Account { get; }
        Statuses Statuses { get; }
        Block Block { get; }
        List Lists { get; }
        Search Search { get; }
        FriendsAndFollowers FriendsAndFollowers { get; set; }
        DirectMessages DirectMessages { get; }
        Favourites Favourites { get; }

        Friendship Friendships { get; }
        Users Users { get; }
    }
}