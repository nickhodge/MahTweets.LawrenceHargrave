using System.Collections.Generic;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core
{
    public static class PluginEventHandler
    {
        private static readonly Dictionary<string, List<IMicroblog>> EventHookup =
            new Dictionary<string, List<IMicroblog>>();

        public static void RegisterEvent(IMicroblog blog, string eventName)
        {
            lock (EventHookup)
            {
                if (!EventHookup.ContainsKey(eventName))
                {
                    EventHookup[eventName] = new List<IMicroblog>();
                }

                EventHookup[eventName].Add(blog);
            }
        }

        public static void FireEvent(string eventName, IStatusUpdate update, params object[] obj)
        {
            if (EventHookup.ContainsKey(eventName))
            {
                foreach (IMicroblog i in EventHookup[eventName])
                {
                    if (update != null && update.Parents.Contains(i))
                    {
                        bool handled = i.HandleEvent(eventName, update, obj);
                        if (handled)
                            break;
                    }
                }
            }
        }
    }
}