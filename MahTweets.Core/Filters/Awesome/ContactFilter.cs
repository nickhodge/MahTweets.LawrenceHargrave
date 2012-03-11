using System.Collections.ObjectModel;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Filters.Awesome
{
    public class ContactFilter : Notify, IFilter
    {
        private readonly ContactComparer comparer;

        private string _contactName;

        public ContactFilter()
        {
            Include = true;
            comparer = new ContactComparer();
            Contacts = new ObservableCollection<IContact>();
        }

        public string ContactName
        {
            get { return _contactName; }
            set { _contactName = value.Trim(); }
        }

        public ObservableCollection<IContact> Contacts { get; set; }

        #region IFilter Members

        public bool Include { get; set; }

        public bool? Filter(IStatusUpdate update)
        {
            if (update.Contact == null)
                return null;

            foreach (IContact contact in Contacts)
            {
                if (comparer.Compare(contact, update.Contact) == 0)
                    return Include;
            }

            string contactName = update.Contact.Name.Trim();

            if (contactName.Matches(ContactName, true))
                return Include;

            return null;
        }

        #endregion
    }
}