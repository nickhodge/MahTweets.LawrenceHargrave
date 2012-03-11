using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using MahTweets.Core;
using MahTweets.Core.Commands;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;

namespace MahTweets.UI.Controls
{
    public partial class CompositionBox
    {
        public static readonly DependencyProperty SendMessageCommand = DependencyProperty.RegisterAttached(
            "SendMessage", typeof (SendMessageCommand), typeof (CompositionBox));

        private TextRange currentRange;

        #region MahTweets Scripting additions

        private void contextMenuSelectedTextOpening(object sender, ContextMenuEventArgs e)
        {
            var fe = sender as FrameworkElement;

            var newcontextualmenu = new ContextMenu();
            var _scriptingUIHelper = CompositionManager.Get<IScriptingUIHelper>();
            var _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

            var ewf = Keyboard.FocusedElement as UIElement; // this retrieves the current textbox that has selection
            if (ewf is CompositionTextBox) // if it is a compositiontextbox
            {
                var tbwf = ewf as RichTextBox;
                var spellingSuggestions = tbwf.GetSpellingError(tbwf.CaretPosition);
                if (spellingSuggestions != null)
                {
                    if (spellingSuggestions.Suggestions.HasItems())
                    {
                        foreach (string str in spellingSuggestions.Suggestions)
                        {
                            var mi = new MenuItem
                                         {
                                             Header = str,
                                             FontWeight = FontWeights.Bold,
                                             Command = EditingCommands.CorrectSpellingError,
                                             CommandParameter = str,
                                             CommandTarget = tbwf
                                         };
                            newcontextualmenu.Items.Add(mi);
                        }
                    }
                    else
                    {
                        var mi = new MenuItem
                                     {Header = "No Suggestions", FontWeight = FontWeights.Bold, IsEnabled = false};
                        newcontextualmenu.Items.Add(mi);
                    }
                    newcontextualmenu.Items.Add(new Separator());
                }
            }
            if (_scriptingUIHelper != null)
            {
                _scriptingUIHelper.createCCPContextualMenus(newcontextualmenu, contextMenuItem_StandardCCP);

                var menu =
                    _scriptingUIHelper.CreateContextMenusFromScripts(true,
                                                                     _scriptingConfiguration.
                                                                         ComposeTextContextMenuEntryPoint,
                                                                     mi_TextSelect_Click);

                if (menu != null)
                {
                    foreach (object i in menu)
                        newcontextualmenu.Items.Add(i);
                }
            }
            if (fe != null) fe.ContextMenu = newcontextualmenu;
        }

        public void mi_TextSelect_Click(object sender, RoutedEventArgs e)
        {
            var mif = e.Source as MenuItem;
            var _scriptingUIHelper = CompositionManager.Get<IScriptingUIHelper>();
            var _scriptingEngine = CompositionManager.Get<IScriptingEngine>();
            var _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

            if (mif == null) return;
            var guidintoscripts = mif.Name; // get the name of the chosen contextual menu, used as key into scripts

            if (guidintoscripts == null && _scriptingUIHelper != null && _scriptingEngine != null) return;
            if (_scriptingUIHelper == null || _scriptingUIHelper.IsEditScriptEvent(null, guidintoscripts)) return;
            var ewf = Keyboard.FocusedElement as UIElement; // this retrieves the current textbox that has selection
            if (!(ewf is CompositionTextBox)) return;
            var tbwf = ewf as CompositionTextBox;
            var textselected = tbwf.Selection.Text; // textual contextual menu needs the selected text
            string textreturned = null;
            if (_scriptingEngine != null)
                textreturned =
                    (string)
                    _scriptingEngine.ExecuteScriptGUID(guidintoscripts,
                                                       _scriptingConfiguration.ComposeTextContextMenuEntryPoint,
                                                       textselected); // using the name which is a GUID, 

            if (textreturned != null)
            {
                tbwf.Selection.Text = textreturned;
            }
        }

