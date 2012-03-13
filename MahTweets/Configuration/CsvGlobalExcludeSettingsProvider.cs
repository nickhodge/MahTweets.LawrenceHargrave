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

namespace MahTweets.Configuration
{
    public class GlobalExclude : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CsvFileDescription _csvFileDescription;
        private readonly CsvContext _csvContext;
        private readonly string _filename;
        private ObservableCollection<string> _globalitems;
        private readonly Storage _storage;
        private readonly Object _writependinglock = new object();
        private Boolean _writequeued;
        private readonly FileSystemWatcher _watcher;

        public GlobalExclude(string fileName)
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
            _filename = _storage.CombineDocumentsFullPath(fileName);
            Read(); // do initial read
            _watcher = new FileSystemWatcher {Path=Path.GetDirectoryName(_filename), Filter  = Path.GetFileName(_filename), NotifyFilter = NotifyFilters.LastWrite};
            _watcher.Changed += OnChanged;
            _watcher.EnableRaisingEvents = true;
        }

        public ObservableCollection<string> GlobalExcludeItems
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
            GlobalExcludeItems.Add(newAddition);
            SendUpdateEvent();
        }

        public void Remove(string deleteAddition)
        {
            if (!GlobalExcludeItems.Contains(deleteAddition)) return;
            GlobalExcludeItems.Remove(deleteAddition);
            SendUpdateEvent();
        }

        public void Read()
        {
            try
            {
                lock (_writependinglock)
                {
                    var gei = _csvContext.Read<GlobalExcludeItem>(_filename, _csvFileDescription);
                    GlobalExcludeItems = new ObservableCollection<string>(gei.Select(i => i.Text.ToLower()).ToList());
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            if (GlobalExcludeItems == null) // list didnt exist on disk
                GlobalExcludeItems = new ObservableCollection<string>();
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
                var gei = (from item in GlobalExcludeItems select (new GlobalExcludeItem { Text = item })).ToList();
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


    public class GlobalExcludeItem
    {
        [CsvColumn(Name = "TextStringToExclude", FieldIndex = 1)]
        public string Text { get; set; }
    }

}
