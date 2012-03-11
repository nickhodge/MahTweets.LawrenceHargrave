using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

namespace MahTweets.Core.Collections
{
    // http://lambert.geek.nz/2007/10/30/wpf-multithreaded-collections/
    public class BindableList<T> : MirroredList<T>
    {
        private readonly Dispatcher _Dispatcher;

        public BindableList(IList<T> baseList)
            : this(baseList, null)
        {
        }

        public BindableList(IList<T> baseList, Dispatcher dispatcher)
            : base(baseList)
        {
            _Dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;
        }

        protected override void OnCollectionDirty()
        {
            _Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(ProcessChanges));
        }
    }
}