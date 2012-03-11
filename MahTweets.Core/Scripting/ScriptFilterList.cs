using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace MahTweets.Core.Scripting
{
    public class ScriptFilterComparer : IEqualityComparer<ScriptFilter>
    {
        #region IEqualityComparer<ScriptFilter> Members

        public bool Equals(ScriptFilter sf1, ScriptFilter sf2)
        {
            return sf1.ScriptKey == sf2.ScriptKey;
        }

        public int GetHashCode(ScriptFilter sf1)
        {
            // how sad and bogus is this?
            return sf1.SadRandomHashIHope;
        }

        #endregion

        public bool Equals(String sfkey1, ScriptFilter sf2)
        {
            return sfkey1 == sf2.ScriptKey;
        }

        public bool Equals(ScriptFilter sf1, string sfkey2)
        {
            return sf1.ScriptKey == sfkey2;
        }
    }

    [DataContract]
    public class ScriptFilter
    {
        public static int _masterscriptfilterhash;

        public ScriptFilter()
        {
            SadRandomHashIHope = _masterscriptfilterhash++;
        }

        [DataMember]
        public string ScriptKey { get; set; }

        [DataMember]
        public string ScriptDescription { get; set; }

        [DataMember]
        public Color ScriptFilterColor { get; set; }

        public int SadRandomHashIHope { get; set; }
    }

    public class ScriptFilterEq : EqualityComparer<ScriptFilter>
    {
        public override bool Equals(ScriptFilter x, ScriptFilter y)
        {
            return x.ScriptKey.ToLower() == y.ScriptKey.ToLower();
        }

        public override int GetHashCode(ScriptFilter obj)
        {
            if (obj != null) return obj.SadRandomHashIHope;
            return 0;
        }
    }
}