        private void contextMenuItem_StandardCCP(object sender, RoutedEventArgs e)
        {
            var mif = e.Source as MenuItem;
            if (mif == null) return;
            var commandchosen = mif.Name; // get the name of the chosen contextual menu, used as key into scripts
            var ewf = Keyboard.FocusedElement as UIElement; // this retrieves the current textbox that has selection
            if (!(ewf is CompositionTextBox)) return;
            var tbwf = ewf as RichTextBox;
            switch (commandchosen)
            {
                case "Cut":
                    tbwf.Cut();
                    break;
                case "Copy":
                    tbwf.Copy();
                    break;
                case "Paste":
                    tbwf.Paste();
                    break;
            }
        }

        #endregion

        public CompositionBox()
        {
            InitializeComponent();
            txtUpdateBox.Height = (Double) FindResource("DefaultCompositionBoxHeight");
            txtUpdateBox.IsDocumentEnabled = true;
#if DEBUG
            if (this.IsInDesignMode())
                ctlAutoComplete.Visibility = Visibility.Visible;
#endif
        }

        public CompositionTextBox TextBox
        {
            get { return txtUpdateBox; }
        }

        public SendMessageCommand SendMessage
        {
            get { return (SendMessageCommand) GetValue(SendMessageCommand); }
            set { SetValue(SendMessageCommand, value); }
        }

        public string Text
        {
            get { return txtUpdateBox.Text; }
            set { txtUpdateBox.Text = value; }
        }

        public void SetHeight(double h)
        {
            txtUpdateBox.MaxHeight = h;
        }

        public void FocusOnText()
        {
            txtUpdateBox.Focus();
        }

        public void ScrollToEnd()
        {
            txtUpdateBox.CaretPosition = txtUpdateBox.Document.ContentEnd;
        }

