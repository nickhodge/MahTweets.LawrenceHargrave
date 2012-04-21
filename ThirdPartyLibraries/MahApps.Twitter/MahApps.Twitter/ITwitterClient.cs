using System;
using System.Collections.Generic;
using System.Net;
using Hammock;
using MahApps.RESTBase;
using MahApps.Twitter.Models;

namespace MahApps.Twitter
{
    public interface ITwitterClient : IBaseTwitterClient
    {
        WebRequest DelegatedRequest(string url, Format format);
        event TwitterClient.VoidDelegate StreamingReconnectAttemptEvent;
        event TwitterClient.VoidDelegate StreamingDisconnectedEvent;

        [Obsolete("No longer feel that RestRequest is necessary on the callback")]
        IAsyncResult BeginStream(TwitterClient.TweetCallback callback, IEnumerable<string> tracks);

        IAsyncResult BeginStream(Action<RestResponse, ITwitterResponse> callback, IEnumerable<string> tracks);
        void SetOAuthToken(Credentials credentials);
    }
}