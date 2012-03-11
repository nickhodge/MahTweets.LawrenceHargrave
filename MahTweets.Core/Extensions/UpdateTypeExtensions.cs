using System.Collections.Generic;
using System.Linq;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Extensions
{
    public static class UpdateTypeExtensions
    {
        public static bool HasColor(this UpdateType updateType)
        {
            return !string.IsNullOrEmpty(updateType.ColorARGB);
        }

        public static bool Is(this UpdateType updateType, string compare)
        {
            return updateType.Type.Matches(compare);
        }

        public static bool Is(this UpdateType updateType, UpdateType update)
        {
            if (updateType == null || update == null)
            {
                return false;
            }
            return updateType.Type.Matches(update.Type);
        }

        public static bool Is(this UpdateType updateType, string compare, bool ignoreCase)
        {
            return updateType.Type.Matches(compare, ignoreCase);
        }

        public static bool HasType(this IList<UpdateType> updates, string type)
        {
            UpdateType found = updates.Where(x => x.Is(type)).SingleOrDefault();
            return (found != null);
        }

        public static bool HasTypes(this IList<UpdateType> updates, params string[] types)
        {
            return types.All(t => updates.HasType(t));
        }

        public static bool Matches(this IList<UpdateType> updates, UpdateType type)
        {
            if (type == null) return false;

            return updates.Any(t => t.Type == type.Type);
        }

        public static bool HasType<T>(this IList<UpdateType> updates) where T : UpdateType
        {
            return updates.OfType<T>().Any();
        }

        public static void AddUpdate<T>(this IList<UpdateType> updates, IMicroblog parent) where T : UpdateType, new()
        {
            if (updates != null)

                if (!updates.OfType<T>().Any())
                {
                    updates.Add(new T {Parent = parent});
                }
        }
    }
}