using System;
using System.Collections.Generic;
using Hammock;
using Hammock.Web;
using MahApps.RESTBase;
using MahApps.Twitter.Delegates;
using MahApps.Twitter.Extensions;
using MahApps.Twitter.Models;

namespace MahApps.Twitter.Methods
{
    public class Search : RestMethodsBase<IBaseTwitterClient>
    {
        private readonly Func<string, IRestClient> _createClients;
        private String baseAddress = "http://search.twitter.com";
        private String basePath = "search.json";

        public Search(IBaseTwitterClient context)
            : this(context, authority => new RestClient {Authority = authority})
        {
        }


        public Search(IBaseTwitterClient context, Func<string, IRestClient> createClients)
            : base(context)
        {
            _createClients = createClients;
        }

        public void BeginSearch(String q, GenericResponseDelegate callback)
        {
            var p = new Dictionary<string, string> {{"q", q}};

            BeginRequest(p, WebMethod.Get, (req, res, state) =>
                                               {
                                                   ITwitterResponse obj = res.Content.Deserialize<ResultsWrapper>();

                                                   if (callback != null)
                                                       callback(req, res,
                                                                (!(obj is ExceptionResponse))
                                                                    ? ((ResultsWrapper) obj).Results
                                                                    : null);
                                               });
        }


        internal void BeginRequest(IDictionary<string, string> parameters, WebMethod method, RestCallback callback)
        {
            var request = new RestRequest
                              {
                                  Path = basePath,
                                  Method = method
                              };

            if (parameters != null)
            {
                foreach (var p in parameters)
                    request.AddParameter(p.Key, p.Value);
            }

            IRestClient client = _createClients(baseAddress);
            client.BeginRequest(request, callback);
        }

        public void BeginGetSavedSearches(GenericResponseDelegate callback)
        {
            Context.BeginRequest("/saved_searches.json", null, WebMethod.Get, (req, res, state) =>
                                                                                  {
                                                                                      ITwitterResponse obj =
                                                                                          res.Content.Deserialize
                                                                                              <
                                                                                                  ResultsWrapper
                                                                                                      <SavedSearch>>();

                                                                                      if (callback != null)
                                                                                          callback(req, res, obj);
                                                                                  });
        }
    }
}