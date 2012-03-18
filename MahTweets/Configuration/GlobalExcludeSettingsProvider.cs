using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LINQtoCSV;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;

namespace MahTweets.Configuration
{
    public class GlobalExcludeSettingProvider : IGlobalExcludeSettings, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CsvFileDescription _csvFileDescription;
        private readonly CsvContext _csvContext;
        private readonly string _filename;
        private ObservableCollection<GlobalExcludeItem> _globalitems;
        private readonly Storage _storage;
        private readonly Object _writependinglock = new object();
        private Boolean _writequeued;
        private readonly FileSystemWatcher _watcher;
        private const string SettingsFileName = "Global Exclusion Strings.mtdata";

        public GlobalExcludeSettingProvider()
        {
            _csvFileDescription = new CsvFileDescription
                                      {
                                          SeparatorChar = ',',
                                          FirstLineHasColumnNames = true,
                                          EnforceCsvColumnAttribute = true
                                      };
            _csvContext = new CsvContext();
            _writequeued = false;
            _storage = new Storage();
            _filename = _storage.CombineDocumentsFullPath(SettingsFileName);
            Read(); // do initial read
            _watcher = new FileSystemWatcher {Path=Path.GetDirectoryName(_filename), Filter  = Path.GetFileName(_filename), NotifyFilter = NotifyFilters.LastWrite};
            _watcher.Changed += OnChanged;
            _watcher.EnableRaisingEvents = true;
        }

        public ObservableCollection<GlobalExcludeItem> GlobalExcludeItems
        { 
            get
            {
                return _globalitems;
            }

            set
            {
                if (value == _globalitems) return;
                _globalitems = value; 
                OnPropertyChanged("GlobalExcludeItems");
            } 
        }

        private void SendUpdateEvent()
        {
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<GlobalExclusionChanged>().Publish(null);
        }

        public void Add(string newAddition)
        {
            GlobalExcludeItems.Add(new GlobalExcludeItem() {Text = newAddition});
            SendUpdateEvent();
        }

        public void Remove(string deleteAddition)
        {
            //TODO do the removal thing
            SendUpdateEvent();
        }

        public void Read()
        {
            try
            {
                lock (_writependinglock)
                {
                    var gei = _csvContext.Read<GlobalExcludeItem>(_filename, _csvFileDescription);
                    GlobalExcludeItems = new ObservableCollection<GlobalExcludeItem>(gei);
                }
            }
            catch (FileNotFoundException ioex)
            {
                var x = new List<GlobalExcludeItem>();
                _csvContext.Write(x,_filename,_csvFileDescription);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            if (GlobalExcludeItems == null) // list didnt exist on disk
                GlobalExcludeItems = new ObservableCollection<GlobalExcludeItem>();
            SendUpdateEvent();
        }

        public void Write()
        {
           if (_writequeued) return;
            _writequeued = true;
            Task.Run(() => QueuedWrite());
            _writequeued = false;
        }

        private void QueuedWrite()
        {
            try
            {
                var gei = (from item in GlobalExcludeItems select (new GlobalExcludeItem { Text = item.Text })).ToList(); // take a copy
                lock (_writependinglock)
                {
                    _csvContext.Write(gei, _filename, _csvFileDescription);
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }           
        }
        
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Read(); // file has changed on disk, time to update the list
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged; if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        } 
    }
}
