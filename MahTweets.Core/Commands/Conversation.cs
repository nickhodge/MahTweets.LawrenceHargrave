using System.Collections.ObjectModel;
using MahTweets.Core.Collections;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Plugins;
using System.Collections.Generic;
using System;

namespace MahTweets.Core.Commands
{
    public class Conversation : Observable
    {
        private IMicroblog _source;
        public IMicroblog Source
        {
            get { return _source; }
            set
            {
                _source = value;
                RaisePropertyChanged(() => Source);
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private object _lockObject = new Object();
        public void LockAddContact(Contact contact)
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

        private List<Contact> _contactsListDefinitive = new List<Contact>(); 


        private ObservableCollection<Contact> _contacts;
        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set
            {
                _contacts = value;
                RaisePropertyChanged(() => Contacts);
            }
        }

        private ThreadSafeObservableCollection<IStatusUpdate> _updates;
        public ThreadSafeObservableCollection<IStatusUpdate> Updates
        {
            get { return _updates; }
            set
            {
                _updates = value;
                RaisePropertyChanged(() => Updates);
            }
        }

        public Conversation()
        {
            Contacts = new ThreadSafeObservableCollection<Contact>();
            Updates = new ThreadSafeObservableCollection<IStatusUpdate>();
        }

        public virtual void LoadConversation()
        {
            // do nothing by default
        }
    }
}