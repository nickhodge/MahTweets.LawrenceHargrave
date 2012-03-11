using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahTweets.Core;
using MahTweets.Core.Collections;
using MahTweets.Core.Commands;
using MahTweets.Core.Composition;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Factory;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Interfaces.ViewModels;
using MahTweets.Core.ViewModels;
using MahTweets.Views;

namespace MahTweets.ViewModels
{
    public class MainViewModel : ContainerViewModel, IMainViewModel
    {
        private readonly IAccountSettingsProvider _accountSettings;
        private readonly IApplicationSettingsProvider _applicationSettingsProvider;
        private readonly IColumnsSettingsProvider _columnsSettings;
        private readonly Func<SearchViewModel> _createSearchViewModel;
        private readonly Func<StreamViewModel> _createStreamViewModel;
        private readonly IPluginRepository _pluginRepository;
        private readonly SetupViewModel _setupViewModel;
        public int DefaultStreamWidth = 310;

        public MainViewModel(
            IPluginRepository pluginRepository,
            IAccountSettingsProvider accountSettings,
            IColumnsSettingsProvider columnsSettings,
            IApplicationSettingsProvider applicationSettingsProvider,
            Func<StreamViewModel> createStreamViewModel,
            Func<SearchViewModel> createSearchViewModel,
            SetupViewModel setupViewModel)
        {
            _pluginRepository = pluginRepository;
            _accountSettings = accountSettings;
            _columnsSettings = columnsSettings;
            _createStreamViewModel = createStreamViewModel;
            _createSearchViewModel = createSearchViewModel;
            _setupViewModel = setupViewModel;
            _applicationSettingsProvider = applicationSettingsProvider;

            EventAggregator.GetEvent<ShowLink>().Subscribe(HandleShowLink);
            EventAggregator.GetEvent<ShowProfile>().Subscribe(HandleShowProfile);
            EventAggregator.GetEvent<ShowConversation>().Subscribe(HandleShowConversation);
            EventAggregator.GetEvent<ShowSearch>().Subscribe(CreateNewSearch);
            EventAggregator.GetEvent<ShowContainer>().Subscribe(HandleShowContainer);
            EventAggregator.GetEvent<MicroblogAdded>().Subscribe(HandleNewMicroblog);
            EventAggregator.GetEvent<ShowColumns>().Subscribe(HandleNewColumns);

            ContactService = CompositionManager.Get<IContactService>();
            StatusManager = CompositionManager.Get<IStatusUpdateService>();

            SelectedMicroblogs = new ThreadSafeObservableCollection<IMicroblog>();

            CurrentSearches = new ObservableCollection<ISearchViewModel>();
            StreamContainers = new ObservableCollection<IContainerViewModel>();
            FilterGroups = new ObservableCollection<StreamModel>();

            HiddenPanels = new ObservableCollection<UIElement>();

            IsSearchVisible = false;
        }

        public IStatusUpdateService StatusManager { get; set; }

        public ObservableCollection<IMicroblog> CurrentMicroblogs
        {
            get { return _pluginRepository.Microblogs; }
        }

        public ObservableCollection<UIElement> HiddenPanels { get; set; }
        public bool IsSearchVisible { get; set; }

        public ICommand SetupCommand { get; set; }

        public ICommand AddColumnCommand { get; set; }

        public ICommand SearchCommand { get; set; }

        public ICommand RefreshCommand { get; set; }

        public ICommand ClearAllCommand { get; set; }

        public ICommand AboutCommand { get; set; }

        public ICommand NewUpdateCommand { get; set; }

        public ICommand ShowScriptConsoleCommand { get; set; }

        public SendMessageCommand SendMessageCommand { get; set; }

        #region IMainViewModel Members

        public IContactService ContactService { get; set; }

        public ThreadSafeObservableCollection<IMicroblog> SelectedMicroblogs { get; set; }

