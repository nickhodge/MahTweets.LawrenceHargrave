using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MahTweets.Core.Composition;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Interfaces.ViewModels;
using MahTweets.Core.ViewModels;

namespace MahTweets.ViewModels
{
    public class AppViewModel : BaseViewModel
    {
        private readonly IAccountSettingsProvider _accounts;
        private readonly IApplicationSettingsProvider _applicationSettings;
        private readonly IColumnsSettingsProvider _columnsSettings;
        private readonly IPluginLoader _loader;
        private readonly IPluginRepository _pluginRepository;

        public AppViewModel(IPluginRepository pluginRepository,
                            IAccountSettingsProvider accounts,
                            IApplicationSettingsProvider applicationSettings,
                            IColumnsSettingsProvider columnsSettings,
                            Func<IPluginLoader> createPluginLoader,
                            Func<ResourcesViewModel> createResourceViewModel)
        {
            _pluginRepository = pluginRepository;
            _accounts = accounts;
            _applicationSettings = applicationSettings;
            _columnsSettings = columnsSettings;

            _loader = createPluginLoader();

            Model = CompositionManager.Get<IMainViewModel>();

            Resources = createResourceViewModel();

            if (Model == null) return;
            Model.FilterGroups = new ObservableCollection<StreamModel>(_columnsSettings.Filters);

            if (Model.FilterGroups.Count != 0) return;
            var f = StreamModel.CreateDefault();
            Model.FilterGroups.Add(f);
        }

        public IMainViewModel Model { get; private set; }
        public ResourcesViewModel Resources { get; private set; }

        public void AddNewPlugin(string path)
        {
        }

        public void ShowSetupWindow()
        {
            Model.ShowSetup();
        }

        public void LoadMicroblogs()
        {
            foreach (var credential in _accounts.MicroblogCredentials)
            {
                //http://blogs.msdn.com/ericlippert/archive/2009/11/12/closing-over-the-loop-variable-considered-harmful.aspx
                IMicroblog foundMicroblog;
                var c = credential;

                if (!_loader.TryFind(c, out foundMicroblog)) continue;
                var newMicroblog = foundMicroblog;
                EventAggregator.GetEvent<MicroblogAdded>().Publish(newMicroblog);
            }
            Model.SelectDefaultMicroblogs();
        }

        public void LoadUrlshorteners()
        {
            foreach (var credential in _accounts.UrlShortenerCredentials)
            {
                IUrlShortener foundShortener;
                var c = credential;

                if (!_loader.TryFind(c, out foundShortener)) continue;
                var newShortener = foundShortener;
                EventAggregator.GetEvent<UrlShortenerAdded>().Publish(newShortener);
            }
        }

        public void LoadStatusHandlers()
        {
            foreach (var credential in _accounts.StatusHandlerCredentials)
            {
                IStatusHandler foundHandler;
                var c = credential;

                if (!_loader.TryFind(c, out foundHandler)) continue;
                var newHandler = foundHandler;
                EventAggregator.GetEvent<StatusHandlerAdded>().Publish(newHandler);
            }
        }

        public void LoadSavedSearches()
        {
            foreach (var search in _applicationSettings.SavedSearches)
            {
                IList<ISearchProvider> providers;
                _loader.TryFind(search.Providers, out providers);
                foreach (var p in providers)
                {
                    p.AddSearchTerm(search.SearchTerm);
                }

                Model.CreateNewSearch(search, providers);
            }
        }


        public void LoadDefaultStream()
        {
            Model.CreateStream(StreamModel.CreateDefault(), null);
        }

        public void LoadStreams()
        {
            foreach (var filterGroup in _columnsSettings.Filters)
            {
                foreach (var f in filterGroup.Filters.OfType<UpdateTypeFilter>().Select(filter => filter))
                    // rehydrate the source "update types" for this filtermodel
                {
                    f.SetMicroblog(_pluginRepository.GetMicroblogByAccount(f.MicroblogName, f.MicroblogAccountName));

                    // REQUIRE at least type name and namespace
                    var typeParameters = f.UpdateTypeName.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

                    if (typeParameters.Length < 2) continue;

                    var typeName = typeParameters[0].Trim();
                    var namespaceName = typeParameters[1].Trim();

                    // check for type and namespace against contents of MEF

                    var t = CompositionManager.ResolveType<UpdateType>(typeName, namespaceName, _accounts.GetType(),
                                                                        _applicationSettings.GetType());
                    if (t == null) continue;

                    if (string.IsNullOrEmpty(f.UpdateTypeParameter))
                    {
                        var type = Activator.CreateInstance(t) as UpdateType;
                        if (type == null) break;
                        f.SetUpdateType(type);
                    }
                    else
                    {
                        try
                        {
                            var type = Activator.CreateInstance(t, f.UpdateTypeParameter) as UpdateType;
                            if (type == null) break;
                            f.SetUpdateType(type);
                        }
                        catch (Exception ex)
                        {
                            CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                            try
                            {
                                var type = Activator.CreateInstance(t) as UpdateType;
                                if (type == null) break;
                                f.SetUpdateType(type);
                            }
                            catch (Exception ex2)
                            {
                                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex2);
                            }
                        }
                    }
                }
            }
            var columns = new ShowColumnsPayload(_columnsSettings.Columns.ToList());
            EventAggregator.GetEvent<ShowColumns>().Publish(columns);
        }

        public override void Cleanup()
        {
            _pluginRepository.Close();
        }
    }
}