        protected override async void OnPreviewKeyDown(KeyEventArgs e)
        {
            var caretPosition = txtUpdateBox.Selection.Start;

            if (e.Key == Key.Space || e.Key == Key.Return)
            {
                txtUpdateBox.wordsAddedFlag = true;
                txtUpdateBox.selectionStartPosition = txtUpdateBox.Selection.Start;
                txtUpdateBox.selectionEndPosition = txtUpdateBox.Selection.End.GetPositionAtOffset(0,
                                                                                                   LogicalDirection.
                                                                                                       Forward);

                if (e.Key == Key.Enter &&
                    !(Keyboard.Modifiers == ModifierKeys.Control || Keyboard.Modifiers == ModifierKeys.Shift))
                {
                    if (ctlAutoComplete.IsActive)
                    {
                        //hide autocomplete
                        HideShowAutoComplete(string.Empty, false);

                        //show and process autocomplete
                        var ct = ctlAutoComplete.SelectedContact();
                        if (ct != null)
                        {
                            currentRange.Text = "@" + ct.Name;
                            txtUpdateBox.Selection.Select(currentRange.End, currentRange.End);

                            e.Handled = true;
                            return;
                        }
                    }
                    else
                    {
                        var message = txtUpdateBox.Text.Trim();

                        if (!String.IsNullOrEmpty(message))
                        {
                            if (SendMessage.CanExecute(message))
                            {
                                var autoUrlShorten =
                                    CompositionManager.Get<IApplicationSettingsProvider>().AutoUrlShorten;
                                if (autoUrlShorten)
                                {
                                    var UrlShortener =
                                        CompositionManager.Get<IApplicationSettingsProvider>().DefaultShortener;
                                    txtUpdateBox.IsEnabled = false;
                                    var words = message.Split(' ');

                                    for (var i = 0; i < words.Count(); i++)
                                    {
                                        if (!words[i].ContainsHyperlink()) continue;
                                        var matches = words[i].GetHyperlinks();
                                        foreach (Match m in matches)
                                        {
                                            var wordURL = m.Value;
                                            var i1 = i;
                                            words[i1] = await UrlShortener.Shorten(wordURL);
                                        }
                                    }
                                    txtUpdateBox.Text = "";
                                    txtUpdateBox.IsEnabled = true;
                                    SendMessage.Execute(String.Join(" ", words), this);
                                }
                                else
                                {
                                    txtUpdateBox.Text = "";
                                    SendMessage.Execute(message, this);
                                }
                            }

                            e.Handled = true;
                            return;
                        }
                    }
                }

                if (e.Key == Key.Space && ctlAutoComplete.IsActive)
                {
                    HideShowAutoComplete(string.Empty, false);
                    return;
                }
            }
            else if (e.Key == Key.Back)
            {
                var backspacePosition = caretPosition.GetNextInsertionPosition(LogicalDirection.Backward);
                Hyperlink hyperlink;
                if (backspacePosition != null &&
                    HyperlinkHelper.IsHyperlinkBoundaryCrossed(caretPosition, backspacePosition, out hyperlink))
                {
                    var newCaretPosition = caretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);

                    var hyperlinkChildren = hyperlink.Inlines;
                    var inlines = new Inline[hyperlinkChildren.Count];
                    hyperlinkChildren.CopyTo(inlines, 0);

                    for (var i = inlines.Length - 1; i >= 0; i--)
                    {
                        hyperlinkChildren.Remove(inlines[i]);
                        if (hyperlink.SiblingInlines != null)
                            hyperlink.SiblingInlines.InsertAfter(hyperlink, inlines[i]);
                    }

                    var localProperties = hyperlink.GetLocalValueEnumerator();
                    var inlineRange = new TextRange(inlines[0].ContentStart, inlines[inlines.Length - 1].ContentEnd);

                    while (localProperties.MoveNext())
                    {
                        var property = localProperties.Current;
                        var dp = property.Property;
                        var value = property.Value;

                        if (!dp.ReadOnly &&
                            dp != Inline.TextDecorationsProperty && // Ignore hyperlink defaults.
                            dp != TextElement.ForegroundProperty &&
                            dp != BaseUriHelper.BaseUriProperty &&
                            dp != FrameworkContentElement.ContextMenuProperty &&
                            dp != TextElement.BackgroundProperty &&
                            !HyperlinkHelper.IsHyperlinkProperty(dp))
                        {
                            inlineRange.ApplyPropertyValue(dp, value);
                        }
                    }

                    if (newCaretPosition != null) txtUpdateBox.Selection.Select(newCaretPosition, newCaretPosition);
                }
            }
            else
            {
                //Moving Up/Down the autocomplete list
                if (e.Key == Key.Up)
                {
                    ctlAutoComplete.MoveUp();
                }
                else if (e.Key == Key.Down)
                {
                    ctlAutoComplete.MoveDown();
                }
                else if (!(e.Key == Key.Left || e.Key == Key.Right))
                {
                    var wordRange = WordBreaker.GetWordRange(caretPosition);
                    if (!(wordRange == null || wordRange.IsEmpty))
                    {
                        currentRange = wordRange;
                        var wordText = wordRange.Text + e.Key.ToString();

                        if (wordText.StartsWith("@"))
                        {
                            HideShowAutoComplete(wordText, true);
                        }
                    }
                }
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            HideShowAutoComplete(string.Empty, false);
        }

        private void HideShowAutoComplete(string filter, bool show)
        {
            if (show)
            {
                popAutoComplete.IsOpen = true;
                ctlAutoComplete.IsActive = true;
                ctlAutoComplete.FilterText = filter.ToLower(); //not case sensitive, use lowercase
                ctlAutoComplete.Visibility = Visibility.Visible;
            }
            else //hide
            {
                popAutoComplete.IsOpen = false;
                ctlAutoComplete.IsActive = false;
                ctlAutoComplete.Visibility = Visibility.Collapsed;
            }
        }
    }
}