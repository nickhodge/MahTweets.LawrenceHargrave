using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.TwitterPlugin.Logic
{
    public class NormalUpdate : Core.Interfaces.Plugins.NormalUpdate
    {
    }

    public class SelfUpdate : Core.Interfaces.Plugins.SelfUpdate
    {
    }

    public class MentionUpdate : UpdateType
    {
        public MentionUpdate()
        {
            Type = "Mention";
            Order = 4;
            ColorARGB = "#330B99DA";
        }
    }

    public class DirectMessageUpdate : UpdateType
    {
        public DirectMessageUpdate()
        {
            Type = "Direct Message";
            Order = 8;
            ColorARGB = "#33A03808";
        }
    }

    public class SearchUpdate : UpdateType
    {
        public SearchUpdate()
        {
            Type = "Search";
            ColorARGB = "#FFffffff";
        }
    }

    public class SelfMessageUpdate : UpdateType
    {
        public SelfMessageUpdate()
        {
            Type = "My Direct Messages";
            Order = 16;
            ColorARGB = "#33202020";
        }
    }

    public class RetweetUpdate : UpdateType
    {
        public RetweetUpdate()
        {
            Type = "Retweets";
            ColorARGB = "#228408A0";
        }
    }


    public class ListUpdate : UpdateType
    {
        public ListUpdate()
        {
            SaveType = true;
            ColorARGB = "#00ffffff";
        }

        public ListUpdate(string listName) : this()
        {
            Type = listName;
        }
    }
}