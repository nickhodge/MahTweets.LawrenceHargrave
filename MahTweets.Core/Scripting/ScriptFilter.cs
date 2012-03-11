using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace MahTweets.Core.Scripting
{
    [DataContract]
    public class ScriptFilter
    {
        [DataMember]
        public string ScriptKey { get; set; }

        [DataMember]
        public string ScriptDescription { get; set; }

        [DataMember]
        public Color ScriptFilterColor { get; set; }

    }

    public class ScriptFilterEq : EqualityComparer<ScriptFilter>
    {
 
        public override bool Equals(ScriptFilter x, ScriptFilter y)
        {
            return Equals(x.ScriptKey.ToLower(), y.ScriptKey.ToLower());
        }

        public override int GetHashCode(ScriptFilter obj)
        {
            return obj!=null ? obj.GetHashCode() : 0;
        }
    }
}