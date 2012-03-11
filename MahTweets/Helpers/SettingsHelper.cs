using System;
using System.Collections.Generic;
using System.Linq;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Factory;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Interfaces.ViewModels;
using MahTweets.Core.Settings;
using MahTweets.ViewModels;

namespace MahTweets.Helpers
{
    public class SettingsHelper
    {
        public static void SaveSettings(IAccountSettingsProvider accounts,
                                        IPluginRepository pluginRepository)
        {
            // clear whatever is cached currently
            accounts.Reset();

            foreach (IMicroblog c in pluginRepository.Microblogs)
            {
                accounts.MicroblogCredentials.Add(
                    new Credential
                        {
                            UserID = c.Credentials.UserID,
                            AccountName = c.Credentials.AccountName,
                            Username = c.Credentials.Username,
                            Password = c.Credentials.Password,
                            Protocol = c.Credentials.Protocol,
                            CustomSettings = c.Credentials.CustomSettings
                        });
            }


            foreach (IUrlShortener s in pluginRepository.UrlShorteners)
            {
                accounts.UrlShortenerCredentials.Add(
                    new Credential
                        {
                            UserID = s.Credentials.UserID,
                            AccountName = s.Credentials.AccountName,
                            Username = s.Credentials.Username,
                            Password = s.Credentials.Password,
                            Protocol = s.Credentials.Protocol,
                            CustomSettings = s.Credentials.CustomSettings
                        });
            }
            IEnumerable<IStatusHandler> StatusHandlers = CompositionManager.GetAll<IStatusHandler>();
            foreach (IStatusHandler sh in StatusHandlers)
            {
                if (sh.Credentials != null)
                    accounts.StatusHandlerCredentials.Add(
                        new Credential
                            {
                                UserID = sh.Credentials.UserID,
                                AccountName = sh.Credentials.AccountName,
                                Username = sh.Credentials.Username,
                                Password = sh.Credentials.Password,
                                Protocol = sh.Credentials.Protocol,
                                CustomSettings = sh.Credentials.CustomSettings
                            });
            }
        }

        public static void SaveSettings(
            IMainViewModel viewModel,
            IAccountSettingsProvider accounts,
            IColumnsSettingsProvider columns,
            IApplicationSettingsProvider applicationSettings)
        {
            columns.Filters.Clear();
            columns.Columns.Clear();

            for (int i = 0; i < viewModel.StreamContainers.Count; i++)
                // ordering by each streamcontainer to get them visual sequentual
            {
                IContainerViewModel container = viewModel.StreamContainers[i];
                if (container is StreamViewModel)
                {
                    StreamModel f = ((StreamViewModel) container).StreamConfiguration.Filters;
                    columns.Filters.Add(f);
                    ColumnConfiguration cc = ((StreamViewModel) container).GetColumnConfiguration();
                    if (cc.Position == 0)
                        cc.Position = (i + 1);
                    columns.Columns.Add(cc);
                }
            }

            columns.Save();

            accounts.Save();

            applicationSettings.SavedSearches.Clear();

            foreach (ISearchViewModel s in viewModel.CurrentSearches)
            {
                var savedSearch = new SavedSearch
                                      {
                                          Position = s.Position,
                                          SearchTerm = s.SearchTerm,
                                          Providers = s.SearchProviders.Select(sp => sp.Protocol).ToList()
                                      };

                applicationSettings.SavedSearches.Add(savedSearch);
            }

            applicationSettings.SelectedAccounts.Clear();

            foreach (IMicroblog s in viewModel.SelectedMicroblogs)
            {
                applicationSettings.SelectedAccounts.Add(s.Id);
            }


            applicationSettings.Save();
        }

        public SettingsUserControl GetSettingsControl<T>(T plugin) where T : IPlugin
        {
            Type type = plugin.GetType();
            object[] attributes = type.GetCustomAttributes(false);
            foreach (object attribute in attributes)
            {
                if (attribute is SettingsClassAttribute)
                {
                    var ctrlAttr = attribute as SettingsClassAttribute;
                    return CompositionManager.Get<SettingsUserControl>(ctrlAttr.Value);
                }
            }

            return null;
        }
    }
}