namespace MahTweets.Core
{
    public class Link
    {
        public Link(string s)
        {
            Uri = s;
        }

        public string Uri { get; private set; }
    }
}