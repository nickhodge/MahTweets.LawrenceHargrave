using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;
using LinqToTwitter;
using System.Globalization;
using System.Diagnostics;

namespace MahTweets2.TwitterPlugin
{
    public class MahTweetsOAuthAuthorization : OAuthAuthorization
    {
        private string requestToken;
        public MahTweetsOAuthTokenManager tokenManager;

        public MahTweetsOAuthAuthorization() : this(TwitterServiceDescription)
        {

        }

        public MahTweetsOAuthAuthorization(ServiceProviderDescription description)
            : base(new DesktopConsumer(description, new MahTweetsOAuthTokenManager()))
        {
            tokenManager = this.Consumer.TokenManager as MahTweetsOAuthTokenManager;
            
        }

        private new DesktopConsumer Consumer
        {
            get { return (DesktopConsumer)base.Consumer; }
        }

        public override bool CachedCredentialsAvailable
        {
            get {
                if (!String.IsNullOrEmpty(tokenManager.AccessToken) && !String.IsNullOrEmpty(tokenManager.AccessTokenSecret))
                    return true;
                else
                    return false;
            }
        }

        protected override string AccessToken
        {
            get { return tokenManager.AccessToken; }
        }

        public Uri BeginAuthorize()
        {
            return this.Consumer.RequestUserAuthorization(null, null, out this.requestToken);
        }

        public override IDictionary<string, string> Authorize()
        {
            if (this.GetVerifier == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0}.GetVerifier must be set first.", this.GetType().Name));
            }

            Process.Start(this.BeginAuthorize().AbsoluteUri);
            return this.CompleteAuthorize();
        }

        public IDictionary<string, string> CompleteAuthorize()
        {
            if (this.GetVerifier == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0}.GetVerifier must be set first.", this.GetType().Name));
            }

            var response = this.Consumer.ProcessUserAuthorization(this.requestToken, this.GetVerifier());

            return response.ExtraData;
        }

        public Func<string> GetVerifier { get; set; }
    }
}
