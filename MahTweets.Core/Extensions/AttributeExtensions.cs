using System;
using System.Linq;
using System.Reflection;

namespace MahTweets.Core.Extensions
{
    public static class AttributeExtensions
    {
        public static A GetAttribute<A>(this MemberInfo memberInfo) where A : Attribute
        {
            return GetAttribute<A>(memberInfo, true);
        }

        public static A GetAttribute<A>(this MemberInfo memberInfo, bool inherit) where A : Attribute
        {
            A[] atts = GetAttributes<A>(memberInfo, inherit);

            if (atts == null || atts.Length == 0) return null;

            return atts[0];
        }

        public static A[] GetAttributes<A>(this MemberInfo memberInfo) where A : Attribute
        {
            return GetAttributes<A>(memberInfo, true);
        }

        public static A[] GetAttributes<A>(this MemberInfo memberInfo, bool inherit) where A : Attribute
        {
            object[] atts = memberInfo.GetCustomAttributes(typeof (A), inherit);

            return atts.Cast<A>().ToArray();
        }
    }
}