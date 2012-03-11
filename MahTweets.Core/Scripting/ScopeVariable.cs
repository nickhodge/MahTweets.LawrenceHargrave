namespace MahTweets.Core.Scripting
{
    public class ScopeVariable
    {
        public ScopeVariable(string vn, object vo)
        {
            variableName = vn;
            variableObject = vo;
        }

        public string variableName { get; set; }
        public object variableObject { get; set; }
    }
}