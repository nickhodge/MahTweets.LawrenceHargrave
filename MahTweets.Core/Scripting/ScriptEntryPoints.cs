using System.Collections.ObjectModel;

namespace MahTweets.Core.Scripting
{
    public class ScriptEntryPoints : KeyedCollection<string, ScriptEntryPoint>
    {
        protected override string GetKeyForItem(ScriptEntryPoint item)
        {
            return item.EntryPoint;
        }

        public void SetItem(string ep, dynamic dm, string doc)
        {
            Add(new ScriptEntryPoint(ep, dm, doc));
        }

        public ScriptEntryPoint GetOrAddItem(string ep, dynamic dm, string doc)
        {
            if (Contains(ep))
            {
                this[ep].ScriptDescription = doc;
                this[ep].DynamicMethod = dm;
                return this[ep];
            }

            var sep = new ScriptEntryPoint(ep, dm, doc);
            Add(sep);
            return sep;
        }
    }
}