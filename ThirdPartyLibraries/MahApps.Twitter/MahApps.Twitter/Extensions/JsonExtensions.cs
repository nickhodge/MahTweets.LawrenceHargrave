using System;
using MahApps.Twitter.Models;
using Newtonsoft.Json;

namespace MahApps.Twitter.Extensions
{
    public static class JsonExtensions
    {
        public static ITwitterResponse Deserialize<T>(this string json) where T : ITwitterResponse
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(json);
                return obj;
            }
            catch (Exception ex)
            {
                return new ExceptionResponse
                           {
                               Content = json,
                               ErrorMessage = ex.Message
                           };
            }
        }

        public static T DeserializeObject<T>(this string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(json);
                return obj;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}