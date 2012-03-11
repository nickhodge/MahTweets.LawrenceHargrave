using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MahTweets.Core.Interfaces;

namespace MahTweets.Core
{
    public static class ExtensionHelpers
    {
        public static bool Matches(this string s, string t, bool matchCase)
        {
            return string.Compare(s, t, matchCase) == 0;
        }

        public static bool Matches(this string s, string t)
        {
            return string.Compare(s, t) == 0;
        }

        public static bool HasItems<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                return false;
            return source.Count() > 0;
        }

        public static bool HasItems<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> exp)
        {
            if (source == null)
                return false;
            return source.Where(exp).Count() > 0;
        }

        public static bool HasOne<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                return false;
            return (source.Count() == 1);
        }

        public static bool HasOne<TSource>(this IEnumerable<TSource> source, Func<TSource,bool> exp)
        {
            if (source == null)
                return false;
            return source.Where(exp).Count() == 1;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable != null)
                foreach (var item in enumerable)
                {
                    action(item);
                }
        }

        public static object[] GetAttributes<T>(this IPluginSettings plugin, string attributeName) where T : Attribute
        {
            var findField = plugin.GetType().GetProperty(attributeName);
            if (findField == null)
                return null;
            else
                return findField.GetCustomAttributes(typeof(T), false);
        }

        public static void SetAttributeValue(this IPluginSettings plugin, string attributeName, object expectedValue)
        {
            var findField = plugin.GetType().GetProperty(attributeName);
            if (findField == null) return;

            object converted = Convert.ChangeType(expectedValue, findField.PropertyType);

            findField.SetValue(plugin, converted, null);
        }

        public static bool HasColor(this UpdateType updateType)
        {
            return !string.IsNullOrEmpty(updateType.ColorARGB);
        }

        public static bool Is(this UpdateType updateType, string compare)
        {
            return (string.Compare(updateType.Type, compare, true) == 0);
        }

        public static bool Is(this UpdateType updateType, UpdateType update)
        {
            if (updateType == null || update == null)
            {
                return false;
            }
            return (string.Compare(updateType.Type, update.Type, true) == 0);
        }

        public static bool Is(this UpdateType updateType, string compare, bool ignoreCase)
        {
            return (string.Compare(updateType.Type, compare, ignoreCase) == 0);
        }

        public static bool HasType(this IList<UpdateType> updates, string type)
        {
            var found = updates.Where(x => x.Is(type)).SingleOrDefault();
            return (found != null);
        }

        public static bool HasTypes(this IList<UpdateType> updates, params string[] types)
        {
            foreach (var t in types)
            {
                if (!updates.HasType(t))
                    return false;
            }

            return true;
        }


        public static bool Matches(this IList<UpdateType> updates, UpdateType type)
        {
            if (type != null)
            {
                foreach (var t in updates)
                {
                    if (t.Type == type.Type)
                        return true;
                }
            }
            return false;
        }


        public static bool HasType<T>(this IList<UpdateType> updates) where T : UpdateType
        {
            var found = updates.OfType<T>().Count();
            return (found > 0);
        }

        public static void AddUpdate<T>(this IList<UpdateType> updates) where T : UpdateType, new()
        {
            if (updates.OfType<T>().Count() == 0)
            {
                updates.Add(new T());
            }
        }

    }
}


