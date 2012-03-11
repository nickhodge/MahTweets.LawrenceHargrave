namespace MahTweets.Core.Interfaces.Plugins
{
    //must make a trivial concrete derived class from this in your plugin (if used), for serialization of filters
    public abstract class SelfUpdate : UpdateType
    {
        public SelfUpdate()
        {
            Type = "Self";
            ColorARGB = "#00FFFFFF";
            Order = 2;
        }
    }
}