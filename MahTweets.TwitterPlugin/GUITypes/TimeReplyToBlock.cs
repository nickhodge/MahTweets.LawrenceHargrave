using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MahTweets2.Library;
using MahTweets2.Library.Gui.Controls;

namespace MahTweets2.TwitterPlugin.GUITypes
{
    public class InReplyToBlock : TextBlock
    {
        public string ReplyID
        {
            get { return (string)GetValue(ReplyIDProperty); }
            set { SetValue(ReplyIDProperty, value); }
        }

        public string ReplyTo
        {
            get { return (string)GetValue(ReplyToProperty); }
            set { SetValue(ReplyToProperty, value); }
        }

        public static readonly DependencyProperty ReplyIDProperty =
            DependencyProperty.Register("ReplyID", typeof(string), typeof(InReplyToBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSourceChanged)));

        public static readonly DependencyProperty ReplyToProperty =
            DependencyProperty.Register("ReplyTo", typeof(string), typeof(InReplyToBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSourceChanged)));

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                InReplyToBlock textblock = (InReplyToBlock)obj;
                textblock.Inlines.Clear();

                if (textblock.ReplyID != "0" && textblock.ReplyID != string.Empty && textblock.ReplyID != null)
                {
                    InlineLink link = new InlineLink();
                    link.Inlines.Add(" in reply to " + textblock.ReplyTo);
                    link.TextDecorations = null;
                    link.ToolTip = "Load conversation";
                    link.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(conversationLink_Click);
                    textblock.Inlines.Add(link);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void link_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Tweet t = ((InlineLink)sender).DataContext as Tweet;
                //System.Diagnostics.Process.Start(((InlineLink)sender).Url.ToString());
                PluginEventHandler.FireEvent("linkclicked", t, ((InlineLink)sender).Url.ToString());
            }
            catch
            {
                MessageBox.Show("There was a problem launching the specified URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        static void conversationLink_Click(Object sender, RoutedEventArgs e)
        {
            InlineLink link = (InlineLink)sender;
            Tweet t = (Tweet)link.DataContext;
            PluginEventHandler.FireEvent("loadconversation", t);
        }
    }

    public class TimeReplyToBlock : TextBlock
    {
        public string ID
        {
            get { return (string)GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public string ReplyID
        {
            get { return (string)GetValue(ReplyIDProperty); }
            set { SetValue(ReplyIDProperty, value); }
        }

        public string ReplyTo
        {
            get { return (string)GetValue(ReplyToProperty); }
            set { SetValue(ReplyToProperty, value); }
        }

        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public string SourceText
        {
            get { return (string)GetValue(SourceTextProperty); }
            set { SetValue(SourceTextProperty, value); }
        }

        public string SourceUri
        {
            get { return (string)GetValue(SourceUriProperty); }
            set { SetValue(SourceUriProperty, value); }
        }

        public static readonly DependencyProperty SourceTextProperty =
            DependencyProperty.Register("SourceText", typeof(string), typeof(TimeReplyToBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSourceChanged)));

        public static readonly DependencyProperty SourceUriProperty =
            DependencyProperty.Register("SourceUri", typeof(string), typeof(TimeReplyToBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSourceChanged)));

        public static readonly DependencyProperty IDProperty =
            DependencyProperty.Register("ID", typeof(string), typeof(TimeReplyToBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSourceChanged)));

        public static readonly DependencyProperty ReplyIDProperty =
            DependencyProperty.Register("ReplyID", typeof(string), typeof(TimeReplyToBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSourceChanged)));

        public static readonly DependencyProperty ReplyToProperty =
            DependencyProperty.Register("ReplyTo", typeof(string), typeof(TimeReplyToBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSourceChanged)));

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(string), typeof(TimeReplyToBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnSourceChanged)));

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                TimeReplyToBlock textblock = (TimeReplyToBlock)obj;
                textblock.Inlines.Clear();
                //textblock.Inlines.Add(textblock.Time + " ");

                InlineLink time = new InlineLink();
                var tweet = (Tweet)textblock.DataContext;
                var uri = string.Format("http://twitter.com/{0}/status/{1}", tweet.Contact.Name.ToLower(), tweet.ID);
                time.Url = new Uri(uri);
                time.Inlines.Add(textblock.Time);
                time.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(link_Click);
                time.TextDecorations = null;
                time.ToolTip = "Open tweet in the default browser";
                textblock.Inlines.Add(time);
                textblock.Inlines.Add(" ");


                if (!String.IsNullOrEmpty(textblock.SourceUri) && !String.IsNullOrEmpty(textblock.SourceText))
                {
                    InlineLink link = new InlineLink();
                    link.Url = new Uri(textblock.SourceUri);
                    link.Inlines.Add(" from " + textblock.SourceText);
                    link.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(link_Click);
                    link.TextDecorations = null;
                    link.ToolTip = "Open link in the default browser";
                    textblock.Inlines.Add(link);

                } else if (!String.IsNullOrEmpty(textblock.SourceText))
                    textblock.Inlines.Add(textblock.SourceText);
                }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void link_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Tweet t = ((InlineLink)sender).DataContext as Tweet;
                //System.Diagnostics.Process.Start(((InlineLink)sender).Url.ToString());
                PluginEventHandler.FireEvent("linkclicked", t, ((InlineLink)sender).Url.ToString());
            }
            catch
            {
                MessageBox.Show("There was a problem launching the specified URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        static void conversationLink_Click(Object sender, RoutedEventArgs e)
        {
            InlineLink link = (InlineLink)sender;
            Tweet t = (Tweet)link.DataContext;
            PluginEventHandler.FireEvent("loadconversation", t);
        }
    }
}
