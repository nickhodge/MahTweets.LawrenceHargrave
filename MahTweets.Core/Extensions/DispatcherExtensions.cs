using System;
using System.Windows.Threading;

namespace MahTweets.Core.Extensions
{
    public static class DispatcherExtensions
    {
        [Obsolete]
        public static void Execute(this Dispatcher dispatcher, Action action, params object[] args)
        {
            dispatcher.Invoke(action, DispatcherPriority.Background, args);
        }
    }
}