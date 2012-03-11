using System;
using System.Collections.Generic;
using Hammock.Web;
using MahApps.RESTBase;
using MahApps.Twitter.Delegates;
using MahApps.Twitter.Extensions;
using MahApps.Twitter.Models;

namespace MahApps.Twitter.Methods
{
    public class Friendship : RestMethodsBase<IBaseTwitterClient>
    {
        private String baseAddress = "friendships/";

        public Friendship(IBaseTwitterClient context)
            : base(context)
        {
        }

        public void BeginCreate(string username, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string> {{"screen_name", username}};

            Context.BeginRequest(baseAddress + "create.json", p, WebMethod.Post, (req, res, state) =>
                                                                                     {
                                                                                         ITwitterResponse obj =
                                                                                             res.Content.Deserialize
                                                                                                 <User>();
                                                                                         if (callback != null)
                                                                                             callback(req, res, obj);
                                                                                     });
        }

        public void BeginDestroy(string username, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string> {{"screen_name", username}};

            Context.BeginRequest(baseAddress + "destroy.json", p, WebMethod.Post, (req, res, state) =>
                                                                                      {
                                                                                          ITwitterResponse obj =
                                                                                              res.Content.Deserialize
                                                                                                  <User>();
                                                                                          if (callback != null)
                                                                                              callback(req, res, obj);
                                                                                      });
        }

        public void BeginShow(string Username, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string> {{"target_screen_name", Username}};

            Context.BeginRequest(baseAddress + "show.json", p, WebMethod.Get, (req, res, state) =>
                                                                                  {
                                                                                      ITwitterResponse obj =
                                                                                          res.Content.Deserialize
                                                                                              <RelationshipWrapper>();
                                                                                      if (callback != null)
                                                                                          callback(req, res,
                                                                                                   ((RelationshipWrapper
                                                                                                    ) obj).Relationship);
                                                                                  });
        }
    }
}