using System;
using System.Collections.Generic;
using System.IO;
using Hammock.Web;
using MahApps.RESTBase;
using MahApps.Twitter.Delegates;
using MahApps.Twitter.Extensions;
using MahApps.Twitter.Models;
using File = MahApps.RESTBase.File;

namespace MahApps.Twitter.Methods
{
    public class Account : RestMethodsBase<IBaseTwitterClient>
    {
        public Account(IBaseTwitterClient context)
            : base(context)
        {
        }

        public void BeginVerifyCredentials(GenericResponseDelegate callback)
        {
            Context.BeginRequest("account/verify_credentials.json", null, WebMethod.Get, (req, res, state) =>
                                                                                             {
                                                                                                 ITwitterResponse obj =
                                                                                                     res.Content.
                                                                                                         Deserialize
                                                                                                         <User>();
                                                                                                 if (callback != null)
                                                                                                     callback(req, res,
                                                                                                              obj);
                                                                                             });
        }


        public void BeginVerifyCredentials(Action<User> callback)
        {
            Context.BeginRequest("account/verify_credentials.json", null, WebMethod.Get, (req, res, state) =>
                                                                                             {
                                                                                                 var user =
                                                                                                     res.Content.
                                                                                                         Deserialize
                                                                                                         <User>() as
                                                                                                     User;
                                                                                                 if (user != null &&
                                                                                                     callback != null)
                                                                                                     callback(user);
                                                                                             });
        }

        public void BeginUpdateProfileImage(FileInfo f, GenericResponseDelegate callback)
        {
            var files = new Dictionary<string, File>();

            if (f.Exists)
                files.Add("image", new File(f.FullName, f.Name));

            Context.BeginRequest("account/update_profile_image.json", null, files, WebMethod.Post, (req, res, state) =>
                                                                                                       {
                                                                                                           ITwitterResponse
                                                                                                               obj =
                                                                                                                   res.
                                                                                                                       Content
                                                                                                                       .
                                                                                                                       Deserialize
                                                                                                                       <
                                                                                                                           User
                                                                                                                           >
                                                                                                                       ();

                                                                                                           if (
                                                                                                               callback !=
                                                                                                               null)
                                                                                                               callback(
                                                                                                                   req,
                                                                                                                   res,
                                                                                                                   obj);
                                                                                                       });
        }

        public void BeginUpdateProfileImage(FileInfo f, Action<User> callback)
        {
            var files = new Dictionary<string, File>();

            if (f.Exists)
                files.Add("image", new File(f.FullName, f.Name));

            Context.BeginRequest("account/update_profile_image.json", null, files, WebMethod.Post, (req, res, state) =>
                                                                                                       {
                                                                                                           var user =
                                                                                                               res.
                                                                                                                   Content
                                                                                                                   .
                                                                                                                   Deserialize
                                                                                                                   <User
                                                                                                                   >()
                                                                                                               as User;
                                                                                                           if (user !=
                                                                                                               null &&
                                                                                                               callback !=
                                                                                                               null)
                                                                                                               callback(
                                                                                                                   user);
                                                                                                       });
        }
    }
}