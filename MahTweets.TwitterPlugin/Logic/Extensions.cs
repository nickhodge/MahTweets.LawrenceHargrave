using System;
using BoxKite.Twitter.Models;

namespace MahTweets.TwitterPlugin.Logic
{
    public static class Extensions
    {
        public static void UpdateContactWithTwitterUser(this TwitterContact contact, User user,
                                                        DateTime? updateTimestamp)
        {
            if (user == null || contact == null)
                return;

            contact.FullName = user.Name;
            contact.Bio = user.Description;
            contact.Followers = user.FollowersCount;
            contact.Following = user.FriendsCount;

            var imageUri = new Uri(user.ProfileImageUrl);
            if (contact.ImageUrl != imageUri)
            {
                contact.SetContactImage(imageUri, updateTimestamp);
            }
        }
    }
}