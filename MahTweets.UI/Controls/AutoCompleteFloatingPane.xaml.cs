using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.UI.Controls
{
    public partial class AutoCompleteFloatingPane
    {
        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register("FilterText",
                                                                                                   typeof (string),
                                                                                                   typeof (
                                                                                                       AutoCompleteFloatingPane
                                                                                                       ),
                                                                                                   new UIPropertyMetadata
                                                                                                       (FilterTextPropertyChanged));

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
                                                                                                 typeof (bool),
                                                                                                 typeof (
                                                                                                     AutoCompleteFloatingPane
                                                                                                     ));

        public static readonly DependencyProperty ContactsProperty = DependencyProperty.Register("Contacts",
                                                                                                 typeof (
                                                                                                     ObservableCollection
                                                                                                     <IContact>),
                                                                                                 typeof (
                                                                                                     AutoCompleteFloatingPane
                                                                                                     ));

        public AutoCompleteFloatingPane()
        {
            InitializeComponent();
            Loaded += AutoCompleteFloatingPane_Loaded;
        }

        public IContactsRepository Manager { get; set; }

        public string FilterText
        {
            get { return (string) GetValue(FilterTextProperty); }
            set
            {
                string newText = value.Replace("@", string.Empty);
                // Logging.Info("Filtering for '{0}'", newText);
                SetValue(FilterTextProperty, newText);
            }
        }

        public bool IsActive
        {
            get { return (bool) GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public ObservableCollection<IContact> Contacts
        {
            get { return (ObservableCollection<IContact>) GetValue(ContactsProperty); }
            set { SetValue(ContactsProperty, value); }
        }

        public event EventHandler ItemSelected;
        public event EventHandler NothingFound;

        private void AutoCompleteFloatingPane_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            Manager = CompositionManager.Get<IContactsRepository>();
        }

        private void CvsFilter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FilterText)) return;

            var i = e.Item as Contact;
            if (i == null) return;

            e.Accepted = i.Name.Contains(FilterText);
        }

        private static void FilterTextPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var pane = obj as AutoCompleteFloatingPane;
            if (pane == null) return;

            var filterText = args.NewValue as string;
            if (filterText == null || string.IsNullOrWhiteSpace(filterText)) return;

            filterText = filterText.ToLower();

            if (pane.Contacts == null)
                pane.Contacts = pane.Manager.ContactsList;

            IEnumerable<IContact> results =
                pane.Contacts.Where(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.ToLower().Contains(filterText));

            if (results.Any())
            {
                pane.lbFilteredList.ItemsSource = null;
                pane.lbFilteredList.ItemsSource = results.OrderBy(x => x.Name).Distinct();
                pane.lbFilteredList.SelectedIndex = 0;
            }
            else
            {
                if (pane.NothingFound != null)
                    pane.NothingFound(null, null);
                pane.lbFilteredList.ItemsSource = pane.Contacts;
            }

            pane.UpdateSource();
        }

        private void UpdateSource()
        {
            var cvs = FindResource("cvsContacts") as CollectionViewSource;
            if (cvs == null) return;

            cvs.Source = Manager.ContactsList;
        }

        private void grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemSelected != null)
                ItemSelected(((Grid) sender).DataContext, null);
        }

        public void MoveDown()
        {
            if (!IsActive) return;

            try
            {
                lbFilteredList.SelectedIndex++;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }

            lbFilteredList.ScrollIntoView(lbFilteredList.SelectedItem);
        }

        public void MoveUp()
        {
            if (!IsActive) return;

            try
            {
                lbFilteredList.SelectedIndex--;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            lbFilteredList.ScrollIntoView(lbFilteredList.SelectedItem);
        }

        public Contact SelectedContact()
        {
            return (Contact) lbFilteredList.SelectedItem;
        }
    }
}