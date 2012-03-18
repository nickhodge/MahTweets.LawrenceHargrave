using System;
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
    public class AcronymSettingsProvider : IAcronymSettingsProvider, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CsvFileDescription _csvFileDescription;
        private readonly CsvContext _csvContext;
        private readonly string _filename;
        private ObservableCollection<AcronymMappingItem> _acronymmapping;
        private readonly Storage _storage;
        private readonly Object _writependinglock = new object();
        private Boolean _writequeued;
        private readonly FileSystemWatcher _watcher;
        private const string SettingsFileName = "Acronym Mappings.mtdata";

        public AcronymSettingsProvider()
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

        public ObservableCollection<AcronymMappingItem> AcronymMapping
        { 
            get
            {
                return _acronymmapping;
            }

            set
            {
                if (value == _acronymmapping) return;
                _acronymmapping = value; 
                OnPropertyChanged("Acronyms");
            } 
        }

        private void SendUpdateEvent()
        {
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<AcronymsChanged>().Publish(null);
        }

        public void Add(string newAcronym, string newMeaning)
        {
            AcronymMapping.Add(new AcronymMappingItem(){Acronym = newAcronym, Meaning = newMeaning});
            SendUpdateEvent();
        }

        public void Remove(string deleteAcronym)
        {
            // TODO Delete element from list
            // SendUpdateEvent();
        }

        public void Read()
        {
            try
            {
                lock (_writependinglock)
                {
                    var gei = _csvContext.Read<AcronymMappingItem>(_filename, _csvFileDescription);
                    AcronymMapping = new ObservableCollection<AcronymMappingItem>(gei.ToList());
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            if (AcronymMapping == null) // list didnt exist on disk
            {
                AcronymMapping = new ObservableCollection<AcronymMappingItem>();
            }
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
                var gei = (from item in AcronymMapping select (new AcronymMappingItem { Acronym = item.Acronym, Meaning = item.Meaning})).ToList();
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
