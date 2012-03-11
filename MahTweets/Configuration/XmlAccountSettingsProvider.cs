using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using MahTweets.Core;
using MahTweets.Core.Collections;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Helpers;

namespace MahTweets.Configuration
{
    public class AccountSettingsProvider : IAccountSettingsProvider
    {
        private readonly SymmetricAlgorithm _algorithm;
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly IStorage _storage;
        private IList<string> _activeStatusHandlers;
        private XDocument _doc;
        private string _docPath;
        private IList<Credential> _microblogCredentials;
        private IList<Credential> _statusHandlerCredentials;
        private IList<Credential> _urlShortenerCredentials;

        public AccountSettingsProvider()
        {
            _algorithm = new RijndaelManaged();
            _hashAlgorithm = new SHA384Managed();
            _storage = CompositionManager.Get<IStorage>();
            SetDocument(_storage.CombineFullPath("Accounts Settings.xml"));
        }

        public AccountSettingsProvider(string docPath)
        {
            _algorithm = new RijndaelManaged();
            _hashAlgorithm = new SHA384Managed();

            SetDocument(docPath);
        }

        #region IAccountSettingsProvider Members

        public IList<string> ActiveStatusHandlers
        {
            get
            {
                if (_activeStatusHandlers != null)
                    return _activeStatusHandlers;

                _activeStatusHandlers = new List<string>();
                XElement node = _doc.GetElement("ActiveStatusHandlers");
                XElement salt = _doc.GetElement("Salt");
                foreach (XElement child in node.DescendantNodes().OfType<XElement>())
                {
                    string encryptedValue = child.Value;
                    if (string.IsNullOrEmpty(encryptedValue)) continue;
                    try
                    {
                        XElement elem = DecryptElement(encryptedValue, salt.Value);
                        _activeStatusHandlers.Add(elem.Value);
                    }
                    catch (Exception ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }
                }

                return _activeStatusHandlers;
            }
        }

        public IList<Credential> UrlShortenerCredentials
        {
            get
            {
                if (_urlShortenerCredentials != null)
                    return _urlShortenerCredentials;

                _urlShortenerCredentials = new List<Credential>();

                XElement node = _doc.GetElement("UrlShortenerCredentials");
                XElement salt = _doc.GetElement("Salt");
                foreach (XElement child in node.DescendantNodes().OfType<XElement>())
                {
                    string encryptedValue = child.Value;
                    if (string.IsNullOrEmpty(encryptedValue)) continue;

                    XElement elem = DecryptElement(encryptedValue, salt.Value);

                    Credential c = DeserializeCredential(elem);
                    _urlShortenerCredentials.Add(c);
                }

                return _urlShortenerCredentials;
            }
        }

        public IList<Credential> MicroblogCredentials
        {
            get
            {
                if (_microblogCredentials != null)
                    return _microblogCredentials;

                _microblogCredentials = new List<Credential>();

                XElement node = _doc.GetElement("MicroblogCredentials");
                XElement salt = _doc.GetElement("Salt");
                foreach (XElement child in node.DescendantNodes().OfType<XElement>())
                {
                    string encryptedValue = child.Value;
                    if (string.IsNullOrEmpty(encryptedValue)) continue;

                    XElement elem = DecryptElement(encryptedValue, salt.Value);

                    Credential c = DeserializeCredential(elem);
                    _microblogCredentials.Add(c);
                }

                return _microblogCredentials;
            }
        }


        public IList<Credential> StatusHandlerCredentials
        {
            get
            {
                if (_statusHandlerCredentials != null)
                    return _statusHandlerCredentials;

                _statusHandlerCredentials = new List<Credential>();
                XElement node = _doc.GetElement("StatusHandlerCredentials");
                XElement salt = _doc.GetElement("Salt");
                foreach (XElement child in node.DescendantNodes().OfType<XElement>())
                {
                    string encryptedValue = child.Value;
                    if (string.IsNullOrEmpty(encryptedValue)) continue;

                    XElement elem = DecryptElement(encryptedValue, salt.Value);

                    Credential c = DeserializeCredential(elem);
                    _statusHandlerCredentials.Add(c);
                }

                return _statusHandlerCredentials;
            }
        }

