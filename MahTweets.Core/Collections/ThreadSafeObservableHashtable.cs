//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Collections;
//using System.Collections.ObjectModel;
//using System.Windows.Threading;
//using System.Threading;

//namespace MahTweets2.Library.Collections
//{
//    /// <summary>
//    /// Is this class used anymore? Marked for depreciation
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    //public class ThreadSafeObservableHashtable<T> : Hashtable, INotifyCollectionChanged
//    //{
//    //    Dispatcher _dispatcher;
//    //    ReaderWriterLock _lock;
//    //    public event NotifyCollectionChangedEventHandler CollectionChanged;

//    //   // private delegate void InternalRaiseCollectionChangedEvent(NotifyCollectionChangedEventArgs e);
//    //    public ThreadSafeObservableHashtable()
//    //    {
//    //        _dispatcher = Dispatcher.CurrentDispatcher;
//    //        _lock = new ReaderWriterLock();
//    //    }

//    //    private void RaiseCollectionChangedEvent(NotifyCollectionChangedEventArgs e)
//    //    {
//    //        if (CollectionChanged == null)
//    //        {
//    //            return;
//    //        }
//    //        CollectionChanged(this, e);
//    //    }


//    //    public void Add(string key, T value)
//    //    {
//    //        if (_dispatcher.CheckAccess())
//    //        {
               
//    //            LockCookie c = _lock.UpgradeToWriterLock(-1);
//    //            if (base.ContainsKey(key))
//    //                return;

//    //            base.Add(key, value);
//    //            try
//    //            {
//    //                RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
//    //            }
//    //            catch (Exception ex)
//    //            {
//    //                MahTweets2.Library.BlackBoxRecorder.LogHandledException(ex);
//    //                RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
//    //            }
//    //            _lock.DowngradeFromWriterLock(ref c);

//    //        } else {
//    //            object[] e = new object[] { key, value };
//    //            _dispatcher.Invoke(DispatcherPriority.DataBind, (SendOrPostCallback)delegate { InsertItemImpl(e); }, e);
//    //        }
//    //    }

//    //    void InsertItemImpl(object[] e)
//    //    {
//    //        if (_dispatcher.CheckAccess())
//    //        {
//    //            Add((String)e[0], (T)e[1]);
//    //        }
//    //        else
//    //        {
//    //            _dispatcher.Invoke(DispatcherPriority.DataBind, (SendOrPostCallback)delegate { InsertItemImpl(e); });
//    //        }
//    //    }

//    //    public override void Remove(object key)
//    //    {
//    //        T t = this[key];
//    //        base.Remove(key);
//    //        RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, t));
//    //    }

//    //    public override void Clear()
//    //    {
//    //        base.Clear();
//    //        RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
//    //    }

//    //    public new T this[object key]
//    //    {
//    //        get
//    //        {
//    //            return ((T)base[key]);
//    //        }
//    //        set
//    //        {
//    //            if (base.ContainsKey(key))
//    //            {
//    //                base[key] = value;
//    //                RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value));
//    //            }
//    //            else
//    //            {
//    //                Add(key, value);
//    //                RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));

//    //            }
//    //        }
//    //    }


//    //}
//    }
