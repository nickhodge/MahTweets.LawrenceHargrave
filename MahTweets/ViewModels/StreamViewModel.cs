using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MahTweets.Core;
using MahTweets.Core.Collections;
using MahTweets.Core.Commands;
using MahTweets.Core.Composition;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Factory;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Scripting;
using MahTweets.Core.ViewModels;

namespace MahTweets.ViewModels
{
    public class StreamViewModel : ContainerViewModel
    {
        private readonly IPluginRepository _pluginsRepository;
        private readonly IApplicationSettingsProvider _settings;
        private readonly IStatusUpdateService _statusUpdate;
        private string _columnStatus;
        private StreamConfigurationViewModel _streamConfigurationViewModel;

        public StreamViewModel()
        {
        }

        public StreamViewModel(
            IStatusUpdateService statusUpdateService,
            IPluginRepository pluginsRepository,
            StreamConfigurationViewModel streamConfigurationViewModel)
        {
            _pluginsRepository = pluginsRepository;
            _statusUpdate = statusUpdateService;

            StreamConfiguration = streamConfigurationViewModel;
            StreamConfiguration.Parent = this;

            Updates = new BindableList<IStatusUpdate>(_statusUpdate.OutgoingUpdates);
            _settings = CompositionManager.Get<IApplicationSettingsProvider>();
        }

        public BindableList<IStatusUpdate> Updates { get; set; }


        public new string Title
        {
            get { return StreamConfiguration.Filters.GroupName; }
            set
            {
                StreamConfiguration.Filters.GroupName = value;
                RaisePropertyChanged(() => StreamConfiguration.Filters.GroupName);
            }
        }

        // overrides the ContainerViewModel Title as this is mapped to the filter's name

        public IEnumerable<IStatusUpdate> LatestUpdates
        {
            get { return Updates.OrderByDescending(t => t.Time).Take(4); }
        }

        public Visibility ContactsVisibility { get; set; }

        public DelegateCommand<object> SearchCommand { get; set; }

        public DelegateCommand<object> ClearCommand { get; set; }

        public bool ShowSearch { get; set; }

        public ObservableCollection<IMicroblog> Accounts
        {
            get { return _pluginsRepository.Microblogs; }
        }

        public string ColumnStatus
        {
            get { return _columnStatus; }
            set
            {
                _columnStatus = value;
                RaisePropertyChanged(() => ColumnStatus);
            }
        }

        public StreamConfigurationViewModel StreamConfiguration
        {
            get { return _streamConfigurationViewModel; }
            set
            {
                _streamConfigurationViewModel = value;
                RaisePropertyChanged(() => StreamConfiguration);
            }
        }

        public bool CanAcceptChildren { get; set; }

        public ColumnConfiguration GetColumnConfiguration()
        {
            var cc = new ColumnConfiguration
                         {
                             ContainerType = "StreamView",
                             Title = Title,
                             Uuid = Uuid,
                             Width = Width,
                             Position = Position
                         };
            cc.Mapping.Add("Filter", StreamConfiguration.Filters.Uuid);
            return cc;
        }

        public bool Filter(IStatusUpdate update)
        {
            return StreamConfiguration.Filter(update);
        }

        public override void Start()
        {
            if (!StreamConfiguration.Filters.IsDefined)
                StreamConfiguration.SetEdit();

            EventAggregator.GetEvent<ReloadFilter>().Subscribe(TriggerUpdate);
        }

        public override void Close()
        {
            var model = (Parent as MainViewModel);
            if (model == null) return;
            model.RemoveContainer(this);
        }

        public void SwitchSearchMode()
        {
            ShowSearch = !ShowSearch;
        }

        public void Clear()
        {
            Updates.Clear();
        }

        public void ClearBeforeDate(DateTime dateTime)
        {
            StreamConfiguration.FilterBefore = dateTime;
        }

        public void ScriptFilterChanged(ScriptFilter sf)
        {
            if (StreamConfiguration.Filters.InScriptFilterActivated(sf) && !StreamConfiguration.ShowConfiguration)
                StreamConfiguration.SaveChanges();

            //RaisePropertyChanged(() => Filters);
            //RaisePropertyChanged(() => ScriptFilters);
        }

        public void UpdateMicroblogColor(IMicroblog imicroblog, UpdateType updatetype, Color color)
        {
            StreamConfiguration.UpdateMicroblogColor(imicroblog, updatetype, color);
        }

        public void UpdateContactColor(Contact contact, Color color)
        {
            StreamConfiguration.UpdateContactColor(contact, color);
        }

        public void Pause()
        {
            if (!_settings.PauseStreams) return;
            Updates.Pause();
            ColumnStatus = "Column Updates Paused";
        }

        public void Resume()
        {
            Updates.Resume();
            ColumnStatus = "";
        }

        public void ResetUpdates()
        {
            //This operation cannot be simply threaded, the results have been logged in MT-141
            Updates = new BindableList<IStatusUpdate>(_statusUpdate.OutgoingUpdates);
        }

        private void TriggerUpdate(NullEvent obj)
        {
            ResetUpdates();
        }

        #region MahTweets Scripting

        public void AddScriptFilter(ScriptFilter sf)
        {
            StreamConfiguration.Filters.AddActiveScriptFilter(sf);
            // RaisePropertyChanged(() => Filters);
        }

        public void RemoveScriptFilter(ScriptFilter sf)
        {
            StreamConfiguration.Filters.RemoveActiveScriptFilter(sf);
            //RaisePropertyChanged(() => Filters);
        }

        public void UpdateScriptFilterColor(ScriptFilter sf, Color colour)
        {
            StreamConfiguration.Filters.UpdateScriptFilterColor(sf, colour);
            //RaisePropertyChanged(() => Filters);
        }

        #endregion
    }
}