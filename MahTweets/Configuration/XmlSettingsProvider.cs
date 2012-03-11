using System;
using System.Linq;
using System.Xml.Linq;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Helpers;

namespace MahTweets.Configuration
{
    public class XmlSettingsProvider : ISettingsProvider
    {
        private readonly IAccountSettingsProvider _accountSettings;
        private readonly IApplicationSettingsProvider _appSettings;
        private readonly IColumnsSettingsProvider _columnsSettings;
        private readonly IPluginSettingsProvider _pluginSettings;

        public XmlSettingsProvider()
        {
            _appSettings = CompositionManager.Get<IApplicationSettingsProvider>();
            _accountSettings = CompositionManager.Get<IAccountSettingsProvider>();
            _columnsSettings = CompositionManager.Get<IColumnsSettingsProvider>();
            _pluginSettings = CompositionManager.Get<IPluginSettingsProvider>();
        }

        public IColumnsSettingsProvider Columns
        {
            get { return _columnsSettings; }
        }

        public IPluginSettingsProvider PluginSettings
        {
            get { return _pluginSettings; }
        }

        #region ISettingsProvider Members

        public IApplicationSettingsProvider Application
        {
            get { return _appSettings; }
        }

        public IAccountSettingsProvider Account
        {
            get { return _accountSettings; }
        }

        #endregion

        public void SaveAll()
        {
            _accountSettings.Save();
            _columnsSettings.Save();
            _appSettings.Save();
        }

        public void ResetAll()
        {
            _appSettings.Save();
            _columnsSettings.Save();
            _accountSettings.Save();
        }
    }


    public static class XDocumentExtension
    {
        public static XElement GetElement(this XDocument doc, string index)
        {
            try
            {
                XElement node =
                    doc.DescendantNodes().OfType<XElement>()
                        .Where(x => string.Compare(x.Name.LocalName, index) == 0)
                        .SingleOrDefault();

                return node;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }

            return null;
        }

        public static string GetValue(this XDocument doc, string index)
        {
            XElement node = doc.GetElement(index);

            return node == null ? string.Empty : node.Value;
        }

        public static bool GetValueBool(this XDocument doc, string index)
        {
            XElement node = doc.GetElement(index);

            return node != null && SecurityHelper.GetValue<bool>(node);
        }

        public static int GetValueInt(this XDocument doc, string index)
        {
            XElement node = doc.GetElement(index);

            if (node == null) return -1;

            return SecurityHelper.GetValue<int>(node);
        }

        public static double GetValueDouble(this XDocument doc, string index)
        {
            XElement node = doc.GetElement(index);

            if (node == null) return -1;

            return SecurityHelper.GetValue<double>(node);
        }

        public static void SetValue(this XDocument doc, string index, object value)
        {
            XElement node = doc.GetElement(index);

            if (node == null)
            {
                var xE = new XElement(index);
                doc.Element("Settings").Add(xE);
                node = doc.GetElement(index);
            }

            node.Value = value.ToString();
        }
    }
}