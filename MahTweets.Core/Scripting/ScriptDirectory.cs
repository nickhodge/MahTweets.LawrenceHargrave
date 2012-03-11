using System.IO;

namespace MahTweets.Core.Scripting
{
    public class ScriptDirectory
    {
        public ScriptDirectory(string lp)
        {
            LongPath = lp;
        }

        public string Key
        {
            get { return LongPath; }
        }

        public string LongPath { get; set; }

        public string ShortPath
        {
            get { return LongPath; }
            set { LongPath = value; }
        }

        public FileSystemWatcher Watcher { get; set; }
    }
}