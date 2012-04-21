using System;
using System.Collections.Generic;
using Hammock.Web;
using MahApps.RESTBase;
using MahApps.Twitter.Delegates;
using MahApps.Twitter.Extensions;
using MahApps.Twitter.Models;

namespace MahApps.Twitter.Methods
{
    // TODO: use /lists.json to query for entire set of lists associated with a user?

    public class List : RestMethodsBase<IBaseTwitterClient>
    {
        public List(IBaseTwitterClient context)
            : base(context)
        {
        }

        public void BeginGetAll(GenericResponseDelegate callback)
        {
            Context.BeginRequest("/lists/all.json", null, WebMethod.Get, (req, res, state) =>
                                                                             {
                                                                                 var obj =
                                                                                     res.Content.DeserializeObject
                                                                                         <List<TwitterList>>();

                                                                                 if (callback == null)
                                                                                     return;

                                                                                 callback(req, res, obj);
                                                                             });
        }

        public void BeginGetAll(string screenName, GenericResponseDelegate callback)
        {
            var parameters = new Dictionary<string, string> {{"screen_name", screenName}};

            Context.BeginRequest("/lists/all.json", parameters, WebMethod.Get, (req, res, state) =>
                                                                                   {
                                                                                       var obj =
                                                                                           res.Content.DeserializeObject
                                                                                               <List<TwitterList>>();

                                                                                       if (callback == null)
                                                                                           return;

                                                                                       callback(req, res, obj);
                                                                                   });
        }

        public void BeginGetList(int listId, GenericResponseDelegate callback)
        {
            var dictionary = new Dictionary<string, string> {{"list_id", listId.ToString()}};

            BeginGetList(dictionary, callback);
        }


        public void BeginGetList(string slug, string ownerScreenName, GenericResponseDelegate callback)
        {
            var dictionary = new Dictionary<string, string> {{"slug", slug}, {"owner_screen_name", ownerScreenName}};

            BeginGetList(dictionary, callback);
        }

        public void BeginGetList(string slug, long ownerId, GenericResponseDelegate callback)
        {
            var dictionary = new Dictionary<string, string> {{"slug", slug}, {"owner_id", ownerId.ToString()}};

            BeginGetList(dictionary, callback);
        }

        private void BeginGetList(IDictionary<string, string> parameters, GenericResponseDelegate callback)
        {
            Context.BeginRequest("/lists/statuses.json", parameters, WebMethod.Get, (req, res, state) =>
                                                                                        {
                                                                                            var obj =
                                                                                                res.Content.
                                                                                                    DeserializeObject
                                                                                                    <List<Tweet>>();

                                                                                            if (callback == null)
                                                                                                return;

                                                                                            callback(req, res, obj);
                                                                                        });
        }


        [Obsolete("Obsoleted by Twitter. Use BeginGetAll")]
        public void BeginGetSubscriptions(string userName, GenericResponseDelegate callback)
        {
            Context.BeginRequest(userName + "/lists/subscriptions.json", null, WebMethod.Get, (req, res, state) =>
                                                                                                  {
                                                                                                      ITwitterResponse
                                                                                                          obj =
                                                                                                              res.
                                                                                                                  Content
                                                                                                                  .
                                                                                                                  Deserialize
                                                                                                                  <
                                                                                                                      ListResult
                                                                                                                      >();

                                                                                                      if (callback !=
                                                                                                          null)
                                                                                                      {
                                                                                                          if (
                                                                                                              obj is
                                                                                                              ListResult)
                                                                                                              callback(
                                                                                                                  req,
                                                                                                                  res,
                                                                                                                  ((
                                                                                                                   ListResult
                                                                                                                   ) obj)
                                                                                                                      .
                                                                                                                      Lists);
                                                                                                          else
                                                                                                              callback(
                                                                                                                  req,
                                                                                                                  res,
                                                                                                                  obj);
                                                                                                      }
                                                                                                  });
        }

        [Obsolete("Deprecated by Twitter. Use BeginGetAll")]
        public void BeginGetUserLists(string Username, GenericResponseDelegate callback)
        {
            Context.BeginRequest(Username + "/lists.json", null, WebMethod.Get, (req, res, state) =>
                                                                                    {
                                                                                        ITwitterResponse obj =
                                                                                            res.Content.Deserialize
                                                                                                <ListResult>();

                                                                                        if (callback != null)
                                                                                        {
                                                                                            if (obj is ListResult)
                                                                                                callback(req, res,
                                                                                                         ((ListResult)
                                                                                                          obj).Lists);
                                                                                            else
                                                                                                callback(req, res, obj);
                                                                                        }
                                                                                    });
        }

        [Obsolete("Deprecated by Twitter. Use BeginGetList")]
        public void BeginGetList(string Username, string Id, long? SinceId, long? MaxId, long? Count, int? Page,
                                 bool IncludeEntities, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string>();
            if (SinceId != null && (long) SinceId > 0)
                p.Add("since_id", SinceId.ToString());

            if (MaxId != null)
                p.Add("max_id", MaxId.ToString());

            if (Count != null)
                p.Add("per_page", Count.ToString());

            if (Page != null)
                p.Add("page", Page.ToString());

            p.Add("include_entities", IncludeEntities.ToString());

            Context.BeginRequest(Username + "/lists/" + Id + "/statuses.json", p, WebMethod.Get, (req, res, state) =>
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
                                                                                                                                 Tweet
                                                                                                                                 >
                                                                                                                         >
                                                                                                                     ();

                                                                                                         if (callback !=
                                                                                                             null)
                                                                                                             callback(
                                                                                                                 req,
                                                                                                                 res,
                                                                                                                 obj);
                                                                                                     });
        }
    }
}