        public ObservableCollection<ISearchViewModel> CurrentSearches { get; set; }
        public ObservableCollection<IContainerViewModel> StreamContainers { get; set; }
        public ObservableCollection<StreamModel> FilterGroups { get; set; }

        public void SelectDefaultMicroblogs()
        {
            if (_applicationSettingsProvider.SelectedAccounts.Count > 0)
            {
                SelectedMicroblogs.Clear();

                foreach (var selected in _applicationSettingsProvider.SelectedAccounts)
                {
                    var selected1 = selected;
                    var selectedMicroblog = _pluginRepository.Microblogs.FirstOrDefault(m => m.Id == selected1);
                    if (selectedMicroblog != null)
                        SelectMicroblog(selectedMicroblog);
                }
            }
            else
            {
                var first = _pluginRepository.Microblogs.FirstOrDefault();
                if (first != null)
                    SelectMicroblog(first);
            }
            RaisePropertyChanged(() => SelectedMicroblogs);
        }

        public void ShowAbout()
        {
            var view = new AboutView();
            EventAggregator.GetEvent<ShowDialog>().Publish(view);
        }

        public void ShowSetup()
        {
            var view = new SetupView(_setupViewModel);
            EventAggregator.GetEvent<ShowDialog>().Publish(view);
        }

        public void CreateBlankStream()
        {
            var f = StreamModel.CreateDefault();
            CreateStream(f, null, DefaultStreamWidth);
        }

        public void CreateStream(StreamModel f)
        {
            CreateStream(f, null, DefaultStreamWidth);
        }

        public void CreateStream(StreamModel f, String title)
        {
            CreateStream(f, title, DefaultStreamWidth);
        }

        public void CreateStream(StreamModel f, String title, double columnWidth)
        {
            var model = _createStreamViewModel();
            model.Parent = this;

            if (f != null)
            {
                model.StreamConfiguration.Filters = f;
                if (string.IsNullOrEmpty(f.GroupName))
                {
                    model.Title = "New Column " + (StreamContainers.Count + 1);
                    model.StreamConfiguration.ShowConfiguration = true;
                }
                else
                {
                    model.Title = f.GroupName;
                }

                if (!FilterGroups.Contains(f))
                    FilterGroups.Add(f);
            }
            else
            {
                model.Title = title;
            }

            if (!StreamContainers.Contains(model))
                StreamContainers.Add(model);

            var container = new StreamView(model);

            model.View = container;
            model.Start();

            var showContainerPayload = new ShowContainerPayload(model);

            ShowContainer(showContainerPayload, null);

            container.Width = columnWidth;
            model.Width = columnWidth;
        }

        public void CreateNewSearch(SavedSearch savedSearch, IList<ISearchProvider> searchProviders)
        {
            var vm = _createSearchViewModel();
            foreach (var sp in searchProviders)
            {
                var current = sp;
                var found = vm.AvailableSearchProviders.FirstOrDefault(provider => provider.GetType() == current.GetType());
                if (found == null) continue;
                found.AddSearchTerm(savedSearch.SearchTerm);
                vm.SearchProviders.Add(found);
            }
            vm.Position = savedSearch.Position;

            AddSearch(vm);
        }

        public void SelectMicroblog(IMicroblog blog)
        {
            if (SelectedMicroblogs.Contains(blog)) return;
            if (blog.ReadOnly) return;
            SelectedMicroblogs.Add(blog);
        }

        public void UnselectMicroblog(IMicroblog blog)
        {
            if (!SelectedMicroblogs.Contains(blog)) return;
            SelectedMicroblogs.Remove(blog);
        }

        #endregion

        public void HandleNewMicroblog(IMicroblog payload)
        {
            if (SelectedMicroblogs.Count == 0)
                SelectMicroblog(payload);
        }

        public void NewUpdate()
        {
            ((MainWindow) View).txtUpdate.FocusOnText();
        }

        public void SendMessage(object obj)
        {
            var str = obj as string;
            foreach (var microblog in SelectedMicroblogs)
            {
                microblog.NewStatusUpdate(str);
            }
        }

