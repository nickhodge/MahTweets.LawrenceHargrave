using System;

namespace MahTweets.Core.Composition
{
    public static class Uuid
    {
        //http://stackoverflow.com/questions/529647/need-a-smaller-alternative-to-guid-for-db-id-but-still-unique-and-random-for-url

        public static string NewUuid()
        {
            var _guid = Guid.NewGuid();
            var encoded = Convert.ToBase64String(_guid.ToByteArray());
            encoded = encoded.Replace("/", "_").Replace("+", "-");
            return encoded.Substring(0, 22);
        }
    }
}