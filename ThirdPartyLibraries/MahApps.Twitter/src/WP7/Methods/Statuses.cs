using System;
using System.Collections.Generic;
using Hammock.Web;
using MahApps.RESTBase;
using MahApps.Twitter.Delegates;
using MahApps.Twitter.Extensions;
using MahApps.Twitter.Models;

namespace MahApps.Twitter.Methods
{
    public class Statuses : RestMethodsBase<IBaseTwitterClient>
    {
        private const string baseAddress = "statuses/";

        public Statuses(IBaseTwitterClient context)
            : base(context)
        {
        }

        public virtual void BeginGetTweet(string id, GenericResponseDelegate callback)
        {
            Context.BeginRequest(string.Format("{0}show/{1}.json", baseAddress, id), null, WebMethod.Get,
                                 (req, res, state) =>
                                     {
                                         ITwitterResponse obj = res.Content.Deserialize<Tweet>();
                                         if (callback != null)
                                             callback(req, res, obj);
                                     });
        }

        public virtual void BeginRetweet(String ID, GenericResponseDelegate callback)
        {
            Context.BeginRequest(baseAddress + "retweet/" + ID + ".json", null, WebMethod.Post, (req, res, state) =>
                                                                                                    {
                                                                                                        ITwitterResponse
                                                                                                            obj =
                                                                                                                res.
                                                                                                                    Content
                                                                                                                    .
                                                                                                                    Deserialize
                                                                                                                    <
                                                                                                                        Tweet
                                                                                                                        >
                                                                                                                    ();
                                                                                                        if (callback !=
                                                                                                            null)
                                                                                                            callback(
                                                                                                                req, res,
                                                                                                                obj);
                                                                                                    });
        }

        public virtual void BeginUpdate(string text, string id, double? lat, double? @long,
                                        GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string> {{"status", Context.Encode ? Uri.EscapeDataString(text) : text}};

            if (!string.IsNullOrEmpty(id))
                p.Add("in_reply_to_status_id", id);

            if (lat != null && @long != null)
            {
                p.Add("lat", lat.ToString());
                p.Add("long", @long.ToString());
            }

            Context.BeginRequest(baseAddress + "update.json", p, WebMethod.Post, (req, res, state) =>
                                                                                     {
                                                                                         ITwitterResponse obj =
                                                                                             res.Content.Deserialize
                                                                                                 <Tweet>();
                                                                                         if (callback != null)
                                                                                             callback(req, res, obj);
                                                                                     });
        }

        public virtual void BeginUpdate(string text, string id, GenericResponseDelegate callback)
        {
            BeginUpdate(text, id, null, null, callback);
        }


        public virtual void BeginUpdate(string text, GenericResponseDelegate callback)
        {
            BeginUpdate(text, null, callback);
        }

        public virtual void BeginPublicTimeline(GenericResponseDelegate callback)
        {
            Context.BeginRequest(baseAddress + "public_timeline.json", null, WebMethod.Get, (req, res, state) =>
                                                                                                {
                                                                                                    ITwitterResponse obj
                                                                                                        =
                                                                                                        res.Content.
                                                                                                            Deserialize
                                                                                                            <
                                                                                                                ResultsWrapper
                                                                                                                    <
                                                                                                                        Tweet
                                                                                                                        >
                                                                                                                >();
                                                                                                    if (callback != null)
                                                                                                        callback(req,
                                                                                                                 res,
                                                                                                                 obj);
                                                                                                });
        }

        public virtual void BeginHomeTimeline(GenericResponseDelegate callback)
        {
            BeginHomeTimeline(null, null, null, null, false, false, callback);
        }

        public virtual void BeginHomeTimeline(long Count, GenericResponseDelegate callback)
        {
            BeginHomeTimeline(null, null, Count, null, false, false, callback);
        }

        public virtual void BeginHomeTimeline(long? SinceId, long? MaxId, long? Count, int? Page, bool TrimUser,
                                              bool IncludeEntities, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string>();
            if (SinceId != null)
                p.Add("since_id", SinceId.ToString());

            if (MaxId != null)
                p.Add("max_id", MaxId.ToString());

            if (Count != null)
                p.Add("count", Count.ToString());

            if (Page != null)
                p.Add("page", Page.ToString());

            p.Add("trim_user", TrimUser.ToString());
            p.Add("include_entities", IncludeEntities.ToString());

            Context.BeginRequest(baseAddress + "home_timeline.json", p, WebMethod.Get, (req, res, state) =>
                                                                                           {
                                                                                               ITwitterResponse obj =
                                                                                                   res.Content.
                                                                                                       Deserialize
                                                                                                       <
                                                                                                           ResultsWrapper
                                                                                                               <Tweet>>();
                                                                                               if (callback != null)
                                                                                                   callback(req, res,
                                                                                                            obj);
                                                                                           });
        }


