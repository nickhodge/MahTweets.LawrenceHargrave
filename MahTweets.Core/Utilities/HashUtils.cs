using System;
using System.Security.Cryptography;
using System.Text;

namespace MahTweets.Core.Utilities
{
    public static class Hash
    {
        public static string GetSHA1(string source)
        {
            return BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(
                Encoding.Default.GetBytes(source))).Replace("-", String.Empty);
        }
    }
}