namespace MahTweets.Core.Events.EventTypes
{
    public class PasteInlineReply : CompositePresentationEvent<PasteInlineReplyPayload>
    {
    }

    public class PasteInlineReplyPayload
    {
        public PasteInlineReplyPayload(string textToPaste, string idToPaste)
        {
            Text = textToPaste;
            id = idToPaste;
        }

        public string Text { get; set; }

        public string id { get; set; }
    }
}