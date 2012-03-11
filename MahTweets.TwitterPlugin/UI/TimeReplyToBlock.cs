using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MahTweets.Core;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Media;

namespace MahTweets.TwitterPlugin.UI
{
    public class InReplyToBlock : TextBlock
    {
        public static readonly DependencyProperty ReplyIDProperty =
            DependencyProperty.Register("ReplyID", typeof (string), typeof (InReplyToBlock),
                                        new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));

        public static readonly DependencyProperty ReplyToProperty =
            DependencyProperty.Register("ReplyTo", typeof (string), typeof (InReplyToBlock),
                                        new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));

        public string ReplyID
        {
            get { return (string) GetValue(ReplyIDProperty); }
            set { SetValue(ReplyIDProperty, value); }
        }

        public string ReplyTo
        {
            get { return (string) GetValue(ReplyToProperty); }
            set { SetValue(ReplyToProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var textblock = (InReplyToBlock) obj;
                textblock.Inlines.Clear();
                textblock.Visibility = Visibility.Collapsed;

                if (textblock.ReplyID != "0" && !string.IsNullOrEmpty(textblock.ReplyID))
                {
                    var link = new InlineLink();
                    link.HoverColour = (Brush) link.FindResource("HoverColour");
                    link.NormalColour = (Brush) link.FindResource("BaseColour");
                    link.Inlines.Add(textblock.ReplyTo);
                    link.TextDecorations = null;
                    link.ToolTip = "View conversation";
                    link.MouseLeftButtonDown += conversationLink_Click;
                    textblock.Inlines.Add(new Run(" in reply to "));
                    textblock.Inlines.Add(link);
                    textblock.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private static void conversationLink_Click(Object sender, RoutedEventArgs e)
        {
            var link = (InlineLink) sender;
            var t = (Tweet) link.DataContext;
            PluginEventHandler.FireEvent("loadconversation", t);
        }
    }

    public class TimeReplyToBlock : TextBlock
    {
        public static readonly DependencyProperty SourceTextProperty =
            DependencyProperty.Register("SourceText", typeof (string), typeof (TimeReplyToBlock),
                                        new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));

        public static readonly DependencyProperty SourceUriProperty =
            DependencyProperty.Register("SourceUri", typeof (string), typeof (TimeReplyToBlock),
                                        new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));

        public static readonly DependencyProperty IDProperty =
            DependencyProperty.Register("ID", typeof (string), typeof (TimeReplyToBlock),
                                        new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));

        public static readonly DependencyProperty ReplyIDProperty =
            DependencyProperty.Register("ReplyID", typeof (string), typeof (TimeReplyToBlock),
                                        new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));

        public static readonly DependencyProperty ReplyToProperty =
            DependencyProperty.Register("ReplyTo", typeof (string), typeof (TimeReplyToBlock),
                                        new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof (string), typeof (TimeReplyToBlock),
                                        new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));

        public string ID
        {
            get { return (string) GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public string ReplyID
        {
            get { return (string) GetValue(ReplyIDProperty); }
            set { SetValue(ReplyIDProperty, value); }
        }

        public string ReplyTo
        {
            get { return (string) GetValue(ReplyToProperty); }
            set { SetValue(ReplyToProperty, value); }
        }

        public string Time
        {
            get { return (string) GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public string SourceText
        {
            get { return (string) GetValue(SourceTextProperty); }
            set { SetValue(SourceTextProperty, value); }
        }

        public string SourceUri
        {
            get { return (string) GetValue(SourceUriProperty); }
            set { SetValue(SourceUriProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var textblock = (TimeReplyToBlock) obj;
                textblock.Inlines.Clear();

                var time = new InlineLink();
                var tweet = textblock.DataContext as Tweet;
                if (tweet == null)
                    return;

                var uri =
                    new Uri(string.Format("http://twitter.com/{0}/status/{1}", tweet.Contact.Name.ToLower(), tweet.ID));

                var clockimage = new InlineLink
                                     {
                                         Text = "P ",
                                         Name = "clockimage",
                                         FontFamily = (FontFamily) textblock.FindResource("WebSymbols"),
                                         FontSize = 9,
                                         Foreground = (Brush) textblock.FindResource("BaseColour"),
                                         HoverColour = (Brush) textblock.FindResource("BaseColour"),
                                         NormalColour = (Brush) textblock.FindResource("BaseColour")
                                     };

                time.Url = uri;
                time.Tag = tweet.Contact.Name.ToLower() + " " + tweet.ID;
                    // grab these now for the dragging and dropping later
                time.MouseMove += time_link_drag;
                time.Inlines.Add(textblock.Time);
                time.MouseUp += link_Click;
                time.TextDecorations = null;
                time.HoverColour = (Brush) time.FindResource("HoverColour");
                time.NormalColour = (Brush) time.FindResource("BaseColour");
                time.ToolTip = "Open tweet in the default browser" + Environment.NewLine + uri + Environment.NewLine +
                               "Or drag to desktop to create link to this tweet";
                textblock.Inlines.Add(clockimage);
                textblock.Inlines.Add(time);

                if (!String.IsNullOrEmpty(textblock.SourceUri) && !String.IsNullOrEmpty(textblock.SourceText))
                {
                    try
                    {
                        if (Uri.TryCreate(textblock.SourceUri, UriKind.Absolute, out uri))
                        {
                            var link = new InlineLink {Url = new Uri(textblock.SourceUri)};
                            link.Inlines.Add(textblock.SourceText);
                            link.HoverColour = (Brush) link.FindResource("HoverColour");
                            link.NormalColour = (Brush) link.FindResource("BaseColour");
                            link.MouseLeftButtonDown += link_Click;
                            link.TextDecorations = null;
                            link.ToolTip = "Open link to tweeting application\'s web site";
                            textblock.Inlines.Add(new Run(" from "));
                            textblock.Inlines.Add(link);
                        }
                        else
                        {
                            textblock.Inlines.Add("from " + textblock.SourceText);
                        }
                    }
                    catch
                    {
                        textblock.Inlines.Add("from " + textblock.SourceText);
                    }
                }
                else if (!String.IsNullOrEmpty(textblock.SourceText))
                {
                    textblock.Inlines.Add(textblock.SourceText);
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private static void time_link_drag(object s, MouseEventArgs e)
        {
            var rplyToBlock = s as InlineLink;

            if (rplyToBlock == null || e.LeftButton != MouseButtonState.Pressed) return;
            var drg = new Dragger();
            DataObject dobj = drg.BuildDesktopInternetShortcutDragger(rplyToBlock.Tag.ToString(),
                                                                      rplyToBlock.Url.ToString());
            DragDrop.DoDragDrop(rplyToBlock, dobj, DragDropEffects.Link);
        }

        private static void link_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var t = ((InlineLink) sender).DataContext as Tweet;
                PluginEventHandler.FireEvent("linkclicked", t, ((InlineLink) sender).Url.ToString());
            }
            catch
            {
                MessageBox.Show("There was a problem launching the specified URL.", "Error", MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
            }
        }
    }

    public class RetweetedBy : TextBlock
    {
        public static readonly DependencyProperty ByProperty =
            DependencyProperty.Register("By", typeof (Contact), typeof (RetweetedBy),
                                        new UIPropertyMetadata(null, OnSourceChanged));

        public static Contact GetBy(DependencyObject obj)
        {
            return (Contact) obj.GetValue(ByProperty);
        }

        public static void SetBy(DependencyObject obj, Double value)
        {
            obj.SetValue(ByProperty, value);
        }

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                var textblock = (RetweetedBy) obj;
                textblock.Inlines.Clear();
                var tweet = (Tweet) textblock.DataContext;

                if (tweet.RetweetBy != null)
                {
                    var rtimage = new InlineLink
                                      {
                                          Text = " J",
                                          FontFamily = (FontFamily) textblock.FindResource("WebSymbols"),
                                          FontSize = 9,
                                          Foreground = (Brush) textblock.FindResource("BaseColour"),
                                          HoverColour = (Brush) textblock.FindResource("BaseColour"),
                                          NormalColour = (Brush) textblock.FindResource("BaseColour")
                                      };
                    var retweet = new InlineLink();
                    string uri = string.Format("http://twitter.com/{0}/status/{1}", tweet.RetweetBy.Name.ToLower(),
                                               tweet.ID);
                    retweet.Url = new Uri(uri);
                    retweet.Inlines.Add(" retweet via " + tweet.RetweetBy.Name);
                    textblock.Inlines.Add(rtimage);
                    textblock.Inlines.Add(retweet);
                    textblock.Visibility = Visibility.Visible;
                }
                else
                {
                    textblock.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }
    }
}