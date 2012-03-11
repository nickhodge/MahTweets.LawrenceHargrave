using System.Collections.ObjectModel;
using System.IO;

namespace MahTweets.Core.Scripting
{
    public class ScriptDirectories : KeyedCollection<string, ScriptDirectory>
    {
        protected override string GetKeyForItem(ScriptDirectory item)
        {
            return item.LongPath;
        }

        public ScriptDirectory GetParentDirectoryForScript(string spath)
        {
            string sdir = Path.GetDirectoryName(spath) + Path.DirectorySeparatorChar;
            if (Directory.Exists(sdir))
            {
                if (Contains(sdir))
                    return this[sdir];
            }
            return null;
        }

        public ScriptDirectory GetOrAddItem(string sd)
        {
            if (Contains(sd))
            {
                return this[sd];
            }

            if (Directory.Exists(sd))
            {
                var nsd = new ScriptDirectory(sd);
                Add(nsd);
                return nsd;
            }

            return null;
        }
    }
}