        [Obsolete]
        public virtual void BeginFriendsTimeline(GenericResponseDelegate callback)
        {
            Context.BeginRequest(baseAddress + "friends_timeline.json", null, WebMethod.Get, (req, res, state) =>
                                                                                                 {
                                                                                                     ITwitterResponse
                                                                                                         obj =
                                                                                                             res.Content
                                                                                                                 .
                                                                                                                 Deserialize
                                                                                                                 <
                                                                                                                     ResultsWrapper
                                                                                                                         <
                                                                                                                             Tweet
                                                                                                                             >
                                                                                                                     >();
                                                                                                     callback(req, res,
                                                                                                              obj);
                                                                                                 });
        }

        public virtual void BeginUserTimeline(String Username, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string> {{"screen_name", Username}};

            Context.BeginRequest(baseAddress + "user_timeline.json", p, WebMethod.Get, (req, res, state) =>
                                                                                           {
                                                                                               ITwitterResponse obj =
                                                                                                   res.Content.
                                                                                                       Deserialize
                                                                                                       <
                                                                                                           ResultsWrapper
                                                                                                               <Tweet>>();
                                                                                               callback(req, res, obj);
                                                                                           });
        }

        public virtual void BeginMentions(GenericResponseDelegate callback)
        {
            BeginMentions(null, null, null, null, false, false, callback);
        }

        public virtual void BeginMentions(long? SinceId, long? MaxId, long? Count, int? Page, bool TrimUser,
                                          bool IncludeEntities, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string>();
            if (SinceId != null)
                p.Add("since_id", SinceId.ToString());

            if (MaxId != null)
                p.Add("max_id", MaxId.ToString());

            if (Count != null)
                p.Add("count", Count.ToString());

            if (Page != null)
                p.Add("page", Page.ToString());

            p.Add("trim_user", TrimUser.ToString());
            p.Add("include_entities", IncludeEntities.ToString());

            Context.BeginRequest(baseAddress + "mentions.json", p, WebMethod.Get, (req, res, state) =>
                                                                                      {
                                                                                          ITwitterResponse obj =
                                                                                              res.Content.Deserialize
                                                                                                  <ResultsWrapper<Tweet>
                                                                                                      >();

                                                                                          if (callback != null)
                                                                                              callback(req, res, obj);
                                                                                      });
        }

        public virtual void BeginRetweetedByMe(GenericResponseDelegate callback)
        {
            Context.BeginRequest(baseAddress + "retweeted_by_me.json", null, WebMethod.Get, (req, res, state) =>
                                                                                                {
                                                                                                    ITwitterResponse obj
                                                                                                        =
                                                                                                        res.Content.
                                                                                                            Deserialize
                                                                                                            <
                                                                                                                ResultsWrapper
                                                                                                                    <
                                                                                                                        Tweet
                                                                                                                        >
                                                                                                                >();
                                                                                                    if (callback != null)
                                                                                                        callback(req,
                                                                                                                 res,
                                                                                                                 obj);
                                                                                                });
        }

        public virtual void BeginRetweetedToMe(GenericResponseDelegate callback)
        {
            Context.BeginRequest(baseAddress + "retweeted_to_me.json", null, WebMethod.Get, (req, res, state) =>
                                                                                                {
                                                                                                    ITwitterResponse obj
                                                                                                        =
                                                                                                        res.Content.
                                                                                                            Deserialize
                                                                                                            <
                                                                                                                ResultsWrapper
                                                                                                                    <
                                                                                                                        Tweet
                                                                                                                        >
                                                                                                                >();
                                                                                                    if (callback != null)
                                                                                                        callback(req,
                                                                                                                 res,
                                                                                                                 obj);
                                                                                                });
        }

        public virtual void BeginRetweetedOfMe(GenericResponseDelegate callback)
        {
            Context.BeginRequest(baseAddress + "retweeted_of_me.json", null, WebMethod.Get, (req, res, state) =>
                                                                                                {
                                                                                                    ITwitterResponse obj
                                                                                                        =
                                                                                                        res.Content.
                                                                                                            Deserialize
                                                                                                            <
                                                                                                                ResultsWrapper
                                                                                                                    <
                                                                                                                        Tweet
                                                                                                                        >
                                                                                                                >();
                                                                                                    if (callback != null)
                                                                                                        callback(req,
                                                                                                                 res,
                                                                                                                 obj);
                                                                                                });
        }

        public virtual void BeginDestroy(String Id, GenericResponseDelegate callback)
        {
            Context.BeginRequest(baseAddress + "destroy/" + Id + ".json", null, WebMethod.Post, (req, res, state) =>
                                                                                                    {
                                                                                                        ITwitterResponse
                                                                                                            obj =
                                                                                                                res.
                                                                                                                    Content
                                                                                                                    .
                                                                                                                    Deserialize
                                                                                                                    <
                                                                                                                        ResultsWrapper
                                                                                                                            <
                                                                                                                                User
                                                                                                                                >
                                                                                                                        >
                                                                                                                    ();
                                                                                                        if (callback !=
                                                                                                            null)
                                                                                                            callback(
                                                                                                                req, res,
                                                                                                                obj);
                                                                                                    });
        }
    }
}