        public void Save()
        {
            SaveMicroblogCredentials();
            SaveUrlShortenerCredentials();
            SaveStatusHandlerCredentials();
            SaveActiveStatusHandlers();

            _doc.Save(_docPath);
        }

        public void Reset()
        {
            File.Delete(_docPath);

            _activeStatusHandlers = null;
            _microblogCredentials = null;
            _urlShortenerCredentials = null;
            _statusHandlerCredentials = null;

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
            return new XDocument(new XElement("Accounts",
                                              new XElement("Salt", SecurityHelper.GetNewSalt()),
                                              new XElement("MicroblogCredentials"),
                                              new XElement("UrlShortenerCredentials"),
                                              new XElement("StatusHandlerCredentials"),
                                              new XElement("ActiveStatusHandlers"),
                                              new XElement("DefaultShortener")
                                     ));
        }

        private void SaveActiveStatusHandlers()
        {
            XElement salt = _doc.GetElement("Salt");
            XElement node = _doc.GetElement("ActiveStatusHandlers");
            node.RemoveAll();
            foreach (string child in ActiveStatusHandlers.Distinct())
            {
                string encryptedValue = EncryptValue(child, salt.Value);

                node.Add(new XElement("Handler", encryptedValue));
            }
        }

        private void SaveMicroblogCredentials()
        {
            XElement salt = _doc.GetElement("Salt");
            XElement node = _doc.GetElement("MicroblogCredentials");
            node.RemoveAll();
            foreach (Credential child in MicroblogCredentials.Distinct())
            {
                XElement elem = SerializeCredential(child);
                string encryptedValue = EncryptValue(elem.ToString(), salt.Value);

                node.Add(new XElement("Credential", encryptedValue));
            }
        }


        private void SaveUrlShortenerCredentials()
        {
            XElement salt = _doc.GetElement("Salt");
            XElement node = _doc.GetElement("UrlShortenerCredentials");
            node.RemoveAll();
            foreach (Credential child in UrlShortenerCredentials.Distinct())
            {
                XElement elem = SerializeCredential(child);
                string encryptedValue = EncryptValue(elem.ToString(), salt.Value);

                node.Add(new XElement("Credential", encryptedValue));
            }
        }

        private void SaveStatusHandlerCredentials()
        {
            XElement salt = _doc.GetElement("Salt");
            XElement node = _doc.GetElement("StatusHandlerCredentials");
            node.RemoveAll();
            foreach (Credential child in StatusHandlerCredentials.Distinct())
            {
                XElement elem = SerializeCredential(child);
                string encryptedValue = EncryptValue(elem.ToString(), salt.Value);

                node.Add(new XElement("Credential", encryptedValue));
            }
        }

        private XElement SerializeCredential(Credential child)
        {
            var elem = new XElement("Credential",
                                    new XElement("AccountName", child.AccountName),
                                    new XElement("UserID", child.UserID),
                                    new XElement("Username", child.Username),
                                    new XElement("Protocol", child.Protocol),
                                    new XElement("Password", child.Password),
                                    new XElement("CustomSettings", SerializeDictionary(child.CustomSettings)));

            return elem;
        }

        private static Credential DeserializeCredential(XElement child)
        {
            var credential = new Credential();
            XElement xElement = child.Element("AccountName");
            if (xElement != null) credential.AccountName = xElement.Value;
            XElement element = child.Element("UserID");
            if (element != null) credential.UserID = element.Value;
            XElement xElement1 = child.Element("Username");
            if (xElement1 != null) credential.Username = xElement1.Value;
            XElement element1 = child.Element("Protocol");
            if (element1 != null) credential.Protocol = element1.Value;
            XElement xElement2 = child.Element("Password");
            if (xElement2 != null) credential.Password = xElement2.Value;
            XElement element2 = child.Element("CustomSettings");
            if (element2 != null)
                credential.CustomSettings = DeserializeDictionary(element2.Value);

            return credential;
        }

        private static SerializableDictionary<string, string> DeserializeDictionary(string p)
        {
            return Serialization.Deserialize<SerializableDictionary<string, string>>(p, Serialization.SerializerType.Xml,
                                                                                     null);
        }

        private static string SerializeDictionary(SerializableDictionary<string, string> dictionary)
        {
            return Serialization.Serialize(dictionary, Serialization.SerializerType.Xml, null);
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