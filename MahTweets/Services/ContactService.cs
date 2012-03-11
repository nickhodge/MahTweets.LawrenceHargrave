using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MahTweets.Core;
using MahTweets.Core.Collections;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Services
{
    /// <summary>
    /// Contact manager creates a cache of Contact for all plugins to use
    /// </summary>
    public class ContactService : IContactService
    {
        private static int _contactsLockWaiting;
        private static readonly Mutex ContactsMutex = new Mutex();

        /// <summary>
        /// Hashtable is the authorative store for Contacts, UI thread should get updated as queue items process
        /// </summary>
        private static readonly Hashtable contacts_by_name = new Hashtable();

        /// <summary>
        /// Collection of active contacts
        /// </summary>
        public ThreadSafeObservableCollection<IContact> Contacts = new ThreadSafeObservableCollection<IContact>();

        #region IContactService Members

        public ObservableCollection<IContact> ContactsList
        {
            get { return new ObservableCollection<IContact>(Contacts.OrderBy(contact => contact.Name)); }

            set
            {
                foreach (IContact c in value)
                {
                    Contacts.Add(c);
                }
            }
        }

        /// <summary>
        /// Get or create a new contact item from collection
        /// </summary>
        /// <param name="name">Name of contact</param>
        /// <param name="source">Microblog where the contact came from</param>
        /// <returns>Contact object</returns>
        public Contact GetOrCreate(string name, IMicroblogSource source)
        {
            if (String.IsNullOrEmpty(name))
                return null;

            Func<IContact, bool> filter = contact =>
                                          String.Compare(contact.Name, name, StringComparison.OrdinalIgnoreCase) == 0
                                          &&
                                          contact.Source == source;


            // Try and return early 
            if (contacts_by_name.ContainsKey(name))
            {
                var potential_matches = (ContactHashKey) contacts_by_name[name];

                if (potential_matches.Where(filter).Count() == 1)
                {
                    return potential_matches.Single(filter) as Contact;
                }
            }


            // If we didn't find it, we need to lock and add the contact. 
            bool mutexLocked = false;
            try
            {
                //Logging.Important("Wating for Lock on Contacts with {0} others", _contactsLockWaiting);
                Interlocked.Increment(ref _contactsLockWaiting);

                ContactsMutex.WaitOne();
                mutexLocked = true;
                Interlocked.Decrement(ref _contactsLockWaiting);
                //Logging.Important("Acquired Lock. {0} waiting", _contactsLockWaiting);


                // Check that it wasn't added before we got the lock 
                ContactHashKey potential_matches = null;
                if (contacts_by_name.ContainsKey(name))
                {
                    potential_matches = (ContactHashKey) contacts_by_name[name];

                    if (potential_matches.Where(filter).Count() == 1)
                    {
                        return potential_matches.Single(filter) as Contact;
                    }
                }

                // Wasn't added, so create it
                var contact = new Contact {Name = name, Source = source};

                // Add the hashkey 
                if (potential_matches == null)
                {
                    potential_matches = new ContactHashKey();
                    contacts_by_name.Add(name, potential_matches);
                }

                // Add to the hashkey 
                potential_matches.Add(contact);

                // Fire a background worker to add it to the Observable collection. 
                //_taskService.Enqueue(() => Contacts.Add(contact));
                Task.Run(() => Contacts.Add(contact));

                return contact;
            }
            finally
            {
                if (mutexLocked)
                {
                    ContactsMutex.ReleaseMutex();
                }
            }
        }

        public T GetOrCreate<T>(string name, IMicroblogSource source) where T : IContact, new()
        {
            if (string.IsNullOrEmpty(name))
                return default(T);

            Func<T, bool> filter = contact => contact.Name.Matches(name, true) && contact.Source == source;

            // Try and return early 
            if (contacts_by_name.ContainsKey(name))
            {
                var potentialMatches = (ContactHashKey) contacts_by_name[name];
                IEnumerable<T> matchingTypes = potentialMatches.OfType<T>();

                if (matchingTypes.Where(filter).Count() == 1)
                {
                    return matchingTypes.Single(filter);
                }
            }


            // If we didn't find it, we need to lock and add the contact. 
            bool mutexLocked = false;
            try
            {
                //Logging.Important("Wating for Lock on Contacts with {0} others", _contactsLockWaiting);
                Interlocked.Increment(ref _contactsLockWaiting);

                ContactsMutex.WaitOne();
                mutexLocked = true;
                Interlocked.Decrement(ref _contactsLockWaiting);
                //Logging.Important("Acquired Lock. {0} waiting", _contactsLockWaiting);


                // Check that it wasn't added before we got the lock 
                ContactHashKey potential_matches = null;
                if (contacts_by_name.ContainsKey(name))
                {
                    potential_matches = (ContactHashKey) contacts_by_name[name];

                    IEnumerable<T> matchingTypes = potential_matches.OfType<T>();

                    if (matchingTypes.Where(filter).Count() == 1)
                    {
                        return matchingTypes.Single(filter);
                    }
                }

                // Wasn't added, so create it
                var contact = new T {Name = name, Source = source};

                // Add the hashkey 
                if (potential_matches == null)
                {
                    potential_matches = new ContactHashKey();
                    contacts_by_name.Add(name, potential_matches);
                }

                // Add to the hashkey 
                potential_matches.Add(contact);

                // Fire a background worker to add it to the Observable collection. 
                // _taskService.Enqueue(() => Contacts.Add(contact));
                Task.Run(() => Contacts.Add(contact));

                return contact;
            }
            finally
            {
                if (mutexLocked)
                {
                    ContactsMutex.ReleaseMutex();
                }
            }
        }

        #endregion

        #region Nested type: ContactHashKey

        private class ContactHashKey : List<IContact>
        {
        }

        #endregion
    }
}