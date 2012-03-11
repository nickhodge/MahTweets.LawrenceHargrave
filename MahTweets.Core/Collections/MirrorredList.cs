using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Windows;
using MahTweets.Core.Interfaces;

namespace MahTweets.Core.Collections
{
    // http://lambert.geek.nz/2007/10/30/wpf-multithreaded-collections/
    public class MirroredList<T> : Notify, IList<T>, IList, INotifyCollectionChanged, IWeakEventListener
    {
        private readonly IList<T> _BaseList;

        private readonly Queue<NotifyCollectionChangedEventArgs> _Changes =
            new Queue<NotifyCollectionChangedEventArgs>();

        private readonly object _ChangesLock = new object();
        private readonly object _MirrorLock = new object();

        private readonly List<NotifyCollectionChangedEventArgs> _PausedChanges =
            new List<NotifyCollectionChangedEventArgs>();

        private readonly object pauseLock = new object();
        private List<T> _MirrorList;
        private bool _isPaused;

        public MirroredList(IList<T> baseList)
        {
            if (baseList == null)
                throw new ArgumentNullException("baseList");

            _BaseList = baseList;
            var collection = _BaseList as ICollection;
            var changeable = _BaseList as INotifyCollectionChanged;
            if (changeable == null)
                throw new ArgumentException("List must support INotifyCollectionChanged", "baseList");

            if (collection != null)
            {
                Monitor.Enter(collection.SyncRoot);
            }
            try
            {
                _MirrorList = new List<T>(_BaseList);
                CollectionChangedEventManager.AddListener(changeable, this);
            }
            finally
            {
                if (collection != null)
                {
                    Monitor.Exit(collection.SyncRoot);
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_MirrorLock)
                {
                    return _MirrorList[index];
                }
            }
        }

        #region IList Members

        public object SyncRoot
        {
            get { return _MirrorLock; }
        }

        int IList.Add(object value)
        {
            ThrowReadOnly();
            return -1; // never reaches here
        }

        void IList.Clear()
        {
            ThrowReadOnly();
        }

        bool IList.Contains(object value)
        {
            lock (_MirrorLock)
            {
                return ((IList) _MirrorList).Contains(value);
            }
        }

        int IList.IndexOf(object value)
        {
            lock (_MirrorLock)
            {
                return ((IList) _MirrorList).IndexOf(value);
            }
        }

        void IList.Insert(int index, object value)
        {
            ThrowReadOnly();
        }

