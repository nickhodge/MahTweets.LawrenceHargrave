using System.Collections.Generic;
using MahTweets.Core.Factory;

namespace MahTweets.Core.Events.EventTypes
{
    public class ShowColumns : CompositePresentationEvent<ShowColumnsPayload>
    {
    }

    public class ShowColumnsPayload
    {
        public readonly List<ColumnConfiguration> Columns;

        public ShowColumnsPayload(List<ColumnConfiguration> element)
        {
            Columns = element;
        }
    }
}