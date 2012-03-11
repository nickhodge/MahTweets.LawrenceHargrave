namespace MahTweets.Core.Scripting
{
    /// <summary>
    /// base class for the entry point in each script
    /// </summary>
    public class ScriptEntryPoint
    {
        public ScriptEntryPoint(string entrypoint, dynamic dynamicmethod, string doc)
        {
            EntryPoint = entrypoint;
            DynamicMethod = dynamicmethod;
            ScriptDescription = doc;
        }

        public string EntryPoint { get; private set; }

        public dynamic DynamicMethod { get; set; }

        public string ScriptDescription { get; set; }
    }
}