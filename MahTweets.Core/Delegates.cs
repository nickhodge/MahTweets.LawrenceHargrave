using System;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Filters;

namespace MahTweets.Core
{
    public delegate void AddNotificationDelegate(ShowNotificationPayload payload);

    public delegate void NewStreamDelegate(StreamModel f);

    public delegate void EmptyDelegate();

    public delegate void PulseEventHandler(object sender, EventArgs e);
}