using System.Collections.Generic;
using Hammock.Web;
using MahApps.RESTBase;
using MahApps.Twitter.Delegates;
using MahApps.Twitter.Extensions;
using MahApps.Twitter.Models;

namespace MahApps.Twitter.Methods
{
    public class Block : RestMethodsBase<IBaseTwitterClient>
    {
        private const string BaseAddress = "blocks/";

        public Block(IBaseTwitterClient context)
            : base(context)
        {
        }

        public virtual void BeginBlock(string username, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string> {{"screen_name", username}};

            Context.BeginRequest(BaseAddress + "create.json", p, WebMethod.Post, (req, res, state) =>
                                                                                     {
                                                                                         ITwitterResponse obj =
                                                                                             res.Content.Deserialize
                                                                                                 <User>();
                                                                                         if (callback != null)
                                                                                             callback(req, res, obj);
                                                                                     });
        }

        public virtual void BeginSpamBlock(string username, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string> {{"screen_name", username}};

            Context.BeginRequest("report_spam.json", p, WebMethod.Post, (req, res, state) =>
                                                                            {
                                                                                ITwitterResponse obj =
                                                                                    res.Content.Deserialize<User>();
                                                                                if (callback != null)
                                                                                    callback(req, res, obj);
                                                                            });
        }
    }
}