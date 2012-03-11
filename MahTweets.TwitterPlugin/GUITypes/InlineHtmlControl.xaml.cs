using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MahTweets2.TwitterPlugin.GUITypes
{
    
    public partial class InlineHtmlControl : UserControl
    {
        BitmapImage bi = new BitmapImage();
        public InlineHtmlControl()
        {
            InitializeComponent();
            this.Unloaded += new RoutedEventHandler(InlineHtmlControl_Unloaded);
        }

        void InlineHtmlControl_Unloaded(object sender, RoutedEventArgs e)
        {
            image = null;
            bi = null;
        }
        private Uri source;

        public Uri Source
        {
            get { return source; }
            set { 
                source = value;

                
                bi.BeginInit();
                bi.UriSource = source;
                bi.EndInit();

                image.Source = bi;
                image.Stretch = System.Windows.Media.Stretch.Uniform;
                txtLink.Text = source.ToString();
            }
        }
        private Uri originalPage;
        public Uri OriginalPage
        {
            get { return originalPage; }
            set { originalPage = value; }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                InlineUIContainer iuc = ((InlineUIContainer)this.Parent);
                ((Paragraph)iuc.Parent).Inlines.Remove(iuc);
            }
            catch { 
            
            }
        }

        private void internal_btnVisit_Click(object sender, RoutedEventArgs e)
        {
            //image.Visibility = Visibility.Collapsed;
            //browser.Source = originalPage.ToString();
            //browser.Visibility = Visibility.Visible;
            System.Diagnostics.Process.Start(originalPage.ToString());
        }
    }
}
