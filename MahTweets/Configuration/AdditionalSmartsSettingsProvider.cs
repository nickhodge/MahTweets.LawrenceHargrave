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
    public class AdditonalSmartsSettingsProvider : IAdditonalSmartsSettingsProvider, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CsvFileDescription _csvFileDescription;
        private readonly CsvContext _csvContext;
        private readonly string _filename;
        private ObservableCollection<AdditonalSmartsMappingItem> _additionalsmartsmapping;
        private readonly Storage _storage;
        private readonly Object _writependinglock = new object();
        private Boolean _writequeued;
        private readonly FileSystemWatcher _watcher;
        private const string SettingsFileName = "Additional Smarts Mappings.mtdata";

        public AdditonalSmartsSettingsProvider()
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

        public ObservableCollection<AdditonalSmartsMappingItem> AdditionalSmartsMapping
        { 
            get
            {
                return _additionalsmartsmapping;
            }

            set
            {
                if (value == _additionalsmartsmapping) return;
                _additionalsmartsmapping = value; 
                OnPropertyChanged("AdditionalSmarts");
            } 
        }

        private void SendUpdateEvent()
        {
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<AdditionalSmartsChanged>().Publish(null);
        }

        public void Add(string newUrl, string newProcess)
        {
            AdditionalSmartsMapping.Add(new AdditonalSmartsMappingItem() { Url = newUrl, ProcessType = newProcess});
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
                    var gei = _csvContext.Read<AdditonalSmartsMappingItem>(_filename, _csvFileDescription);
                    AdditionalSmartsMapping = new ObservableCollection<AdditonalSmartsMappingItem>(gei.ToList());
                }
            }
            catch (FileNotFoundException ioex)
            {
                var x = new List<AdditonalSmartsMappingItem>();
                _csvContext.Write(x, _filename, _csvFileDescription);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            if (AdditionalSmartsMapping == null) // list didnt exist on disk
            {
                AdditionalSmartsMapping = new ObservableCollection<AdditonalSmartsMappingItem>();
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
                var gei = (from item in AdditionalSmartsMapping select (new AdditonalSmartsMappingItem { Url = item.Url, ProcessType = item.ProcessType })).ToList();
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
