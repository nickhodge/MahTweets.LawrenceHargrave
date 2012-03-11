using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using MahTweets.Core.Extensions;

namespace MahTweets.Core.Interfaces
{
    /// <summary>
    /// Reponsible of notifying property changes
    /// </summary>
    /// <remarks>
    /// It does it efficiently caching the event args used for all the classes
    /// It also includes some hacks for a typical timing issue in Silverlight
    /// </remarks>
    [DataContract]
    public abstract class Notify : INotifyPropertyChanged
    {
        private static readonly Dictionary<string, PropertyChangedEventArgs> EventArgsMap =
            new Dictionary<string, PropertyChangedEventArgs>();

        private PropertyChangedEventHandler _propChangedHandler;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propChangedHandler = (PropertyChangedEventHandler) Delegate.Combine(_propChangedHandler, value); }
            remove
            {
                if (_propChangedHandler != null)
                    _propChangedHandler = (PropertyChangedEventHandler) Delegate.Remove(_propChangedHandler, value);
            }
        }

        #endregion

        #region Notifications

        private static PropertyChangedEventArgs GetEventArgs(string propertyName)
        {
            PropertyChangedEventArgs pe;
            if (EventArgsMap.TryGetValue(propertyName, out pe) == false)
            {
                pe = new PropertyChangedEventArgs(propertyName);
                EventArgsMap[propertyName] = pe;
            }
            return pe;
        }

        protected void RaisePropertyChanged(params Expression<Func<object>>[] expressions)
        {
            if (_propChangedHandler == null)
                // The check is duplicated, but we want to return here for performance reasons
                return;
            if (!expressions.HasItems())
                throw new ArgumentOutOfRangeException("expressions", "You need to provide at least one expression");

            ReflectionExtensions.GetPropertyNames(expressions)
                .ForEach(OnPropertyChanged);
        }

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            if (_propChangedHandler == null)
                // The check is duplicated, but we want to return here for performance reasons
                return;
            if (!propertyNames.HasItems())
                throw new ArgumentException("propertyNames");

            Array.ForEach(propertyNames, OnPropertyChanged);
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            OnPropertyChanged(propertyName);
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (_propChangedHandler == null)
                return;
            try
            {
                _propChangedHandler(this, GetEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Failed raising the property change event {0}.{1}", GetType().FullName,
                                              propertyName));
                Debug.WriteLine(ex.ToString());
            }
        }

        #endregion
    }
}