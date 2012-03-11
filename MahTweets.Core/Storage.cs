using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core
{
    public class Storage : IStorage
    {
        private readonly string APP_DATA_PATH;
        private readonly string EXECUTING_APP_PATH;
        private readonly string USER_APP_DATA_PATH;

        private const string APP_DATA_SUBST = "%appdata%";
        private const string EXECUTING_APP_SUBST = "%application%";
        private const string USER_APP_DATA_SUBST = "%userdata%";

        public Storage()
        {
            APP_DATA_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                     "MahTweets");
            USER_APP_DATA_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                              "MahTweets");
            EXECUTING_APP_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        #region IStorage Members

        public void Store<T>(T store, string filename, IEnumerable<Type> KnownTypes)
        {
            if (!Directory.Exists(APP_DATA_PATH))
            {
                Directory.CreateDirectory(APP_DATA_PATH);
            }
            var fs = File.Create(CombineFullPath(filename));
            var sw = new StreamWriter(fs);
            sw.Write(Serialization.Serialize(store, Serialization.SerializerType.Xml, KnownTypes));
            sw.Flush();
            sw.Close();
        }

        public async Task<T> Load<T>(string filename)
        {
            try
            {
                var fs = File.Open(CombineFullPath(filename), FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(fs);
                var content = await sr.ReadToEndAsync();
                sr.Close();

                return Serialization.Deserialize<T>(content, Serialization.SerializerType.Xml, null);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            return default(T);
        }

        public bool Exists(string filename)
        {
            return File.Exists(CombineFullPath(filename));
        }

        public string CombineFullPath(string filename)
        {
            return Path.Combine(APP_DATA_PATH, filename);
        }

        public string GetFullPath()
        {
            return APP_DATA_PATH;
        }

        public string GetDecodeTokenisedPath(string pathToDetokenize)
        {
            var sb = new StringBuilder();
            foreach (var c in pathToDetokenize.Split(Path.DirectorySeparatorChar))
            {
                switch (c.ToLower())
                {
                    case EXECUTING_APP_SUBST:
                        {
                            sb.Append(EXECUTING_APP_PATH);
                            break;
                        }
                    case APP_DATA_SUBST:
                        {
                            sb.Append(APP_DATA_PATH);
                            break;
                        }
                    case USER_APP_DATA_SUBST:
                        {
                            sb.Append(USER_APP_DATA_PATH);
                            break;
                        }
                    default:
                        {
                            sb.Append(c);
                            break;
                        }
                }
                sb.Append(Path.DirectorySeparatorChar);
            }
            return sb.ToString();
        }

        public string CombineDocumentsFullPath(string filename)
        {
            if (!Directory.Exists(USER_APP_DATA_PATH))
                try
                {
                    Directory.CreateDirectory(USER_APP_DATA_PATH);
                }
                catch (Exception ex)
                {
                    CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    return null;
                }
            return Path.Combine(USER_APP_DATA_PATH, filename);
        }

        public string GetDocumentsFullPath()
        {
            return USER_APP_DATA_PATH;
        }

        public string GetApplicationFullPath()
        {
            return EXECUTING_APP_PATH;
        }

        #endregion
    }
}