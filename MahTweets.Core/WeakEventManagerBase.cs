using System;
using System.Windows;

namespace MahTweets.Core
{
    public abstract class WeakEventManagerBase<T, TSource> : WeakEventManager
        where T : WeakEventManagerBase<T, TSource>, new()
        where TSource : class
    {
        public static T Current
        {
            get
            {
                Type managerType = typeof (T);
                var manager = GetCurrentManager(managerType) as T;
                if (manager == null)
                {
                    manager = new T();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        public static void AddListener(TSource source, IWeakEventListener listener)
        {
            Current.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(TSource source, IWeakEventListener listener)
        {
            Current.ProtectedRemoveListener(source, listener);
        }

        protected override sealed void StartListening(object source)
        {
            StartListeningTo(source as TSource);
        }

        protected abstract void StartListeningTo(TSource source);

        protected override sealed void StopListening(object source)
        {
            StopListeningTo(source as TSource);
        }

        protected abstract void StopListeningTo(TSource source);
    }
}