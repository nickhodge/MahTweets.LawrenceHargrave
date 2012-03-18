using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Media;

namespace MahTweets.Configuration
{
    public class ApplicationSettingsProvider : IApplicationSettingsProvider
    {
        private const string KeyUseLocation = "UseLocation";
        private const string KeyLatitude = "Latitude";
        private const string KeyLongitude = "Longitude";
        private const string KeyDisableProfileColumns = "DisableProfileColumns";
        private const string KeyMapEngine = "MapEngine";
        private const string KeyIgnoreAutoUnreads = "IgnoreAutoUnreads";
        private const string KeyAutoExpandUrls = "AutoExpandUrls";
        private const string KeyScriptingEnabled = "ScriptingEnabled";
        private const string KeyRichAnimations = "RichAnimations";
        private const string KeyIsCPURendering = "IsCPURendering";
        private const string KeyMediaHandling = "MediaHandling";
        private const string KeyGlobalIgnore = "GlobalIgnore";
        private const string KeySavedSearches = "SavedSearches";
        private const string KeyDisableMediaProviders = "DisableMediaProviders";
        private const string KeyPauseStreams = "PauseStreams";
        private const string KeyInstallationId = "InstallationId";
        private const string KeySelectedAccounts = "SelectedAccount";
        private const string KeyAutoShorten = "KeyAutoShorten";
        private const string KeyDefaultShortener = "KeyDefaultShortener";
        public const string KeyStyleFontSizeKey = "StyleFontSize";


        private readonly string _docPath;
        private readonly IStorage _storage;
        private IUrlShortener _defaultShortener;
        private XDocument _doc;
        private IList<IMediaProvider> _mediaProviders;
        private ObservableCollection<SavedSearch> _savedSearches;
        private ObservableCollection<string> _selectedAccounts;

