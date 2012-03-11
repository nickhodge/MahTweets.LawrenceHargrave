using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MahTweets.UI.Controls
{
    public partial class AccountSelector
    {
        public AccountSelector()
        {
            InitializeComponent();
            Popup.IsOpen = false;
        }

        public DataTemplate SelectedItemTemplate
        {
            get { return CurrentAccount.ContentTemplate; }
            set { CurrentAccount.ContentTemplate = value; }
        }


        public DataTemplate ItemTemplate
        {
            get { return AvailableAccounts.ItemTemplate; }
            set { AvailableAccounts.ItemTemplate = value; }
        }

        public object SelectedItem
        {
            get { return AvailableAccounts.SelectedItem; }
            set
            {
                AvailableAccounts.SelectedItem = value;
                CurrentAccount.Content = value;
            }
        }

        public SelectionMode SelectionMode
        {
            get { return AvailableAccounts.SelectionMode; }
            set { AvailableAccounts.SelectionMode = value; }
        }

        public IEnumerable ItemsSource
        {
            get { return AvailableAccounts.ItemsSource; }
            set { AvailableAccounts.ItemsSource = value; }
        }

        public IList SelectedItems
        {
            get { return AvailableAccounts.SelectedItems; }
        }

        private void CurrentAccountClick(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = !Popup.IsOpen;
        }

        private void AvailableAccountsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionMode == SelectionMode.Single)
            {
                Popup.IsOpen = false;
                CurrentAccount.Content = e.AddedItems[0];
            }
        }
    }
}