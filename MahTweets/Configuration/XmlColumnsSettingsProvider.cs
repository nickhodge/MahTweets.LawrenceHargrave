using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Factory;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Helpers;

namespace MahTweets.Configuration
{
    public class ColumnsSettingsProvider : IColumnsSettingsProvider
    {
        private readonly SymmetricAlgorithm _algorithm;
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly IStorage _storage;
        private IList<ColumnConfiguration> _columns;
        private XDocument _doc;
        private string _docPath;
        private IList<StreamModel> _filters;

        public ColumnsSettingsProvider()
        {
            _algorithm = new RijndaelManaged();
            _hashAlgorithm = new SHA384Managed();
            _storage = CompositionManager.Get<IStorage>();
            SetDocument(_storage.CombineDocumentsFullPath("Saved Columns.mtcolumns"));
                //store this somewhere more visible to the end user, and easily transported from device to device
        }

        public ColumnsSettingsProvider(string docPath)
        {
            _algorithm = new RijndaelManaged();
            _hashAlgorithm = new SHA384Managed();

            SetDocument(docPath);
        }

        #region IColumnsSettingsProvider Members

        public IList<StreamModel> Filters
        {
            get
            {
                if (_filters != null)
                    return _filters;

                _filters = new List<StreamModel>();
                XElement node = _doc.GetElement("Filters");
                if (node != null)
                {
                    XElement salt = _doc.GetElement("Salt");
                    foreach (XElement child in node.DescendantNodes().OfType<XElement>())
                    {
                        string encryptedValue = child.Value;
                        if (string.IsNullOrEmpty(encryptedValue)) continue;
                        try
                        {
                            XElement elem = DecryptElement(encryptedValue, salt.Value);

                            StreamModel f = DeserializeFilters(elem);
                            _filters.Add(f);
                        }
                        catch (Exception ex)
                        {
                            CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                        }
                    }
                }
                return _filters;
            }
        }

        public IList<ColumnConfiguration> Columns
        {
            get
            {
                if (_columns != null)
                    return _columns;

                _columns = new List<ColumnConfiguration>();
                XElement node = _doc.GetElement("ColumnConfigurations");
                if (node != null)
                {
                    foreach (XElement child in node.DescendantNodes().OfType<XElement>())
                    {
                        try
                        {
                            ColumnConfiguration f = DeserializeColumns(child);
                            _columns.Add(f);
                        }
                        catch (Exception ex)
                        {
                            CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                        }
                    }
                }
                return _columns;
            }
        }

        public void Save()
        {
            SaveFilters();
            SaveColumns();
            _doc.Save(_docPath);
        }

        public void Reset()
        {
            File.Delete(_docPath);

            _filters = null;

            _doc = CreateDefaultXmlFile();
        }

        #endregion

        private void SetDocument(string path)
        {
            _docPath = path;
            _doc = !File.Exists(_docPath) ? CreateDefaultXmlFile() : XDocument.Load(_docPath);
        }

        private static XDocument CreateDefaultXmlFile()
        {
            // TODO: override to allow new salt to be added
            return new XDocument(new XElement("Columns",
                                              new XElement("Salt", SecurityHelper.GetNewSalt()),
                                              new XElement("ColumnConfigurations"),
                                              new XElement("Sources"),
                                              new XElement("Filters")));
        }

        private void SaveFilters()
        {
            XElement salt = _doc.GetElement("Salt");
            XElement node = _doc.GetElement("Filters");
            if (node == null) return;
            node.RemoveAll();
            foreach (StreamModel child in Filters.Distinct())
            {
                // bugfix while testing - remove duplicates before saving
                child.Filters = new ObservableCollection<Filter>(child.Filters.Distinct());

                XElement elem = SerializeFilters(child);
                string encryptedValue = EncryptValue(elem.ToString(), salt.Value);

                node.Add(new XElement("Filter", encryptedValue));
            }
        }

        private void SaveColumns()
        {
            XElement node = _doc.GetElement("ColumnConfigurations");
            if (node == null) return;
            node.RemoveAll();
            foreach (ColumnConfiguration child in Columns.Distinct(new ColumnConfigurationComparer()))
            {
                XElement elem = SerializeColumns(child);
                node.Add(elem);
            }
        }

        private static XElement SerializeFilters(StreamModel f)
        {
            return new XElement("StreamModel", Serialization.Serialize(f, Serialization.SerializerType.Xml, null));
        }

        private static StreamModel DeserializeFilters(XElement elem)
        {
            return Serialization.Deserialize<StreamModel>(elem.Value, Serialization.SerializerType.Xml, null);
        }

        private static XElement SerializeColumns(ColumnConfiguration f)
        {
            return new XElement("ColumnConfiguration",
                                Serialization.Serialize(f, Serialization.SerializerType.Xml, null));
        }

        private static ColumnConfiguration DeserializeColumns(XElement elem)
        {
            return Serialization.Deserialize<ColumnConfiguration>(elem.Value, Serialization.SerializerType.Xml, null);
        }

        private XElement DecryptElement(string encryptedValue, string salt)
        {
            byte[] b = _hashAlgorithm.ComputeHash(new ASCIIEncoding().GetBytes(salt));
            var key = new byte[32];
            var vector = new byte[16];

            Array.Copy(b, 0, key, 0, 32);
            Array.Copy(b, 32, vector, 0, 16);

            byte[] cipher = Convert.FromBase64String(encryptedValue);

            ICryptoTransform encryptor = _algorithm.CreateDecryptor(key, vector);

            string output;

            using (var memoryStream = new MemoryStream(cipher))
            {
                using (var crptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Read))
                {
                    var data = new byte[cipher.Length];
                    int dataLength = crptoStream.Read(data, 0, data.Length);
                    output = (new ASCIIEncoding()).GetString(data, 0, dataLength);
                }
            }

            return XElement.Parse(output);
        }

        private string EncryptValue(string value, string salt)
        {
            byte[] b = _hashAlgorithm.ComputeHash(new ASCIIEncoding().GetBytes(salt));
            var key = new byte[32];
            var vector = new byte[16];

            Array.Copy(b, 0, key, 0, 32);
            Array.Copy(b, 32, vector, 0, 16);

            byte[] data = new ASCIIEncoding().GetBytes(value);

            ICryptoTransform encryptor = _algorithm.CreateEncryptor(key, vector);

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}