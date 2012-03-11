using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.ViewModels;

namespace MahTweets.Core.ViewModels
{
    [DataContract]
    public class BaseViewModel : Notify, IBaseViewModel
    {
        public BaseViewModel()
        {
            if (IsInDesignMode)
                InitializeDesignData();
            else
            {
                EventAggregator = CompositionManager.Get<IEventAggregator>();
            }
        }

        public BaseViewModel(IEventAggregator eventAggregator)
        {
            if (IsInDesignMode)
                InitializeDesignData();
            else
            {
                EventAggregator = eventAggregator;
            }
        }

        // not a huge fan of these, but needed to link parts together currently

        [IgnoreDataMember]
        public BaseViewModel Parent { get; set; }

        [IgnoreDataMember]
        public IEventAggregator EventAggregator { get; set; }

        #region DesignTime

        private static bool? _isInDesignMode;

        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running in Blend
        /// or Visual Studio).
        /// </summary>
        [IgnoreDataMember]
        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool) DependencyPropertyDescriptor
                                     .FromProperty(prop, typeof (FrameworkElement))
                                     .Metadata.DefaultValue;
                }
                if (_isInDesignMode != null)
                {
                    // ReSharper disable PossibleInvalidOperationException
                    return _isInDesignMode.Value;
                    // ReSharper restore PossibleInvalidOperationException
                }

                return false;
            }
        }

        [IgnoreDataMember]
        public bool IsInDesignMode
        {
            get { return IsInDesignModeStatic; }
        }

        [IgnoreDataMember]
        public static bool IsMockingStatic
        {
            get { return false; }
        }

        [IgnoreDataMember]
        public bool IsMocking
        {
            get { return IsMockingStatic; }
        }

        public virtual void InitializeDesignData()
        {
        }

        #endregion

        #region Service Calls

        private bool _isInitializing;
        private bool _isLoading;

        [IgnoreDataMember]
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
        }

        [IgnoreDataMember]
        public bool IsInitializing
        {
            get { return _isInitializing; }
            set
            {
                _isInitializing = value;
                RaisePropertyChanged(() => IsInitializing);
            }
        }

        //public AsyncCallback CreateCallback(Action<IAsyncResult> endMethodCall, Action onSuccess = null)
        //{
        //    Func<IAsyncResult, object> fakeEndMethod = ar => { endMethodCall(ar); return null; };

        //    Action<object> fakeOnSuccess = null;
        //    if (onSuccess != null)
        //        fakeOnSuccess = o => onSuccess();

        //    return CreateCallback(fakeEndMethod, fakeOnSuccess);
        //}

        //public AsyncCallback CreateCallback<T>(Func<IAsyncResult, T> endMethodCall, Action<T> onSuccess = null, Action<Exception> onError = null)
        //{
        //    IsLoading = true;
        //    return ar =>
        //    {
        //        try
        //        {
        //            var result = endMethodCall(ar);
        //            if (onSuccess != null)
        //                onSuccess(result);
        //            //if (onSuccess != null)
        //            //    DispatcherExtensions.Dispatch(() => onSuccess(result));
        //        }
        //        catch (Exception ex)
        //        {
        //            if (onError != null)
        //                onError(ex);
        //        }
        //        finally
        //        {
        //            IsLoading = false;
        //        }
        //    };
        //}

        #endregion

        #region IBaseViewModel Members

        [IgnoreDataMember]
        public FrameworkElement View { get; set; }

        #endregion

        public virtual void Initialize()
        {
        }

        public virtual void Cleanup()
        {
        }
    }
}