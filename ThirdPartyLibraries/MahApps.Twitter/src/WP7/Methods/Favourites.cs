using System;
using Hammock.Web;
using MahApps.RESTBase;
using MahApps.Twitter.Delegates;
using MahApps.Twitter.Extensions;
using MahApps.Twitter.Models;

namespace MahApps.Twitter.Methods
{
    public class Favourites : RestMethodsBase<IBaseTwitterClient>
    {
        private String baseAddress = "favorites/";

        public Favourites(IBaseTwitterClient context)
            : base(context)
        {
        }

        public virtual void BeginGetFavourites(GenericResponseDelegate callback)
        {
            Context.BeginRequest("favorites.json", null, WebMethod.Get, (req, res, state) =>
                                                                            {
                                                                                ITwitterResponse obj =
                                                                                    res.Content.Deserialize
                                                                                        <ResultsWrapper<Tweet>>();
                                                                                if (callback != null)
                                                                                    callback(req, res, obj);
                                                                            });
        }

        public virtual void BeginCreate(String ID, GenericResponseDelegate callback)
        {
            Context.BeginRequest(baseAddress + "create/" + ID + ".json", null, WebMethod.Post, (req, res, state) =>
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

        public virtual void BeginDestroy(String ID, GenericResponseDelegate callback)
        {
            Context.BeginRequest(baseAddress + "destroy/" + ID + ".json", null, WebMethod.Post, (req, res, state) =>
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
    }
}