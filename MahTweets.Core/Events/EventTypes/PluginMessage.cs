namespace MahTweets.Core.Events.EventTypes
{
    public class PluginMessage : CompositePresentationEvent<Message>
    {
    }

    public class Message
    {
        public string Text { get; private set; }

        public static Message Of(string text)
        {
            return new Message {Text = text};
        }
    }
}