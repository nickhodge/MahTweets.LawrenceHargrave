using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Interfaces.Plugins
{
    [InheritedExport]
    public interface IStatusUpdate
    {
        string ID { get; set; }

        IContact Contact { get; set; }

        string Text { get; set; }

        bool IsRead { get; set; }

        DateTime Time { get; set; }

        IList<UpdateType> Types { get; set; }

        ObservableCollection<IMicroblog> Parents { get; set; }

        GeoLocation Location { get; set; }

        string ColorARGB { get; set; }

        ObservableCollection<IStatusUpdateAttachment> Attachments { get; set; }

        IMicroblog Microblog { get; set; }

        bool Filter(string ignore);

        bool HasAttachment(string url);

        void RemoveAttachment(string url);
    }
}