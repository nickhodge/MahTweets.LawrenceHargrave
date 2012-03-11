namespace MahTweets.Core.Interfaces.Plugins
{
    //must make a trivial concrete derived class from this in your plugin (if used), for serialization of filters
    public abstract class NormalUpdate : UpdateType
    {
        public NormalUpdate()
        {
            Type = "Normal";
            ColorARGB = "#00FFFFFF";
            Order = 1;
        }
    }
}