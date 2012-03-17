using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using MahTweets.Core.Commands;
using MahTweets.Core.Composition;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Scripting;

namespace MahTweets.Core.ViewModels
{
    public class StreamConfigurationViewModel : BaseViewModel
    {
        private readonly IAccountSettingsProvider _accountSettingsProvider;
        private readonly IApplicationSettingsProvider _applicationSettingsProvider;
        private readonly IGlobalExcludeSettings _globalExcludeSettings;
        private readonly IColumnsSettingsProvider _columnsSettingsProvider;
        private readonly IContactService _contactService;
        private readonly IScriptingLibrarian _scriptLibrary;
        private ObservableCollection<IContact> _contacts;
        private DateTime _filterbefore;
        private bool _showconfiguration;

        public StreamConfigurationViewModel(
            IApplicationSettingsProvider applicationSettingsProvider,
            IGlobalExcludeSettings globalExcludeSettings,
            IAccountSettingsProvider accountSettingsProvider,
            IColumnsSettingsProvider columnsSettingsProvider,
            IContactService contactService)
        {
            _applicationSettingsProvider = applicationSettingsProvider;
            _accountSettingsProvider = accountSettingsProvider;
            _globalExcludeSettings = globalExcludeSettings;
            _columnsSettingsProvider = columnsSettingsProvider;
            _contactService = contactService;
            _scriptLibrary = CompositionManager.Get<IScriptingLibrarian>();

            ShowConfiguration = false;

            Filters = new StreamModel {Direction = ListSortDirection.Descending};
            FilterBefore = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0, 0));

            ScriptFilters = _scriptLibrary.ScriptFilters;

            EditCommand = new DelegateCommand(SetEdit);
            CancelCommand = new DelegateCommand(CancelChanges);
            SaveCommand = new DelegateCommand(SaveChanges);
        }

        public ICommand SaveCommand { get; set; }

        public ICommand EditCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public StreamModel Filters { get; set; }

        public DateTime FilterBefore
        {
            get { return _filterbefore; }
            set
            {
                _filterbefore = value;
                RaisePropertyChanged(() => FilterBefore);
            }
        }

        public string SearchText { get; set; }

        public ObservableCollection<IContact> Contacts
        {
            get { return _contacts; }
            set
            {
                _contacts = value;
                RaisePropertyChanged(() => Contacts);
            }
        }

        public IEnumerable<ScriptFilter> ScriptFilters { get; set; }

        public StreamModel OldFilters { get; set; }

        public bool ShowConfiguration
        {
            get { return _showconfiguration; }
            set
            {
                _showconfiguration = value;
                RaisePropertyChanged(() => ShowConfiguration);
            }
        }

        public bool Filter(IStatusUpdate update)
        {
            if (update == null) return false;

            if (_globalExcludeSettings.GlobalExcludeItems.Any(ignore => !update.Filter(ignore)))
            {
                return false;
            }


            //master script defined filters
            if (_scriptLibrary.IsScriptEngineActive)
            {
                var _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

                if (_scriptLibrary.CountScriptEntryPoints(_scriptingConfiguration.GlobalScriptFilterEntryPoint) > 0 &&
                    _scriptLibrary.ScriptEntryPoints(_scriptingConfiguration.GlobalScriptFilterEntryPoint).Any(
                        sf => !Filters.ExecuteGlobalFilterScript(sf.Key, update)))
                {
                    return false; // if returning false, that means don't include, please
                }
            }

            bool isAccepted = false;

            //For ignoring specific text
            if (Filters.IsIgnored(update.Text) == FilterBehaviour.Exclude)
            {
                return false;
            }

            //TODO "Everything" column relies on the specific checkbox, not nothing being set!
            //If it is unfiltered (ie, an 'everything' column)
            if ((!Filters.IsDefined || Filters.SelectAll) && update.Time > FilterBefore)
                isAccepted = true;

            //make sure if there is a date filter (created when a column clear is performed)
            if (update.Time > FilterBefore)
            {
                FilterBehaviour state = Filters.IsIgnored(update);

                if (state == FilterBehaviour.Exclude)
                    isAccepted = false;
                else if (state == FilterBehaviour.Include)
                    isAccepted = true;
            }

            //inline search
            if (!string.IsNullOrEmpty(SearchText))
            {
                if (!update.Text.ToLower().Contains(SearchText.ToLower()))
                    isAccepted = false;
            }

            return isAccepted;
        }

        public void SaveChanges()
        {
            _accountSettingsProvider.Save();
            _columnsSettingsProvider.Save();
            ShowConfiguration = false;
            EventAggregator.GetEvent<ReloadFilter>().Publish(new NullEvent());

            Task.Run(() => { Contacts = null; });
        }

        public void CancelChanges()
        {
            if (OldFilters != null)
                Filters = OldFilters;

            ShowConfiguration = false;

            Task.Run(() => { Contacts = null; });
        }

        public void SetEdit()
        {
            if (OldFilters != null)
                OldFilters = Filters;

            ShowConfiguration = true;

            ScriptFilters = _scriptLibrary.ScriptFilters;
            Task.Run(() => { Contacts = _contactService.ContactsList; });
        }

        public void UpdateMicroblogColor(IMicroblog imicroblog, UpdateType updatetype, Color color)
        {
            Filters.Include(imicroblog, updatetype, color);
            RaisePropertyChanged(() => Filters);
        }

        public void UpdateContactColor(Contact contact, Color color)
        {
            Filters.Color(contact, color);
            RaisePropertyChanged(() => Filters);
        }

        public void AddStream(IMicroblog blog, UpdateType updateType)
        {
            Filters.Include(blog, updateType);
            RaisePropertyChanged(() => Filters);
        }

        public void RemoveStream(IMicroblog blog, UpdateType updateType)
        {
            Filters.RemoveExclude(blog, updateType);
            RaisePropertyChanged(() => Filters);
        }

        public void IgnoreStream(IMicroblog blog, UpdateType updateType)
        {
            Filters.Exclude(blog, updateType);
            RaisePropertyChanged(() => Filters);
        }

        public void AddContactFilter(IContact contact, Color color)
        {
            Filters.Color(contact, color);
            RaisePropertyChanged(() => Filters);
        }

        public void AddContactExcludeFilter(IContact contact)
        {
            Filters.Exclude(contact);
            RaisePropertyChanged(() => Filters);
        }

        public void RemoveContactFilter(IContact contact)
        {
            Filters.RemoveExclude(contact);
            RaisePropertyChanged(() => Filters);
        }
    }
}