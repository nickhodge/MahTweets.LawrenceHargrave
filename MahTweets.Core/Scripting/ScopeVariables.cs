using System.Collections.ObjectModel;

namespace MahTweets.Core.Scripting
{
    public class ScopeVariables : KeyedCollection<string, ScopeVariable>
    {
        protected override string GetKeyForItem(ScopeVariable item)
        {
            return item.variableName;
        }

        public ScopeVariable GetOrAddItem(string vn, object vo)
        {
            if (Contains(vn))
            {
                return this[vn];
            }

            var nsv = new ScopeVariable(vn, vo);
            Add(nsv);
            return nsv;
        }
    }
}