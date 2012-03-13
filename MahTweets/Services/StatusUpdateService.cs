using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using MahTweets.Core.Collections;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Services
{
    public class StatusUpdateService : IStatusUpdateService, IDisposable
    {
        private const int DelayBetweenPush = 10;

        private readonly DispatcherTimer _dispatchTimer;

        private readonly Queue<IStatusUpdate> _incomingUpdates;
        private readonly object _lock = new object();

        /// <summary>
        /// Key = string (from IUpdateType.ID), Value = List of IStatusUpdate 
        /// </summary>
        private readonly Hashtable _statusesById = new Hashtable();

        private IEnumerable<IStatusHandler> _statusHandlers;

        public StatusUpdateService()
        {
            _incomingUpdates = new Queue<IStatusUpdate>();
            OutgoingUpdates = new ThreadSafeObservableCollection<IStatusUpdate>();

            _dispatchTimer = new DispatcherTimer(DispatcherPriority.Background)
                                 {Interval = TimeSpan.FromMilliseconds(DelayBetweenPush)};
            _dispatchTimer.Tick += DispatchTimerTick;
            _dispatchTimer.Start();
        }

        public IEnumerable<IStatusHandler> StatusHandlers
        {
            get { return _statusHandlers ?? (_statusHandlers = CompositionManager.GetAll<IStatusHandler>()); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _dispatchTimer.Stop();
            _dispatchTimer.Tick -= DispatchTimerTick;
        }

        #endregion

        #region IStatusUpdateService Members

        public ThreadSafeObservableCollection<IStatusUpdate> OutgoingUpdates { get; private set; }

        public void Reset()
        {
            lock (_lock)
            {
                _statusesById.Clear();
                OutgoingUpdates.Clear();
            }
        }

        public void MarkAllRead()
        {
            lock (_lock)
            {
                foreach (IStatusUpdate s in OutgoingUpdates)
                    s.IsRead = true;
            }
        }

        public void Send(IEnumerable<IStatusUpdate> updates)
        {
            ThreadPool.QueueUserWorkItem(state =>
                                             {
                                                 List<IStatusUpdate> newUpdates =
                                                     updates.Where(SendStatusUpdateInternal).ToList();

                                                 if (newUpdates.Any())
                                                 {
                                                     StatusHandlers.Where(sh => sh.Enabled).ForEach(
                                                         sh => sh.HandleUpdates(newUpdates));
                                                 }
                                             });
        }

        public void Send(IStatusUpdate update)
        {
            if (SendStatusUpdateInternal(update))
            {
                StatusHandlers.Where(sh => sh.Enabled).ForEach(sh => sh.HandleUpdate(update));
            }
        }

        public bool UpdateOrCreateStatus<T>(string id, Func<string, T> createNew, Func<T, T> updateExisting)
            where T : class, IStatusUpdate
        {
            // See if we can return the ID if it already exists before trying to get a lock 
            if (_statusesById.ContainsKey(id))
            {
                var listUpdates = _statusesById[id] as IList<IStatusUpdate>;
                if (listUpdates == null) return false; // default(T);

                //if (listUpdates.Count(s => s.GetType() == typeof(T)) >= 0)
                //{
                // BF - potential concurrency issue here when using .Single() with multiple items
                T existing = listUpdates.OfType<T>().FirstOrDefault();
                if (existing != null)
                    lock (existing)
                    {
                        updateExisting(existing);
                        return false;
                    }
                //}
            }

            // Doesn't exist, so lock
            lock (_lock)
            {
                List<IStatusUpdate> listUpdates;

                if (!_statusesById.ContainsKey(id))
                {
                    listUpdates = new List<IStatusUpdate>();
                    _statusesById.Add(id, listUpdates);
                }
                else
                {
                    listUpdates = (List<IStatusUpdate>) _statusesById[id];
                }

                // Check: Was it already created before we got the lock? 
                if (listUpdates.Count(s => s.GetType() == typeof (T)) == 1)
                {
                    var existing = (T) listUpdates.Single(s => s.GetType() == typeof (T));
                    lock (existing)
                    {
                        updateExisting(existing);
                        return false;
                    }
                }

                // No, so, go add it to the hashtable
                T newStatusUpdate = createNew(id);

                listUpdates.Add(newStatusUpdate);

                _incomingUpdates.Enqueue(newStatusUpdate);

                return true;
            }
        }

        public T GetById<T>(string id, T defaultIfNotFound) where T : class, IStatusUpdate
        {
            if (_statusesById.ContainsKey(id))
            {
                var listUpdates = (List<IStatusUpdate>) _statusesById[id];
                if (listUpdates.Count(s => s.GetType() == typeof (T)) == 1)
                {
                    var existing = (T) listUpdates.Single(s => s.GetType() == typeof (T));
                    return existing;
                }
            }
            return defaultIfNotFound;
        }

        public T GetById<T>(string id) where T : class, IStatusUpdate
        {
            return GetById<T>(id, null);
        }

        #endregion

        private void DispatchTimerTick(object sender, EventArgs e)
        {
            {
                lock (_lock)
                {
                    if (_incomingUpdates.Any())
                    {
                        var update = _incomingUpdates.Dequeue();
                        OutgoingUpdates.Add(update);
                    }
                }
            }
        }

        private bool SendStatusUpdateInternal(IStatusUpdate update)
        {
            IStatusUpdate u = update;
            bool y = UpdateOrCreateStatus(update.ID,
                                          id => u,
                                          existing =>
                                              {
                                                  u.Types.Where(t => !existing.Types.Contains(t))
                                                      .ForEach(type => existing.Types.Add(type));
                                                  u.Parents.Where(p => !existing.Parents.Contains(p))
                                                      .ForEach(parent => existing.Parents.Add(parent));
                                                  return existing;
                                              });

            return y;
        }
    }
}