        bool IList.IsFixedSize
        {
            get { return ((IList) _MirrorList).IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        void IList.Remove(object value)
        {
            ThrowReadOnly();
        }

        void IList.RemoveAt(int index)
        {
            ThrowReadOnly();
        }

        object IList.this[int index]
        {
            get
            {
                lock (_MirrorLock)
                {
                    return ((IList) _MirrorList)[index];
                }
            }
            set { ThrowReadOnly(); }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            lock (_MirrorLock)
            {
                ((IList) _MirrorList).CopyTo(array, index);
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            lock (_MirrorLock)
            {
                return _MirrorList.IndexOf(item);
            }
        }

        public bool Contains(T item)
        {
            lock (_MirrorLock)
            {
                return _MirrorList.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_MirrorLock)
            {
                _MirrorList.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                lock (_MirrorLock)
                {
                    return _MirrorList.Count;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_MirrorLock)
            {
                foreach (T item in _MirrorList)
                {
                    yield return item;
                }
            }
        }

        T IList<T>.this[int index]
        {
            get { return this[index]; }
            set { ThrowReadOnly(); }
        }

        void IList<T>.Insert(int index, T item)
        {
            ThrowReadOnly();
        }

        void IList<T>.RemoveAt(int index)
        {
            ThrowReadOnly();
        }

        void ICollection<T>.Add(T item)
        {
            ThrowReadOnly();
        }

        public void Clear()
        {
            lock (_ChangesLock)
            {
                _MirrorList.Clear();
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove(T item)
        {
            ThrowReadOnly();
            return false; // never reaches here
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region IWeakEventListener Members

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof (CollectionChangedEventManager) && sender == _BaseList)
            {
                RecordChange((NotifyCollectionChangedEventArgs) e);
                return true;
            }
            return false;
        }

        #endregion

        private void RecordChange(NotifyCollectionChangedEventArgs changeInfo)
        {
            bool isFirstChange;
            lock (_ChangesLock)
            {
                isFirstChange = (_Changes.Count == 0);
                _Changes.Enqueue(changeInfo);
            }
            if (isFirstChange)
            {
                OnCollectionDirty();
            }
        }

        protected virtual void OnCollectionDirty()
        {
            // This is virtual so that derived classes can eg. redirect
            // this to a different thread...
            ProcessChanges();
        }

        protected void ProcessChanges()
        {
            bool locked = false;
            Monitor.Enter(_ChangesLock);
            try
            {
                locked = true;
                while (_Changes.Count > 0)
                {
                    NotifyCollectionChangedEventArgs info = _Changes.Dequeue();
                    Monitor.Exit(_ChangesLock);
                    locked = false;

                    // ProcessChange occurs outside the ChangesLock,
                    // permitting other threads to queue things up behind us.
                    // NB that this means that if your change producer is
                    // running faster than your change consumer, this
                    // method may never exit.  But it does avoid making the
                    // producer wait for the consumer to process.
                    ProcessChange(info);

                    Monitor.Enter(_ChangesLock);
                    locked = true;
                }
            }
            finally
            {
                if (locked)
                {
                    Monitor.Exit(_ChangesLock);
                }
            }
        }

        public void Pause()
        {
            lock (pauseLock)
            {
                _isPaused = true;
            }
        }

        public void Resume()
        {
            lock (pauseLock)
            {
                _isPaused = false;
                foreach (NotifyCollectionChangedEventArgs p in _PausedChanges)
                    ProcessChange(p);

                _PausedChanges.Clear();
            }
        }


        private void ProcessChange(NotifyCollectionChangedEventArgs
                                       info)
        {
            if (_isPaused)
            {
                _PausedChanges.Add(info);
                return;
            }

            lock (_MirrorLock)
            {
                bool changedCount = true;
                switch (info.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (info.OldItems != null)
                            throw new ArgumentException("Old items present in Add?!", "info");
                        if (info.NewItems == null)
                            throw new ArgumentException("New items not present in Add?!", "info");

                        for (int itemIndex = 0; itemIndex < info.NewItems.Count; ++itemIndex)
                        {
                            _MirrorList.Insert(info.NewStartingIndex +
                                               itemIndex, (T) info.NewItems[itemIndex]);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (info.OldItems == null)
                            throw new ArgumentException("Old items not present in Remove?!", "info");

                        if (info.NewItems != null)
                            throw new ArgumentException("New items present in Remove?!", "info");

                        for (int itemIndex = 0; itemIndex < info.OldItems.Count; ++itemIndex)
                        {
                            _MirrorList.RemoveAt(info.OldStartingIndex);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        if (info.NewItems == null)
                            throw new ArgumentException("New items not present in Move?!", "info");
                        if (info.NewItems.Count != 1)
                            throw new NotSupportedException("Move operations only supported for one item at a time.");

                        _MirrorList.RemoveAt(info.OldStartingIndex);
                        _MirrorList.Insert(info.NewStartingIndex, (T) info.NewItems[0]);
                        changedCount = false;
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        if (info.OldItems == null)
                            throw new ArgumentException("Old items not present in Replace?!", "info");
                        if (info.NewItems == null)
                            throw new ArgumentException("New items not present in Replace?!", "info");

                        for (int itemIndex = 0; itemIndex < info.NewItems.Count; ++itemIndex)
                        {
                            _MirrorList[info.NewStartingIndex + itemIndex]
                                = (T) info.NewItems[itemIndex];
                        }
                        changedCount = false;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        var collection = _BaseList as ICollection;
                        if (collection != null)
                        {
                            Monitor.Enter(collection.SyncRoot);
                        }
                        try
                        {
                            lock (_ChangesLock)
                            {
                                _MirrorList = new List<T>(_BaseList);
                                _Changes.Clear();
                            }
                        }
                        finally
                        {
                            if (collection != null)
                            {
                                Monitor.Exit(collection.SyncRoot);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentException("Unrecognised collection "
                                                    + "change operation.", "info");
                }

                OnCollectionChanged(info);
                OnPropertyChanged("Items[]");
                if (changedCount)
                {
                    OnPropertyChanged("Count");
                }
            }
        }

        public void CopyTo(T[] array)
        {
            lock (_MirrorLock)
            {
                _MirrorList.CopyTo(array);
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }


        private void ThrowReadOnly()
        {
            throw new NotSupportedException("Collection is read-only.");
        }
    }
}