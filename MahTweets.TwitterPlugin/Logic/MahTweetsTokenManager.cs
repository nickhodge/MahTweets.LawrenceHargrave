using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;
using LinqToTwitter;
using System.Configuration;

namespace MahTweets2.TwitterPlugin
{
    public class MahTweetsOAuthTokenManager : IConsumerTokenManager
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        public string RequestToken { get; set; }
        public string RequestTokenSecret { get; set; }

        public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret)
        {
            this.RequestToken = null;
            this.RequestTokenSecret = null;
            this.AccessToken = accessToken;
            this.AccessTokenSecret = accessTokenSecret;
        }

        public string GetTokenSecret(string token)
        {
            if (token == this.RequestToken)
            {
                return this.RequestTokenSecret;
            }
            else if (token == this.AccessToken)
            {
                return this.AccessTokenSecret;
            }
            else
            {
                throw new ArgumentOutOfRangeException("token");
            }
        }

        public TokenType GetTokenType(string token)
        {
            throw new NotImplementedException();
        }

        public bool IsRequestTokenAuthorized(string requestToken)
        {
            throw new NotImplementedException();
        }

        public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response)
        {
            this.RequestToken = response.Token;
            this.RequestTokenSecret = response.TokenSecret;
        }
    }
}
