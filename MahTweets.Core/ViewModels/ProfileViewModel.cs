using System.Windows;
using MahTweets.Core.Collections;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.ViewModels
{
    public abstract class ProfileViewModel : ContainerViewModel
    {
        private IContact _contact;
        private ThreadSafeObservableCollection<IStatusUpdate> _contactUpdates;
        private UIElement _profileDetails;
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

        public IContact Contact
        {
            get { return _contact; }
            set
            {
                _contact = value;
                RaisePropertyChanged(() => Contact);
            }
        }

        public ThreadSafeObservableCollection<IStatusUpdate> ContactUpdates
        {
            get { return _contactUpdates; }
            set
            {
                _contactUpdates = value;
                RaisePropertyChanged(() => ContactUpdates);
            }
        }

        public UIElement ProfileDetails
        {
            get { return _profileDetails; }
            set
            {
                _profileDetails = value;
                RaisePropertyChanged(() => ProfileDetails);
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