        [ImportingConstructor]
        public ApplicationSettingsProvider()
        {
            _storage = CompositionManager.Get<IStorage>();
            _docPath = _storage.CombineFullPath("Application Settings.xml");

            try
            {
                _doc = !File.Exists(_docPath) ? CreateDefaultXmlFile() : XDocument.Load(_docPath);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        public ApplicationSettingsProvider(string docPath)
        {
            _docPath = docPath;
        }

        public bool IgnoreReadUnread
        {
            get { return _doc.GetValueBool(KeyIgnoreAutoUnreads); }
            set { _doc.SetValue(KeyIgnoreAutoUnreads, value); }
        }

        public bool RichAnimations
        {
            get { return _doc.GetValueBool(KeyRichAnimations); }
            set { _doc.SetValue(KeyRichAnimations, value); }
        }

        #region IApplicationSettingsProvider Members

        public bool UseLocation
        {
            get { return _doc.GetValueBool(KeyUseLocation); }
            set { _doc.SetValue(KeyUseLocation, value); }
        }

        public double Latitude
        {
            get { return _doc.GetValueDouble(KeyLatitude); }
            set { _doc.SetValue(KeyLatitude, value); }
        }

        public double Longitude
        {
            get { return _doc.GetValueDouble(KeyLongitude); }
            set { _doc.SetValue(KeyLongitude, value); }
        }

        public bool DisableProfileColumns
        {
            get { return _doc.GetValueBool(KeyDisableProfileColumns); }
            set { _doc.SetValue(KeyDisableProfileColumns, value); }
        }

        public int MapEngine
        {
            get { return _doc.GetValueInt(KeyMapEngine); }
            set { _doc.SetValue(KeyMapEngine, value); }
        }

        public bool AutoExpandUrls
        {
            get { return _doc.GetValueBool(KeyAutoExpandUrls); }
            set { _doc.SetValue(KeyAutoExpandUrls, value); }
        }

        public double WindowWidth
        {
            get { return _doc.GetValueDouble("WindowWidth"); }
            set { _doc.SetValue("WindowWidth", value); }
        }

        public double WindowXPos
        {
            get { return _doc.GetValueDouble("WindowXPos"); }
            set { _doc.SetValue("WindowXPos", value); }
        }

        public double WindowHeight
        {
            get { return _doc.GetValueDouble("WindowHeight"); }
            set { _doc.SetValue("WindowHeight", value); }
        }

        public double WindowYPos
        {
            get { return _doc.GetValueDouble("WindowYPos"); }
            set { _doc.SetValue("WindowYPos", value); }
        }

        public bool IsCPURendering
        {
            get { return _doc.GetValueBool(KeyIsCPURendering); }
            set { _doc.SetValue(KeyIsCPURendering, value); }
        }

        public IList<IMediaProvider> MediaProviders
        {
            get
            {
                if (_mediaProviders == null)
                    _mediaProviders = new List<IMediaProvider>();

                return _mediaProviders;
            }
        }

        public ObservableCollection<SavedSearch> SavedSearches
        {
            get
            {
                if (_savedSearches != null)
                    return _savedSearches;

                _savedSearches = new ObservableCollection<SavedSearch>();
                XElement node = _doc.GetElement(KeySavedSearches);

                if (node == null)
                {
                    var xE = new XElement(KeySavedSearches);
                    XElement xElement = _doc.Element("Settings");
                    if (xElement != null) xElement.Add(xE);
                }
                else
                {
                    foreach (XElement descendant in node.Descendants("Search"))
                    {
                        XElement search = descendant;
                        if (search != null)
                        {
                            var savedSearch = new SavedSearch();

                            XElement searchTerm = search.Descendants("Term").OfType<XElement>().FirstOrDefault();
                            if (searchTerm != null) savedSearch.SearchTerm = searchTerm.Value.UnescapeXml();

                            // bugfix - handling the invalid state of the settings
                            if (string.IsNullOrWhiteSpace(savedSearch.SearchTerm))
                                continue;

                            XElement searchPosition = search.Descendants("Position").OfType<XElement>().FirstOrDefault();
                            int defaultPosition = 0;
                            if (searchPosition != null)
                            {
                                int.TryParse(searchPosition.Value.UnescapeXml(), out defaultPosition);
                            }
                            savedSearch.Position = defaultPosition;

                            List<string> l =
                                search.Descendants("Provider").OfType<XElement>().Select(child => child.Value).ToList();

                            savedSearch.Providers = l;
                            _savedSearches.Add(savedSearch);
                        }
                    }
                }

                return _savedSearches;
            }
        }

        public MediaHandling MediaHandling
        {
            get { return (MediaHandling) _doc.GetValueInt(KeyMediaHandling); }
            set { _doc.SetValue(KeyMediaHandling, (int) value); }
        }

        public IUrlShortener DefaultShortener
        {
            get
            {
                string name = _doc.GetValue(KeyDefaultShortener);
                _defaultShortener =
                    CompositionManager.GetAll<IUrlShortener>().FirstOrDefault(
                        u => String.Compare(u.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
                return _defaultShortener;
            }
            private set
            {
                if (value != null)
                    _doc.SetValue(KeyDefaultShortener, value.Name);
            }
        }

        public void SetDefaultShortener(IUrlShortener shortener)
        {
            DefaultShortener = shortener;
        }


        public ObservableCollection<string> SelectedAccounts
        {
            get
            {
                if (_selectedAccounts != null)
                    return _selectedAccounts;

                _selectedAccounts = new ObservableCollection<string>();
                XElement node = _doc.GetElement(KeySelectedAccounts);

                if (node == null)
                {
                    var xE = new XElement(KeySelectedAccounts);
                    XElement xElement = _doc.Element("Settings");
                    if (xElement != null) xElement.Add(xE);
                }
                else
                {
                    foreach (XElement child in node.DescendantNodes().OfType<XElement>())
                    {
                        _selectedAccounts.Add(child.Value);
                    }
                }

                return _selectedAccounts;
            }
        }

        public void Save()
        {
            SaveSearches();
            SaveSelectedAccounts();
            _doc.Save(_docPath);
        }

        public void Reset()
        {
            File.Delete(_docPath);
            _doc = CreateDefaultXmlFile();
        }

        public bool AutoUrlShorten
        {
            get { return _doc.GetValueBool(KeyAutoShorten); }
            set { _doc.SetValue(KeyAutoShorten, value); }
        }

        public bool DisableMediaProviders
        {
            get { return _doc.GetValueBool(KeyDisableMediaProviders); }
            set { _doc.SetValue(KeyDisableMediaProviders, value); }
        }

        public bool PauseStreams
        {
            get { return _doc.GetValueBool(KeyPauseStreams); }
            set { _doc.SetValue(KeyPauseStreams, value); }
        }

        public double StyleFontSize
        {
            get { return _doc.GetValueDouble(KeyStyleFontSizeKey); }
            set { _doc.SetValue(KeyStyleFontSizeKey, value); }
        }

        public string InstallationId
        {
            get
            {
                string installationId = _doc.GetValue(KeyInstallationId);
                if (String.IsNullOrEmpty(installationId))
                {
                    installationId = Guid.NewGuid().ToString();
                    _doc.SetValue(KeyInstallationId, installationId);
                }

                return installationId;
            }
        }

        #endregion

        #region MahTweets Scripting

        public bool ScriptingEnabled
        {
            get { return _doc.GetValueBool(KeyScriptingEnabled); }
            set { _doc.SetValue(KeyScriptingEnabled, value); }
        }

        #endregion

        private static XDocument CreateDefaultXmlFile()
        {
            // root elements - main settings
            return new XDocument(new XElement("Settings",
                                              new XElement("Location",
                                                           new XElement(KeyUseLocation, false),
                                                           new XElement(KeyLatitude, 0),
                                                           new XElement(KeyLongitude, 0)),
                                              new XElement(KeyDisableProfileColumns, false),
                                              new XElement(KeyIgnoreAutoUnreads, true),
                                              new XElement(KeyAutoExpandUrls, true),
                                              new XElement(KeyScriptingEnabled, true),
                                              new XElement(KeyRichAnimations, false),
                                              new XElement(KeyIsCPURendering, false),
                                              new XElement(KeyMapEngine, 0),
                                              new XElement(KeyMediaHandling, 0),
                                              new XElement("Window",
                                                           new XElement("WindowXPos", 0),
                                                           new XElement("WindowYPos", 0),
                                                           new XElement("WindowHeight", 400),
                                                           new XElement("WindowWidth", 800)),
                                              new XElement(KeyGlobalIgnore, new List<String>()),
                                              new XElement(KeySavedSearches, new List<SavedSearch>()),
                                              new XElement(KeyPauseStreams, true),
                                              new XElement(KeyDisableMediaProviders, false),
                                              new XElement(KeyInstallationId, Guid.NewGuid().ToString()),
                                              new XElement(KeySelectedAccounts, new List<String>()),
                                              new XElement(KeyAutoShorten, false),
                                              new XElement(KeyDefaultShortener, ""),
                                              new XElement(KeyStyleFontSizeKey, 12)
                                     ));
        }

        private void SaveSearches()
        {
            var node = _doc.GetElement(KeySavedSearches);

            if (node == null) return;
            node.RemoveAll();

            foreach (var key in SavedSearches)
            {
                node.Add(new XElement("Search",
                                      new XElement("Term", key.SearchTerm.EscapeXml()),
                                      new XElement("Position", key.Position),
                                      key.Providers.Select(s => new XElement("Provider", s))));
            }
        }

        private void SaveSelectedAccounts()
        {
            var elementsToSave = SelectedAccounts.Distinct();

            var node = _doc.GetElement(KeySelectedAccounts);
            node.RemoveAll();

            foreach (var child in elementsToSave)
            {
                node.Add(new XElement("Selected", child));
            }
        }
    }
}