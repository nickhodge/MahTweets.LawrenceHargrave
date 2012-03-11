using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static bool HasItems<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                return false;
            return source.Any();
        }

        public static bool HasItems<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> exp)
        {
            if (source == null)
                return false;
            return source.Any(exp);
        }

        public static bool HasOne<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                return false;
            return (source.Count() == 1);
        }

        public static bool HasOne<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> exp)
        {
            if (source == null)
                return false;
            return source.Count(exp) == 1;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null) return;
            foreach (T item in enumerable)
            {
                action(item);
            }
        }

        public static object[] GetAttributes<T>(this IPluginSettings plugin, string attributeName) where T : Attribute
        {
            PropertyInfo findField = plugin.GetType().GetProperty(attributeName);
            if (findField == null)
                return null;

            return findField.GetCustomAttributes(typeof (T), false);
        }

        public static void SetAttributeValue(this IPluginSettings plugin, string attributeName, object expectedValue)
        {
            PropertyInfo findField = plugin.GetType().GetProperty(attributeName);
            if (findField == null) return;

            object converted = Convert.ChangeType(expectedValue, findField.PropertyType);

            findField.SetValue(plugin, converted, null);
        }
    }
}