        public void Refresh()
        {
            _pluginRepository.Refresh(true);
        }

        //small hack: sets the viewmodel.position = streamcontainer.index

        public void ShowContainer(ShowContainerPayload payload, DelegateCommand closeDelegate)
        {
            var viewModel = payload.ViewModel;
            var index = payload.ViewModel.Position;

            viewModel.CloseCommand = closeDelegate ?? new DelegateCommand(() => RemoveContainer(viewModel));

            if (!StreamContainers.Contains(viewModel))
            {
                if (index > 0 && index <= StreamContainers.Count)
                    StreamContainers.Insert(index, viewModel);
                else
                    StreamContainers.Add(viewModel);
            }
            viewModel.Start();
        }

        public void HandleShowContainer(ShowContainerPayload payload)
        {
            ShowContainer(payload, null);
        }

        public void AppendUpdateBox(string text)
        {
            ((MainWindow) View).txtUpdate.Text += text;
        }

        public void HandleNewColumns(ShowColumnsPayload s)
        {
            foreach (
                var c in
                    s.Columns.Where(c => c.ContainerType == "StreamView").Distinct(new ColumnConfigurationComparer()).
                        OrderBy(c => c.Position))
            {
                if (c.Mapping.Keys.Contains("Filter"))
                {
                    var f = c.Mapping["Filter"];
                    var g = FilterGroups.First(z => z.Uuid == f);
                    if (g != null)
                    {
                        CreateStream(g, c.Title, c.Width);
                        continue;
                    }
                }
                var h = FilterGroups.First(z => z.GroupName.ToLower() == c.Title.ToLower());
                    // fall through to mapping the name of the column to the name on the filter TODO remove later as this is transitionary
                {
                    CreateStream(h, c.Title, c.Width);
                }
            }
        }


        public void HandleShowConversation(ConversationViewModel conversationViewModel)
        {
            conversationViewModel.Parent = this;
            var showContainerPayload = new ShowContainerPayload(conversationViewModel);
            ShowContainer(showContainerPayload, null);
        }

        public void HandleShowProfile(ProfileViewModel profileViewModel)
        {
            profileViewModel.Parent = this;
            var showContainerPayload = new ShowContainerPayload(profileViewModel);
            ShowContainer(showContainerPayload, null);
        }


        public void CreateNewSearch()
        {
            var vm = _createSearchViewModel();
            vm.Parent = this;
            AddSearch(vm);
        }

        public void CreateNewSearch(ISearchProvider search)
        {
            var vm = _createSearchViewModel();
            vm.SearchProviders.Add(search);

            AddSearch(vm);
        }

        private void AddSearch(SearchViewModel vm)
        {
            vm.Parent = this;

            var showContainerPayload = new ShowContainerPayload(vm);
            ShowContainer(showContainerPayload, null);
            CurrentSearches.Add(vm);
        }

        public override void RemoveContainer(ContainerViewModel model)
        {
            StreamContainers.Remove(model);

            var streamViewModel = model as StreamViewModel;
            if (streamViewModel != null)
                FilterGroups.Remove((streamViewModel).StreamConfiguration.Filters);

            var searchViewModel = model as SearchViewModel;
            if (searchViewModel != null)
                CurrentSearches.Remove(searchViewModel);
        }

        public void ClosePlugins()
        {
            foreach (IMicroblog blog in SelectedMicroblogs)
                blog.Disconnect();
        }

        public void HandleShowLink(Link link)
        {
            try
            {
                Process.Start(link.Uri);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        public void ClearAll()
        {
            StatusManager.Reset();

            foreach (var container in StreamContainers)
            {
                var streamViewModel = container as StreamViewModel;
                if (streamViewModel != null)
                {
                    streamViewModel.ClearBeforeDate(DateTime.Now);
                }

                var searchViewModel = container as SearchViewModel;
                if (searchViewModel != null)
                {
                    searchViewModel.Clear();
                }
            }
        }
    }
}