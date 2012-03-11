using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Helpers;

namespace MahTweets.Configuration
{
    public class PluginSettingsProvider : IPluginSettingsProvider
    {
        private readonly string _docPath;
        private readonly IStorage _storage;

        private XDocument _doc;

        public PluginSettingsProvider()
        {
            _storage = CompositionManager.Get<IStorage>();
            _docPath = _storage.CombineFullPath("Plugin Settings.xml");
            _doc = !File.Exists(_docPath) ? CreateDefaultXmlFile() : XDocument.Load(_docPath);
        }

        public PluginSettingsProvider(string docPath)
        {
            _docPath = docPath;
            _doc = !File.Exists(_docPath) ? CreateDefaultXmlFile() : XDocument.Load(_docPath);
        }

        #region IPluginSettingsProvider Members

        public void Save()
        {
            _doc.Save(_docPath);
        }

        public void Reset()
        {
            File.Delete(_docPath);
            _doc = CreateDefaultXmlFile();
        }

        public void Set<T>(IPlugin plugin, string key, T value)
        {
            XElement elem = GetPluginElement(plugin);

            XElement findChild = elem.Descendants().SingleOrDefault(x => x.Name == key);

            if (findChild == null)
            {
                findChild = new XElement(key);
                elem.Add(findChild);
            }

            SecurityHelper.SetValue<T>(findChild, key, value);
        }

        public T Get<T>(IPlugin plugin, string key)
        {
            if (!Has(plugin, key))
                return default(T);

            XElement elem = GetPluginElement(plugin);

            XElement findChild = elem.Descendants().Where(x => x.Name == key).SingleOrDefault();

            if (findChild == null) return default(T);

            return SecurityHelper.GetValue<T>(findChild);
        }

        public bool HasSettingsFor(IPlugin plugin)
        {
            string typeName = plugin.GetType().FullName;
            string identifier = plugin.Id;

            List<XElement> elem =
                _doc.Descendants().Where(x => x.Attribute("Type") != null && x.Attribute("Identifier") != null)
                    .Where(y =>
                           y.Name == "Plugin"
                           && y.Attribute("Type").Value.Matches(typeName)
                           && y.Attribute("Identifier").Value.Matches(identifier)).ToList();


            return (elem.Count > 0);
        }

        #endregion

        private static XDocument CreateDefaultXmlFile()
        {
            return new XDocument(new XElement("Settings", new XElement("Salt", SecurityHelper.GetNewSalt())
                                              , new XElement("Plugins")));
        }

        private bool Has(IPlugin plugin, string key)
        {
            XElement elem = GetPluginElement(plugin);

            IEnumerable<XElement> findChild = elem.Descendants().Where(x => x.Name == key);

            return findChild.Count() == 1;
        }

        private XElement GetPluginElement(IPlugin plugin)
        {
            string typeName = plugin.GetType().FullName;
            string identifier = plugin.Id;

            List<XElement> elem =
                _doc.Descendants().Where(x => x.Attribute("Type") != null && x.Attribute("Identifier") != null)
                    .Where(y =>
                           y.Name == "Plugin"
                           && y.Attribute("Type").Value.Matches(typeName)
                           && y.Attribute("Identifier").Value.Matches(identifier)).ToList();

            if (elem.Count == 0)
            {
                var element = new XElement("Plugin");
                element.SetAttributeValue("Type", typeName);
                element.SetAttributeValue("Identifier", identifier);

                XElement singleOrDefault = _doc.Descendants("Plugins").SingleOrDefault();
                if (singleOrDefault != null)
                    singleOrDefault.Add(element);

                return element;
            }

            if (elem.Count > 1)
                throw new InvalidOperationException(
                    "More than one plugin instance matches this identifier and type information");

            return elem.SingleOrDefault();
        }
    }
}