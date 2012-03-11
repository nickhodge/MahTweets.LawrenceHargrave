using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MahTweets.Helpers
{
    public static class SecurityHelper
    {
        public static string GetNewSalt()
        {
            return "12345";
        }

        public static T GetValue<T>(XElement value)
        {
            var x = (value.FirstNode as XElement);
            if (x != null)
            {
                var xs = new XmlSerializer(typeof (T));
                XmlReader reader = x.CreateReader();
                object obj = xs.Deserialize(reader);

                return (T) obj;
            }

            return (T) Convert.ChangeType(value.Value, typeof (T));
        }

        public static void SetValue<T>(XElement element, string key, object value)
        {
            var xs = new XmlSerializer(typeof (T));
            element.RemoveAll();
            element.Add(xs.SerializeAsXElement(value));
        }

        public static XElement SerializeAsXElement(this XmlSerializer xs, object o)
        {
            var d = new XDocument();
            using (XmlWriter w = d.CreateWriter()) xs.Serialize(w, o);
            XElement e = d.Root;
            if (e != null)
            {
                e.Remove();
                return e;
            }
            return null;
        }
    }
}