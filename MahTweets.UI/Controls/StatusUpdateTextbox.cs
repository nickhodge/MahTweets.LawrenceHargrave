using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Media;
using MahTweets.TweetProcessors;

namespace MahTweets.UI.Controls
{
    public class StatusUpdateTextbox : RichTextBox
    {
        private static ITextProcessorEngine _textProcessorEngine;

        public static readonly DependencyProperty UpdateProperty = DependencyProperty.Register("Text", typeof (String),
                                                                                               typeof (
                                                                                                   StatusUpdateTextbox),
                                                                                               new FrameworkPropertyMetadata
                                                                                                   (null, OnTextChanged));

        public StatusUpdateTextbox()
        {
            IsDocumentEnabled = true;

            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>();
                // get the cached engine that does all the hard work
            _textProcessorEngine.SetupElementCaches(this); // and ensure visual elements are setup

            GotFocus += StatusUpdateTextboxGotFocus;

            PreviewDrop += DroppedSomething;
            PreviewDragEnter += DraggedSomething;
            PreviewDragOver += DraggedSomething;
        }

        public String Text
        {
            get { return (String) GetValue(UpdateProperty); }
            set { SetValue(UpdateProperty, value); }
        }

        private static void DroppedSomething(object sender, DragEventArgs e)
        {
            var text = (string) e.Data.GetData(DataFormats.Text);
            if (text == null) return;
            var update = ((StatusUpdateTextbox) sender).DataContext as IStatusUpdate;
            if (update == null) return;
            var eventAggregator = CompositionManager.Get<IEventAggregator>();
            var pasteInline = new PasteInlineReplyPayload(text, update.ID);
            eventAggregator.GetEvent<PasteInlineReply>().Publish(pasteInline);
        }

        private static void DraggedSomething(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Handled = true;
            }
        }

        public static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        {
            DependencyObject current = initial;

            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current as T != null) break;
            }
            return current as T;
        }

        private void StatusUpdateTextboxGotFocus(object sender, RoutedEventArgs e)
        {
            var li = FindUpVisualTree<ListBoxItem>(this);
            li.IsSelected = true;
        }

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var textblock = obj as StatusUpdateTextbox;
            if (textblock == null) return;
            textblock.Document.Blocks.Clear();
            var para = new Paragraph
                           {
                               FontFamily = _textProcessorEngine.FfDefault,
                               FontSize = _textProcessorEngine.FsDefault,
                               Foreground = _textProcessorEngine.BrText,
                               Background = _textProcessorEngine.BrTransparent
                           };

            var tr = new TextRange(textblock.Document.ContentEnd, textblock.Document.ContentEnd);
            var update = textblock.DataContext as IStatusUpdate;

            if (update == null) return;
            string text = update.Text;

            if (!string.IsNullOrEmpty(text))
            {
                var txtCollector = new StringBuilder();
                foreach (string word in text.Split(' ')) // split into words
                {
                    bool wpappended = false;
                    string word1 = word;
                    foreach (
                        InlineLink ptext in
                            _textProcessorEngine.WordProcessors.Select(
                                tp => tp.Match(word1, _textProcessorEngine.BrLink, update)).Where(ptext => ptext != null)
                        )
                    {
                        if (txtCollector.Length > 0)
                        {
                            Inline st = ProcessAsSentence(txtCollector, update);
                            if (st != null)
                                para.Inlines.Add(st);
                            else
                                para.Inlines.Add(txtCollector.ToString());
                            txtCollector.Clear();
                        }
                        para.Inlines.Add(ptext); // append the new inline with the processed text
                        txtCollector.Append(" ");
                        wpappended = true;
                        break;
                    }
                    if (!wpappended)
                        txtCollector.Append(word + " ");
                }
                if (txtCollector.Length > 0)
                {
                    Inline st = ProcessAsSentence(txtCollector, update);
                    if (st != null)
                        para.Inlines.Add(st);
                    else
                        para.Inlines.Add(txtCollector.ToString());
                }
            }

            textblock.Document.Blocks.Add(para);

            if (update.Attachments == null || update.Attachments.Count <= 0) return;
            foreach (IStatusUpdateAttachment a in update.Attachments.Where(a => tr.End.Paragraph != null))
                if (tr.End.Paragraph != null) tr.End.Paragraph.Inlines.Add(a.NewView());
        }


        private static Inline ProcessAsSentence(StringBuilder txtCollector, IStatusUpdate oStatusUpdate)
        {
            return
                _textProcessorEngine.SentenceProcessors.Select(
                    sp => sp.Match(txtCollector.ToString(), _textProcessorEngine.BrText, oStatusUpdate)).FirstOrDefault(
                        stext => stext != null);
        }
    }
}