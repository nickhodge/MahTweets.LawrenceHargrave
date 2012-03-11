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
using MahTweets.Helpers;
using MahTweets.Views.Controls;

namespace MahTweets.ViewModels.Setup
{
    public class UploadersViewModel : BaseViewModel
    {
        public IPluginRepository PluginRepository { get; private set; }
        public ObservableCollection<IUploaderCreator> NewUploaders { get; set; }

        public UploadersViewModel(
            IPluginRepository pluginRepository,
            IEnumerable<IUploaderCreator> uploaders,
            IEventAggregator eventAggregator): base(eventAggregator)
        {
            PluginRepository = pluginRepository;
            NewUploaders = new ObservableCollection<IUploaderCreator>(uploaders.OrderBy(m => m.Name));

            AddCommand = new DelegateCommand<IUploaderCreator>(Add, a => CanAdd);
            RemoveCommand = new DelegateCommand<IUploader>(Remove, a => CanRemove);
            NewSelectedCommand = new DelegateCommand(NewSelected);
            ExistingSelectedCommand = new DelegateCommand(ExistingSelected);
            SaveSettingsCommand = new DelegateCommand(SaveSettings);
        }

        public virtual ICommand NewSelectedCommand { get; set; }

        public virtual DelegateCommand<IUploaderCreator> AddCommand { get; set; }

        public virtual ICommand SaveSettingsCommand { get; set; }

        public virtual ICommand ExistingSelectedCommand { get; set; }

        public virtual DelegateCommand<IUploader> RemoveCommand { get; set; }

        public virtual IUploaderCreator NewUploader { get; set; }

        public virtual IUploader CurrentUploader { get; set; }

        public virtual bool CanRemove { get; set; }

        public virtual bool CanAdd { get; set; }

        public virtual SettingsUserControl SettingsControl { get; set; }

        public void Add(IUploaderCreator newUploader)
        {
            if (newUploader == null) return;

            var found = newUploader.Create();

            if (found != null)
                found.Setup();
        }

        public void Remove(IUploader existingUploader)
        {
            if (existingUploader == null)
                return;

            EventAggregator.GetEvent<UploaderRemoved>().Publish(existingUploader);
        }

        public void NewSelected()
        {
            CanAdd = (NewUploader != null);
            AddCommand.RaiseCanExecuteChanged();
        }

        public void ExistingSelected()
        {
            // update Remove button logic
            CanRemove = (CurrentUploader != null);
            RemoveCommand.RaiseCanExecuteChanged();

            if (CurrentUploader == null)
                return;

            var helper = new SettingsHelper();
            var control = helper.GetSettingsControl(CurrentUploader);
            if (control != null)
            {
                control.Load(CurrentUploader);
                SettingsControl = control;
            }
            else
                SettingsControl = new NoSettingsDefined();
        }

        public void SaveSettings()
        {
            if (SettingsControl == null) return;
            if (CurrentUploader == null) return;

            SettingsControl.Save(CurrentUploader);
        }
    }
}
