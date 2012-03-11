using System.Collections.Generic;
using MahTweets.Core.Collections;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.ViewModels
{
    public class ConversationViewModel : ContainerViewModel
    {
        private readonly List<IContact> _contactsListDefinitive = new List<IContact>();
        private readonly object _lockObject = new object();
        private ThreadSafeObservableCollection<IContact> _contacts;

        private IMicroblog _source;
        private ThreadSafeObservableCollection<IStatusUpdate> _updates;

        public ConversationViewModel()
        {
            Contacts = new ThreadSafeObservableCollection<IContact>();
            Updates = new ThreadSafeObservableCollection<IStatusUpdate>();
        }

        public IMicroblog Source
        {
            get { return _source; }
            set
            {
                _source = value;
                RaisePropertyChanged(() => Source);
            }
        }

        public ThreadSafeObservableCollection<IContact> Contacts
        {
            get { return _contacts; }
            set
            {
                _contacts = value;
                RaisePropertyChanged(() => Contacts);
            }
        }

        public ThreadSafeObservableCollection<IStatusUpdate> Updates
        {
            get { return _updates; }
            set
            {
                _updates = value;
                RaisePropertyChanged(() => Updates);
            }
        }

        public void LockAddContact(IContact contact)
        {
            lock (_lockObject)
            {
                if (!_contactsListDefinitive.Contains(contact))
                {
                    _contactsListDefinitive.Add(contact);
                    Contacts.Add(contact);
                }
            }
        }

        public override void Close()
        {
            var container = Parent as ContainerViewModel;
            if (container == null) return;
            container.RemoveContainer(this);
        }
    }
}