using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MahTweets.Core.Commands;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.ViewModels;

namespace MahTweets.ViewModels.Setup
{
    public class GlobalIgnoresViewModel : BaseViewModel
    {
        private readonly IGlobalExcludeSettings _globalExcludeSettingsProvider;
        private string _currentText;
        private bool _isModified;

        public GlobalIgnoresViewModel(
            IGlobalExcludeSettings globalExcludeSettingsProvider)
        {
            _globalExcludeSettingsProvider = globalExcludeSettingsProvider;

            ClearCommand = new DelegateCommand(Clear, () => CanClear);
            AddCommand = new DelegateCommand(Add, () => CanAdd);
            RemoveCommand = new DelegateCommand<string>(Remove);
        }

        public ObservableCollection<GlobalExcludeItem> GlobalExcludes
        {
            get { return _globalExcludeSettingsProvider.GlobalExcludeItems; }
        }

        public string CurrentText
        {
            get { return _currentText; }
            set
            {
                _currentText = value;
                RaisePropertyChanged(() => CurrentText);
                AddCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddCommand { get; set; }
        public DelegateCommand<String> RemoveCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public bool CanClear
        {
            get { return _globalExcludeSettingsProvider.GlobalExcludeItems.Any(); }
        }

        public bool CanAdd
        {
            get { return !string.IsNullOrWhiteSpace(CurrentText); }
        }

        public void Clear()
        {
            _globalExcludeSettingsProvider.GlobalExcludeItems.Clear();

            _isModified = true;

            RaisePropertyChanged(() => GlobalExcludes);
            ClearCommand.RaiseCanExecuteChanged();
        }

        public void Remove(String Text)
        {
            _globalExcludeSettingsProvider.Remove(Text);
            RaisePropertyChanged(() => GlobalExcludes);
        }

        public void Add()
        {
            _globalExcludeSettingsProvider.Add(CurrentText);
            RaisePropertyChanged(() => GlobalExcludes);
            CurrentText = string.Empty;

            _isModified = true;

            ClearCommand.RaiseCanExecuteChanged();
        }

        public void CheckForNewUpdates()
        {
            if (_isModified)
            {
                var reloadingMessage = new ShowNotificationPayload("Reloading Streams", TimeSpan.FromSeconds(10),
                                                                   NotificactionLevel.Information);
                EventAggregator.GetEvent<ShowNotification>().Publish(reloadingMessage);

                Task.Run(() => EventAggregator.GetEvent<ReloadFilter>().Publish(new NullEvent()));

                _isModified = false;
            }
        }
    }
}