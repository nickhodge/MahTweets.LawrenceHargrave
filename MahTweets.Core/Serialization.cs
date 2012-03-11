using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MahTweets.Core
{
    /// <summary>
    /// Helper class for serializing/deserializing objects
    /// </summary>
    public static class Serialization
    {
        #region SerializerType enum

        /// <summary>
        /// Declare the Serializer Type you want to use.
        /// </summary>
        public enum SerializerType
        {
            Xml, // Use DataContractSerializer
            Json // Use DataContractJsonSerializer
        }

        #endregion

        /// <summary>
        /// Deserialize a string to a specific type
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="SerializedString"></param>
        /// <param name="UseSerializer"></param>
        /// <param name="KnownTypes"> </param>
        /// <returns></returns>
        public static T Deserialize<T>(string SerializedString, SerializerType UseSerializer,
                                       IEnumerable<Type> KnownTypes)
        {
            // Get a Stream representation of the string.
            using (Stream s = new MemoryStream(Encoding.UTF8.GetBytes(SerializedString)))
            {
                T item;
                switch (UseSerializer)
                {
                    case SerializerType.Json:
                        // Declare Serializer with the Type we're dealing with.
                        var serJson = new DataContractJsonSerializer(typeof (T));
                        // Read(Deserialize) with Serializer and cast
                        item = (T) serJson.ReadObject(s);
                        break;
                    default:
                        var serXml = new DataContractSerializer(typeof (T), KnownTypes);
                        item = (T) serXml.ReadObject(s);
                        break;
                }
                return item;
            }
        }

        public static string Serialize<T>(T ObjectToSerialize, SerializerType UseSerializer,
                                          IEnumerable<Type> KnownTypes)
        {
            using (var serialiserStream = new MemoryStream())
            {
                string serialisedString;
                switch (UseSerializer)
                {
                    case SerializerType.Json:
                        // init the Serializer with the Type to Serialize
                        var serJson = new DataContractJsonSerializer(typeof (T), KnownTypes);
                        // The serializer fills the Stream with the Object's Serialized Representation.
                        serJson.WriteObject(serialiserStream, ObjectToSerialize);
                        break;
                    default:
                        var serXml = new DataContractSerializer(typeof (T), KnownTypes);
                        serXml.WriteObject(serialiserStream, ObjectToSerialize);
                        break;
                }
                // Rewind the stream to the start so we can now read it.
                serialiserStream.Position = 0;
                using (var sr = new StreamReader(serialiserStream))
                {
                    // Use the StreamReader to get the serialized text out
                    serialisedString = sr.ReadToEnd();
                    sr.Close();
                }
                return serialisedString;
            }
        }
    }
}