using System.Windows;
using System.Windows.Data;
using MahTweets.ViewModels.Setup;

namespace MahTweets.Views.Setup
{
    public partial class AccountsView
    {
        public AccountsView()
        {
            InitializeComponent();
            Loaded += AccountsView_Loaded;
        }

        private void AccountsView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (AccountsViewModel) DataContext;
            var cvsAvailable = Resources["cvsAvailableAccounts"] as CollectionViewSource;
            var c = new CompositeCollection();
            c.Add(new CollectionContainer {Collection = vm.NewMicroblogs});
            cvsAvailable.Source = c;

            var cvsCurrent = Resources["cvsCurrentAccounts"] as CollectionViewSource;
            c = new CompositeCollection();
            c.Add(new CollectionContainer {Collection = vm.PluginRepository.Microblogs});
            cvsCurrent.Source = c;
        }
    }
}