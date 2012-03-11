using System.Windows.Threading;

namespace MahTweets.Core.Global
{
    public static class GlobalDispatcher
    {
        public static Dispatcher Dispatcher { get; set; }
    }
}