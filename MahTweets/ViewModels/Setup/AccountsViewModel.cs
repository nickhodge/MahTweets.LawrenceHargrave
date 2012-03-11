using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MahTweets.Core.Commands;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Settings;
using MahTweets.Core.ViewModels;

namespace MahTweets.ViewModels.Setup
{
    public class AccountsViewModel : BaseViewModel
    {
        public AccountsViewModel(
            IPluginRepository pluginRepository,
            IEnumerable<IMicroblogSource> microblogs,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            PluginRepository = pluginRepository;
            NewMicroblogs = new ObservableCollection<IMicroblogSource>(microblogs.OrderBy(f => f.Name));

            AddCommand = new DelegateCommand<ICreator>(Add); //, a => CanAdd);
            RemoveCommand = new DelegateCommand<IPlugin>(Remove); //, a => CanRemove);
            //NewSelectedCommand = new DelegateCommand(NewSelected);
            //ExistingSelectedCommand = new DelegateCommand(ExistingSelected);
            //SaveSettingsCommand = new DelegateCommand(SaveSettings);
            ShowSettingsCommand = new DelegateCommand<IPlugin>(ShowSettings);
        }

        public IPluginRepository PluginRepository { get; private set; }
        public ObservableCollection<IMicroblogSource> NewMicroblogs { get; set; }

        public virtual ICommand ShowSettingsCommand { get; set; }

        public virtual ICreator NewPlugin { get; set; }

        public virtual DelegateCommand<ICreator> AddCommand { get; set; }

        public virtual ICommand SaveSettingsCommand { get; set; }

        public virtual DelegateCommand<IPlugin> RemoveCommand { get; set; }

        public virtual bool CanRemove { get; set; }

        public virtual bool CanAdd { get; set; }

        public virtual SettingsUserControl SettingsControl { get; set; }

        public void Add(ICreator newPlugin)
        {
            if (newPlugin == null) return;

            IPlugin found = newPlugin.Create();

            if (found != null)
            {
                found.Setup();
            }
        }

        public void Remove(IPlugin existingPlugin)
        {
            if (existingPlugin == null)
                return;

            if (existingPlugin is IMicroblog)
            {
                var existingMicroblog = existingPlugin as IMicroblog;
                if (!PluginRepository.Microblogs.Contains(existingMicroblog))
                    return;

                EventAggregator.GetEvent<MicroblogRemoved>().Publish(existingMicroblog);
            }
        }

        private void ShowSettings(IPlugin plugin)
        {
            if (plugin.HasSettings)
                plugin.ShowSettings();
        }

        //public void NewSelected()
        //{
        //    //CanAdd = (NewMicroblog != null);
        //    AddCommand.RaiseCanExecuteChanged();
        //}

        //public void ExistingSelected()
        //{
        //    // update Remove button logic
        //    /*CanRemove = (CurrentPlugin != null);
        //    RemoveCommand.RaiseCanExecuteChanged();

        //    if (CurrentPlugin == null)
        //        return;

        //    var helper = new SettingsHelper();
        //    var control = helper.GetSettingsControl(CurrentPlugin);
        //    if (control != null)
        //    {
        //        control.Load(CurrentPlugin);
        //        SettingsControl = control;
        //    }
        //    else
        //        SettingsControl = new NoSettingsDefined();*/
        //}

        //public void SaveSettings()
        //{
        //    /*if (SettingsControl == null) return;
        //    if (CurrentPlugin == null) return;

        //    SettingsControl.Save(CurrentPlugin);*/
        //}
    }
}