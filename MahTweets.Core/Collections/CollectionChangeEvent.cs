using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Collections
{
    public class CollectionChangeEvent
    {
        public CollectionChangeEvent(object item, int index, CollectionChangeType type)
        {
            Item = item;
            OldIndex = NewIndex = index;
            Type = type;
        }

        public CollectionChangeEvent(IStatusUpdate item, CollectionChangeType type)
        {
            Item = item;
            Type = type;
        }

        public CollectionChangeEvent(CollectionChangeType type)
        {
            Type = type;
        }

        public object Item { get; private set; }

        public CollectionChangeType Type { get; private set; }

        public int OldIndex { get; private set; }

        public int NewIndex { get; private set; }
    }
}