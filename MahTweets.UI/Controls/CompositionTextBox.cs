using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.UI.Controls
{
    public class CompositionTextBox : RichTextBox, INotifyPropertyChanged
    {
        public bool IsInlineReply = true;
        public TextPointer selectionEndPosition;
        public TextPointer selectionStartPosition;
        public bool wordsAddedFlag;

        public CompositionTextBox()
        {
            DataObject.AddPastingHandler(this, DataObjectPastingEventHandler);
            TextChanged += TextChangedEventHandler;
            Background = null;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            var c = (TextBlock) FindName("compositionTextCharacterCount");
            if (c != null)
            {
                c.Foreground = LengthColour(Text.Length);
            }

            UrlShorteners = CompositionManager.GetAll<IUrlShortener>();
        }

        public IEnumerable<IUrlShortener> UrlShorteners { get; set; }

        public String Text
        {
            get
            {
                return Document.Blocks.Select(b => new TextRange(b.ContentStart, b.ContentEnd)).Aggregate(String.Empty,
                                                                                                          (current, tr)
                                                                                                          =>
                                                                                                          current +
                                                                                                          tr.Text);
            }
            set
            {
                Document.Blocks.Clear();
                new TextRange(Document.ContentEnd, Document.ContentEnd) {Text = value};
                NotifyPropertyChanged("Text");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public event EventHandler CompositionUpdated;

        public Brush LengthColour(int txtlngth)
        {
            if (txtlngth < 70) return (Brush) FindResource("Under70Colour");
            if (txtlngth < 100) return (Brush) FindResource("Under100Colour");
            if (txtlngth < 140) return (Brush) FindResource("Under140Colour");
            if (txtlngth >= 140) return (Brush) FindResource("OverLengthColour");
            return (Brush) FindResource("LightGreyColour"); // fallback to tumbledown
        }

        /// <summary>
        /// Event handler for DataObject.Pasting event on this RichTextBox.
        /// </summary>
        private void DataObjectPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            wordsAddedFlag = true;
            selectionStartPosition = Selection.Start;
            selectionEndPosition = Selection.IsEmpty
                                       ? Selection.End.GetPositionAtOffset(0, LogicalDirection.Forward)
                                       : Selection.End;

            // We don't handle the event here. Let the base RTB handle the paste operation.
            // This will raise a TextChanged event, which we handle below to scan for any matching hyperlinks.
        }

        /// <summary>
        /// Event handler for RichTextBox.TextChanged event.
        /// </summary>
        private void TextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            NotifyPropertyChanged("Text");

            var rtb = sender as CompositionTextBox;
            if (rtb != null)
                if (rtb.IsInlineReply)
                    // The richtextbox/compositionbox on the top of the mainview is set to; therefore don't autosize.
                    Height = Double.NaN;
                        // therefore we are an inline reply, autosize (as the text area is small & needs space to show all the characters)

            var c = (TextBlock) FindName("compositionTextCharacterCount");
            if (c != null)
            {
                c.Foreground = LengthColour(Text.Length);
            }

            var ll = (TextBlock) FindName("compositionTextCharacterLengthLeft");
            if (ll != null)
            {
                if (Text.Length > 140)
                {
                    ll.Text = (140 - Text.Length).ToString(CultureInfo.InvariantCulture);
                    ll.Visibility = Visibility.Visible;
                }
                else
                {
                    ll.Visibility = Visibility.Hidden;
                }
            }


            if (!wordsAddedFlag || Document == null) return;

            // Temporarily disable TextChanged event handler, since following code might insert Hyperlinks,
            // which will raise another TextChanged event.
            TextChanged -= TextChangedEventHandler;


            var navigator = selectionStartPosition;
            while (navigator != null && navigator.CompareTo(selectionEndPosition) <= 0)
            {
                var wordRange = WordBreaker.GetWordRange(navigator);
                if (wordRange == null || wordRange.IsEmpty)
                {
                    break;
                }

                var wordText = wordRange.Text;
                if (wordText.ContainsHyperlink() && !HyperlinkHelper.IsInHyperlinkScope(wordRange.Start) &&
                    !HyperlinkHelper.IsInHyperlinkScope(wordRange.End))
                {
                    var hyperlink = new Hyperlink(wordRange.Start, wordRange.End);
                    var bc = new BrushConverter();
                    hyperlink.Background = (Brush) bc.ConvertFromString("#27005D9D");

                    var cm = new ContextMenu();
                    foreach (
                        var mi in
                            UrlShorteners.Select(
                                s => new MenuItem {Header = "Shorten with " + s.Name, Tag = hyperlink, DataContext = s})
                        )
                    {
                        mi.Click += mi_Click;
                        cm.Items.Add(mi);
                    }
                    hyperlink.ContextMenu = cm;
                    navigator = hyperlink.ElementEnd.GetNextInsertionPosition(LogicalDirection.Forward);
                }
                else
                {
                    navigator = wordRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                }
            }

            TextChanged += TextChangedEventHandler;
            wordsAddedFlag = false;
            selectionStartPosition = null;
            selectionEndPosition = null;

            if (CompositionUpdated != null)
                CompositionUpdated(this, new EventArgs());
        }

        public async void mi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mi = (MenuItem) sender;
                if (mi.Tag is Hyperlink)
                {
                    var hyperlink = mi.Tag as Hyperlink;
                    var link = ((Run) hyperlink.Inlines.FirstInline).Text;
                    var shortener = mi.DataContext as IUrlShortener;

                    //fire off a new thread and replace
                    if (shortener != null)
                    {
                        ((Run) hyperlink.Inlines.FirstInline).Text = await shortener.Shorten(link);
                    }
                    //make it null so it can't be reshortened
                    // mi.Tag = null;
                }
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        // Private flag - true when word(s) are added to this RichTextBox.

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public static class HyperlinkHelper
    {
        // Helper that returns true when passed property applies to Hyperlink only.
        public static bool IsHyperlinkProperty(DependencyProperty dp)
        {
            return dp == Hyperlink.CommandProperty ||
                   dp == Hyperlink.CommandParameterProperty ||
                   dp == Hyperlink.CommandTargetProperty ||
                   dp == Hyperlink.NavigateUriProperty ||
                   dp == Hyperlink.TargetNameProperty;
        }

        // Helper that returns true if passed caretPosition and backspacePosition cross a hyperlink end boundary
        // (under the assumption that caretPosition and backSpacePosition are adjacent insertion positions).
        public static bool IsHyperlinkBoundaryCrossed(TextPointer caretPosition, TextPointer backspacePosition,
                                                      out Hyperlink backspacePositionHyperlink)
        {
            var caretPositionHyperlink = GetHyperlinkAncestor(caretPosition);
            backspacePositionHyperlink = GetHyperlinkAncestor(backspacePosition);

            return (caretPositionHyperlink == null && backspacePositionHyperlink != null) ||
                   (caretPositionHyperlink != null && backspacePositionHyperlink != null &&
                    caretPositionHyperlink != backspacePositionHyperlink);
        }

        // Helper that returns a Hyperlink ancestor of passed TextPointer.
        private static Hyperlink GetHyperlinkAncestor(TextPointer position)
        {
            var parent = position.Parent as Inline;
            while (parent != null && !(parent is Hyperlink))
            {
                parent = parent.Parent as Inline;
            }

            return parent as Hyperlink;
        }

        // Helper that returns true when passed TextPointer is within the scope of a Hyperlink element.
        public static bool IsInHyperlinkScope(TextPointer position)
        {
            return GetHyperlinkAncestor(position) != null;
        }
    }

    public static class WordBreaker
    {
        /// <summary>
        /// Returns a TextRange covering a word containing or following this TextPointer.
        /// </summary>
        /// <remarks>
        /// If this TextPointer is within a word or at start of word, the containing word range is returned.
        /// If this TextPointer is between two words, the following word range is returned.
        /// If this TextPointer is at trailing word boundary, the following word range is returned.
        /// </remarks>
        public static TextRange GetWordRange(TextPointer position)
        {
            TextRange wordRange = null;
            TextPointer wordStartPosition = null;

            // Go forward first, to find word end position.
            TextPointer wordEndPosition = GetPositionAtWordBoundary(position, LogicalDirection.Forward);

            if (wordEndPosition != null)
            {
                // Then travel backwards, to find word start position.
                wordStartPosition = GetPositionAtWordBoundary(wordEndPosition, LogicalDirection.Backward);
            }

            if (wordStartPosition != null)
            {
                wordRange = new TextRange(wordStartPosition, wordEndPosition);
                // Logging.Info("Current Text: '{0}'", currentText);
            }

            return wordRange;
        }

        /// <summary>
        /// 1.  When wordBreakDirection = Forward, returns a position at the end of the word,
        ///     i.e. a position with a wordBreak character (space) following it.
        /// 2.  When wordBreakDirection = Backward, returns a position at the start of the word,
        ///     i.e. a position with a wordBreak character (space) preceeding it.
        /// 3.  Returns null when there is no workbreak in the requested direction.
        /// </summary>
        private static TextPointer GetPositionAtWordBoundary(TextPointer position, LogicalDirection wordBreakDirection)
        {
            if (!position.IsAtInsertionPosition)
            {
                position = position.GetInsertionPosition(wordBreakDirection);
            }

            var navigator = position;
            while (navigator != null && !IsPositionNextToWordBreak(navigator, wordBreakDirection))
            {
                navigator = navigator.GetNextInsertionPosition(wordBreakDirection);
            }

            return navigator;
        }

        // Helper for GetPositionAtWordBoundary.
        // Returns true when passed TextPointer is next to a wordBreak in requested direction.
        private static bool IsPositionNextToWordBreak(TextPointer position, LogicalDirection wordBreakDirection)
        {
            var isAtWordBoundary = false;

            // Skip over any formatting.
            if (position.GetPointerContext(wordBreakDirection) != TextPointerContext.Text)
            {
                position = position.GetInsertionPosition(wordBreakDirection);
            }

            if (position.GetPointerContext(wordBreakDirection) == TextPointerContext.Text)
            {
                var oppositeDirection = (wordBreakDirection == LogicalDirection.Forward)
                                                         ? LogicalDirection.Backward
                                                         : LogicalDirection.Forward;

                var runBuffer = new char[1];
                var oppositeRunBuffer = new char[1];

                position.GetTextInRun(wordBreakDirection, runBuffer, /*startIndex*/0, /*count*/1);
                position.GetTextInRun(oppositeDirection, oppositeRunBuffer, /*startIndex*/0, /*count*/1);

                if (runBuffer[0] == ' ' && oppositeRunBuffer[0] != ' ')
                {
                    isAtWordBoundary = true;
                }
            }
            else
            {
                // If we're not adjacent to text then we always want to consider this position a "word break".  
                // In practice, we're most likely next to an embedded object or a block boundary.
                isAtWordBoundary = true;
            }

            return isAtWordBoundary;
        }
    }
}