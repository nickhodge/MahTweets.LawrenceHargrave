using System;
using System.Collections.Generic;
using MahTweets.Core.Collections;

namespace MahTweets.Core
{
    public class Credential
    {
        public Credential()
        {
            if (CustomSettings == null)
            {
                CustomSettings = new SerializableDictionary<string, string>();
            }
        }

        public String UserID { get; set; }
        public String AccountName { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public String Protocol { get; set; }
        public SerializableDictionary<string, string> CustomSettings { get; set; }
    }

    public class CredentialContainer
    {
        public List<Credential> Credentials { get; set; }
    }
}