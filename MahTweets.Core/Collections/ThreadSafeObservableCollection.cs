using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MahTweets.Core.Global;

// http://khason.net/blog/how-to-disconnect-ui-and-data-in-wpf-cachedobservablecollection-and-some-updates-regarding-threadsafeobservablecollection/

namespace MahTweets.Core.Collections
{
    /// <summary>
    /// Observable collection with support for multi-threaded calls
    /// </summary>
    /// <typeparam name="T">Type of collection</typeparam>
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        private readonly ReaderWriterLock _lock;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ThreadSafeObservableCollection()
        {
            _lock = new ReaderWriterLock();
        }

        public ThreadSafeObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
            _lock = new ReaderWriterLock();
        }

        /// <summary>
        /// Clear all items from collection
        /// </summary>
        protected override void ClearItems()
        {
            if (GlobalDispatcher.Dispatcher.CheckAccess())
            {
                LockCookie c = _lock.UpgradeToWriterLock(-1);
                base.ClearItems();
                _lock.DowngradeFromWriterLock(ref c);
            }
            else
            {
                Task.Run(() => Clear());
            }
        }

        /// <summary>
        /// Insert item at specific index
        /// </summary>
        /// <param name="index">Index to insert item</param>
        /// <param name="item">Item to insert</param>
        protected override void InsertItem(int index, T item)
        {
            if (GlobalDispatcher.Dispatcher.CheckAccess())
            {
                if (index > Count)
                    return;
                LockCookie c = _lock.UpgradeToWriterLock(-1);
                base.InsertItem(index, item);
                _lock.DowngradeFromWriterLock(ref c);
            }
            else
            {
                var e = new object[] {index, item};
                GlobalDispatcher.Dispatcher.BeginInvoke(DispatcherPriority.DataBind,
                                                        (SendOrPostCallback) delegate { InsertItem(index, item); }, e);
            }
        }

        /// <summary>
        /// Add an item to the end of the collection
        /// </summary>
        /// <param name="item">Item to add</param>
        public new void Add(T item)
        {
            InsertItem(0, item);
        }

        /// <summary>
        /// Clear all items from the collection
        /// </summary>
        public new void Clear()
        {
            ClearItems();
        }

        /// <summary>
        /// Insert an item at a specific position
        /// </summary>
        /// <param name="index">Position in list</param>
        /// <param name="item">Item to insert</param>
        public new void Insert(int index, T item)
        {
            InsertItem(index, item);
        }

        /// <summary>
        /// Move item to new location
        /// </summary>
        /// <param name="oldIndex">Index of item</param>
        /// <param name="newIndex">Destination index</param>
        public new void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        /// Remove item from collection
        /// </summary>
        /// <param name="item">Item to remove</param>
        public new void Remove(T item)
        {
            if (GlobalDispatcher.Dispatcher.CheckAccess())
            {
                int idx = IndexOf(item);
                RemoveItem(idx);
            }
            else
            {
                GlobalDispatcher.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                   (SendOrPostCallback) delegate { Remove(item); }, item);
            }
        }

        /// <summary>
        /// Remove item at index
        /// </summary>
        /// <param name="index">Index to remove</param>
        public new void RemoveAt(int index)
        {
            RemoveItem(index);
        }


        /// <summary>
        /// Move item to new location
        /// </summary>
        /// <param name="oldIndex">Index of item</param>
        /// <param name="newIndex">Destination index</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (GlobalDispatcher.Dispatcher.CheckAccess())
            {
                if (oldIndex >= Count | newIndex >= Count | oldIndex == newIndex)
                    return;
                LockCookie c = _lock.UpgradeToWriterLock(-1);
                base.MoveItem(oldIndex, newIndex);
                _lock.DowngradeFromWriterLock(ref c);
            }
            else
            {
                var e = new object[] {oldIndex, newIndex};
                GlobalDispatcher.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                   (SendOrPostCallback) delegate { MoveItemImpl(e); }, e);
            }
        }

        private void MoveItemImpl(object[] e)
        {
            if (GlobalDispatcher.Dispatcher.CheckAccess())
            {
                MoveItem((int) e[0], (int) e[1]);
            }
            else
            {
                GlobalDispatcher.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                   (SendOrPostCallback) delegate { MoveItemImpl(e); });
            }
        }

        /// <summary>
        /// Remove item at index
        /// </summary>
        /// <param name="index">Index to remove</param>
        protected override void RemoveItem(int index)
        {
            if (GlobalDispatcher.Dispatcher.CheckAccess())
            {
                if (index >= Count)
                    return;
                LockCookie c = _lock.UpgradeToWriterLock(-1);
                base.RemoveItem(index);
                _lock.DowngradeFromWriterLock(ref c);
            }
            else
            {
                GlobalDispatcher.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                   (SendOrPostCallback) delegate { RemoveItem(index); }, index);
            }
        }

        /// <summary>
        /// Set item at index
        /// </summary>
        /// <param name="index">Index to set</param>
        /// <param name="item">Item to set</param>
        protected override void SetItem(int index, T item)
        {
            if (GlobalDispatcher.Dispatcher.CheckAccess())
            {
                LockCookie c = _lock.UpgradeToWriterLock(-1);
                base.SetItem(index, item);
                _lock.DowngradeFromWriterLock(ref c);
            }
            else
            {
                var e = new object[] {index, item};
                GlobalDispatcher.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                   (SendOrPostCallback) delegate { SetItemImpl(e); }, e);
            }
        }

        private void SetItemImpl(object[] e)
        {
            if (GlobalDispatcher.Dispatcher.CheckAccess())
            {
                SetItem((int) e[0], (T) e[1]);
            }
            else
            {
                GlobalDispatcher.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                   (SendOrPostCallback) delegate { SetItemImpl(e); });
            }
        }

        /// <summary>
        /// Convert collection to array
        /// </summary>
        /// <returns>Array of items</returns>
        public T[] ToSyncArray()
        {
            _lock.AcquireReaderLock(-1);
            var _sync = new T[Count];
            CopyTo(_sync, 0);
            _lock.ReleaseReaderLock();
            return _sync;
        }
    }
}