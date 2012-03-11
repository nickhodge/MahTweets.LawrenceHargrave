using System;
using System.Collections.Generic;
using Hammock.Web;
using MahApps.RESTBase;
using MahApps.Twitter.Delegates;
using MahApps.Twitter.Extensions;
using MahApps.Twitter.Models;

namespace MahApps.Twitter.Methods
{
    public class DirectMessages : RestMethodsBase<IBaseTwitterClient>
    {
        private String baseAddress = "direct_messages";

        public DirectMessages(IBaseTwitterClient context)
            : base(context)
        {
        }

        public void BeginDirectMessages(GenericResponseDelegate callback)
        {
            BeginDirectMessages(null, null, null, null, false, false, callback);
        }

        public void BeginDirectMessages(long? SinceId, long? MaxId, long? Count, int? Page, bool TrimUser,
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

            Context.BeginRequest(baseAddress + ".json", p, WebMethod.Get, (req, res, state) =>
                                                                              {
                                                                                  ITwitterResponse obj =
                                                                                      res.Content.Deserialize
                                                                                          <ResultsWrapper<DirectMessage>
                                                                                              >();

                                                                                  if (callback != null)
                                                                                      callback(req, res, obj);
                                                                              });
        }

        public void BeginSentDirectMessages(GenericResponseDelegate callback)
        {
            BeginSentDirectMessages(null, null, null, null, false, false, callback);
        }

        public void BeginSentDirectMessages(long? SinceId, long? MaxId, long? Count, int? Page, bool TrimUser,
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

            Context.BeginRequest(baseAddress + "/sent.json", p, WebMethod.Get, (req, res, state) =>
                                                                                   {
                                                                                       ITwitterResponse obj =
                                                                                           res.Content.Deserialize
                                                                                               <
                                                                                                   ResultsWrapper
                                                                                                       <DirectMessage>>();

                                                                                       if (callback != null)
                                                                                           callback(req, res, obj);
                                                                                   });
        }

        public void BeginCreate(string screenName, string text, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string>
                        {
                            {"screen_name", screenName},
                            {"text", Context.Encode ? Uri.EscapeDataString(text) : text}
                        };

            Context.BeginRequest(baseAddress + "/new.json", p, WebMethod.Post, (req, res, state) =>
                                                                                   {
                                                                                       ITwitterResponse obj =
                                                                                           res.Content.Deserialize
                                                                                               <DirectMessage>();

                                                                                       if (callback != null)
                                                                                           callback(req, res, obj);
                                                                                   });
        }
    }
}