using System;
using System.Collections.Generic;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IStatusUpdateRepository
    {
        bool UpdateOrCreateStatus<T>(string id, Func<string, T> createNew, Func<T, T> updateExisting)
            where T : class, IStatusUpdate;

        T GetById<T>(string id)
            where T : class, IStatusUpdate;

        T GetById<T>(string id, T defaultIfNotFound)
            where T : class, IStatusUpdate;

        void Send(IEnumerable<IStatusUpdate> updates);

        void Send(IStatusUpdate update);
    }
}