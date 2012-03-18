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
    public class HighlighterSettingsProvider : IHighlighterSettings, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CsvFileDescription _csvFileDescription;
        private readonly CsvContext _csvContext;
        private readonly string _filename;
        private ObservableCollection<HighlightWordsItem> _highlightwords;
        private readonly Storage _storage;
        private readonly Object _writependinglock = new object();
        private Boolean _writequeued;
        private readonly FileSystemWatcher _watcher;
        private const string SettingsFileName = "Highlight Words.mtdata";

        public HighlighterSettingsProvider()
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

        public ObservableCollection<HighlightWordsItem> HighlightWords
        { 
            get
            {
                return _highlightwords;
            }

            set
            {
                if (value == _highlightwords) return;
                _highlightwords = value; 
                OnPropertyChanged("HighlighWords");
            } 
        }

        private void SendUpdateEvent()
        {
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            eventAggregator.GetEvent<HighlighWordsChanged>().Publish(null);
        }

        public void Add(string newAddition)
        {
            HighlightWords.Add(new HighlightWordsItem(){Text = newAddition});
            SendUpdateEvent();
        }

        public void Remove(string deleteAddition)
        {
            // TODO deal with remove
            SendUpdateEvent();
        }

        public void Read()
        {
            try
            {
                lock (_writependinglock)
                {
                    var gei = _csvContext.Read<HighlightWordsItem>(_filename, _csvFileDescription);
                    HighlightWords = new ObservableCollection<HighlightWordsItem>(gei);
                }
            }
            catch (FileNotFoundException ioex)
            {
                var x = new List<HighlightWordsItem>();
                _csvContext.Write(x, _filename, _csvFileDescription);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            if (HighlightWords == null) // list didnt exist on disk
                HighlightWords = new ObservableCollection<HighlightWordsItem>();
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
                var gei = (from item in HighlightWords select (new HighlightWordsItem { Text = item.Text })).ToList();
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
