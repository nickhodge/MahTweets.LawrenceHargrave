using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahTweets.Core.Commands;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.UI.Controls
{
    [TemplatePart(Name = "PART_Buttons", Type = typeof (ContentPresenter))]
    [TemplatePart(Name = "PART_InlineWithContactName", Type = typeof (ContentPresenter))]
    [TemplatePart(Name = "PART_BelowTextBox", Type = typeof (ContentPresenter))]
    [TemplatePart(Name = "PART_ContactName", Type = typeof (ContentPresenter))]
    [TemplatePart(Name = "PART_ReplyExtras", Type = typeof (ContentPresenter))]
    [TemplatePart(Name = "PART_BelowReplyExtras", Type = typeof (ContentPresenter))]
    [TemplatePart(Name = "PART_BelowAvatar", Type = typeof (ContentPresenter))]
    public class StatusUpdateView : ItemsControl
    {
        /* Properties */

        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.RegisterAttached("Buttons",
                                                                                                        typeof (object),
                                                                                                        typeof (
                                                                                                            StatusUpdateTextbox
                                                                                                            ),
                                                                                                        new UIPropertyMetadata
                                                                                                            (null));

        public static readonly DependencyProperty InlineWithContactNameProperty =
            DependencyProperty.RegisterAttached("InlineWithContactName", typeof (object), typeof (StatusUpdateTextbox),
                                                new UIPropertyMetadata(null));

        public static readonly DependencyProperty BelowTextBoxProperty = DependencyProperty.Register("BelowTextBox",
                                                                                                     typeof (object),
                                                                                                     typeof (
                                                                                                         StatusUpdateView
                                                                                                         ),
                                                                                                     new UIPropertyMetadata
                                                                                                         (null));

        public static readonly DependencyProperty ReplyExtrasProperty = DependencyProperty.Register("ReplyExtras",
                                                                                                    typeof (object),
                                                                                                    typeof (
                                                                                                        StatusUpdateView
                                                                                                        ),
                                                                                                    new UIPropertyMetadata
                                                                                                        (null));

        public static readonly DependencyProperty BelowReplyExtrasProperty =
            DependencyProperty.Register("BelowReplyExtras", typeof (object), typeof (StatusUpdateView),
                                        new UIPropertyMetadata(null));

        public static readonly DependencyProperty BelowAvatarProperty = DependencyProperty.Register("BelowAvatar",
                                                                                                    typeof (object),
                                                                                                    typeof (
                                                                                                        StatusUpdateView
                                                                                                        ),
                                                                                                    new UIPropertyMetadata
                                                                                                        (null));

        public static readonly RoutedEvent ContactNameClickedEvent =
            EventManager.RegisterRoutedEvent("ContactNameClicked", RoutingStrategy.Bubble, typeof (RoutedEventHandler),
                                             typeof (StatusUpdateView));

        public static readonly DependencyProperty SendMessageCommand = DependencyProperty.RegisterAttached(
            "SendMessage", typeof (SendMessageCommand), typeof (StatusUpdateView));

        static StatusUpdateView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (StatusUpdateView),
                                                     new FrameworkPropertyMetadata(typeof (StatusUpdateView)));
        }

        public object BelowTextBox
        {
            get { return GetValue(BelowTextBoxProperty); }
            set { SetValue(BelowTextBoxProperty, value); }
        }

        public object ReplyExtras
        {
            get { return GetValue(ReplyExtrasProperty); }
            set { SetValue(ReplyExtrasProperty, value); }
        }

        public object BelowAvatar
        {
            get { return GetValue(BelowAvatarProperty); }
            set { SetValue(BelowAvatarProperty, value); }
        }

        public object BelowReplyExtras
        {
            get { return GetValue(BelowReplyExtrasProperty); }
            set { SetValue(BelowReplyExtrasProperty, value); }
        }

        public object Buttons
        {
            get { return GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public object InlineWithContactName
        {
            get { return GetValue(InlineWithContactNameProperty); }
            set { SetValue(InlineWithContactNameProperty, value); }
        }

        /* Events */

        public FrameworkElement CompositionBoxContainer
        {
            get { return Template.FindName("spReply", this) as FrameworkElement; }
        }

        public CompositionBox CompositionBox
        {
            get { return Template.FindName("txtReply", this) as CompositionBox; }
        }

        public IStatusUpdate StatusUpdate
        {
            get { return DataContext as IStatusUpdate; }
        }

        public SendMessageCommand SendMessage
        {
            get { return (SendMessageCommand) GetValue(SendMessageCommand); }
            set { SetValue(SendMessageCommand, value); }
        }

        public event RoutedEventHandler ContactNameClicked
        {
            add { AddHandler(ContactNameClickedEvent, value); }
            remove { RemoveHandler(ContactNameClickedEvent, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            try
            {
                var PART_ContactName = (TextBlock) Template.FindName("PART_ContactName", this);
                PART_ContactName.MouseDown += PART_ContactName_MouseDown;

                var StatusUpdateTextbox = (StatusUpdateTextbox) Template.FindName("StatusUpdateTextbox", this);

                StatusUpdateTextbox.ContextMenuOpening += contextMenuSelectedTextOpening;


                var compositionBox = (CompositionBox) Template.FindName("txtReply", this);
                compositionBox.SendMessage = SendMessage;
                Height = Double.NaN;
                compositionBox.Height = Double.NaN;
                    // this sets the height to Auto; Auto will auto rize the reply area to fix the text ref remarks in: http://msdn.microsoft.com/en-us/library/system.windows.frameworkelement.height.aspx

                MouseDown += StatusUpdateView_MouseDown;
                ContextMenuOpening += StatusUpdateView_ContextMenuOpening;
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        private void MarkRead()
        {
            ((IStatusUpdate) DataContext).IsRead = true;
        }

        private void StatusUpdateView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MarkRead();
        }

        private void contextMenuSelectedTextOpening(object sender, ContextMenuEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var scriptingUiHelper = CompositionManager.Get<IScriptingUIHelper>();
            var scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();


            if (scriptingUiHelper != null)
            {
                var newcontextualmenu = new ContextMenu();

                scriptingUiHelper.createCCPContextualMenus(newcontextualmenu, contextMenuItem_StandardCCP, false, true,
                                                           true);

                List<object> menu = scriptingUiHelper.CreateContextMenusFromScripts(newcontextualmenu.HasItems,
                                                                                    scriptingConfiguration.
                                                                                        StatusUpdateTextContextMenuEntryPoint,
                                                                                    mi_TextSelect_Click);

                if (menu != null)
                {
                    foreach (object i in menu)
                        newcontextualmenu.Items.Add(i);
                }

                if (fe != null) fe.ContextMenu = newcontextualmenu;
            }
            MarkRead();
        }

        private void contextMenuItem_StandardCCP(object sender, RoutedEventArgs e)
        {
            var mif = e.Source as MenuItem;
            if (mif == null) return;

            string commandchosen = mif.Name; // get the name of the chosen contextual menu, used as key into scripts
            var ewf = Keyboard.FocusedElement as UIElement; // this retrieves the current textbox that has selection
            if (!(ewf is StatusUpdateTextbox)) return;

            var tbwf = ewf as RichTextBox;
            switch (commandchosen)
            {
                case "Copy":
                    tbwf.Copy();
                    break;
                case "Paste":
                    if (Clipboard.ContainsText())
                    {
                        var text = Clipboard.GetData(DataFormats.Text) as string;
                        var eventAggregator = CompositionManager.Get<IEventAggregator>();
                        var pasteInline = new PasteInlineReplyPayload(text, StatusUpdate.ID);
                        eventAggregator.GetEvent<PasteInlineReply>().Publish(pasteInline);
                    }
                    break;
            }
        }

        private void mi_TextSelect_Click(object sender, RoutedEventArgs e)
        {
            var mif = e.Source as MenuItem;
            if (mif != null)
            {
                string guidintoscripts = mif.Name;
                    // get the name of the chosen contextual menu, used as key into scripts
                var scriptingEngine = CompositionManager.Get<IScriptingEngine>();
                var scriptingUiHelper = CompositionManager.Get<IScriptingUIHelper>();
                var scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

                if (guidintoscripts == null || scriptingUiHelper == null) return;
                if (scriptingUiHelper.IsEditScriptEvent(null, guidintoscripts)) return;

                var ewf = Keyboard.FocusedElement as UIElement; // this retrieves the current textbox that has selection
                if (!(ewf is StatusUpdateTextbox)) return;

                var tbwf = ewf as StatusUpdateTextbox;
                string textselected = tbwf.Selection.Text; // textual contextual menu needs the selected text
                var textreturned =
                    (string)
                    scriptingEngine.ExecuteScriptGUID(guidintoscripts,
                                                      scriptingConfiguration.StatusUpdateTextContextMenuEntryPoint,
                                                      textselected);
                if (textreturned == null) return;
                tbwf.Selection.Text = textreturned;
            }
        }

        private void StatusUpdateView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var scriptingUiHelper = CompositionManager.Get<IScriptingUIHelper>();
            var scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

            if (fe != null)
            {
                ContextMenu newcontextualmenu = fe.ContextMenu ?? new ContextMenu();

                if (fe.Tag == null && scriptingUiHelper != null)
                    // bit of a shortcut: this just avoids the problem where scripts may be updated by the app is running
                {
                    List<object> menu = scriptingUiHelper.CreateContextMenusFromScripts(newcontextualmenu.HasItems,
                                                                                        scriptingConfiguration.
                                                                                            StatusUpdateContextMenuEntryPoint,
                                                                                        mi_Click);

                    if (menu != null)
                    {
                        foreach (object i in menu)
                            newcontextualmenu.Items.Add(i);
                    }
                }
                fe.Tag = 1;
                fe.ContextMenu = newcontextualmenu;
            }
        }

        private void mi_Click(object sender, RoutedEventArgs e)
        {
            var mif = e.Source as MenuItem;
            if (mif == null) return;

            string guidintoscripts = mif.Name; // get the name of the chosen contextual menu, used as key into scripts
            var scriptingEngine = CompositionManager.Get<IScriptingEngine>();
            var scriptingUiHelper = CompositionManager.Get<IScriptingUIHelper>();
            var scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

            if (guidintoscripts == null || scriptingUiHelper == null) return;
            if (scriptingUiHelper.IsEditScriptEvent(null, guidintoscripts)) return;

            var textreturned =
                (string)
                scriptingEngine.ExecuteScriptGUID(guidintoscripts,
                                                  scriptingConfiguration.StatusUpdateContextMenuEntryPoint,
                                                  StatusUpdate.Text);
            if (textreturned == null) return;

            StatusUpdate.Text = textreturned;
        }

        private void PART_ContactName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var newEventArgs = new RoutedEventArgs(ContactNameClickedEvent);
            RaiseEvent(newEventArgs);
            MarkRead();
        }

        public void ToggleCompositionBoxContainer(String Text)
        {
            if (CompositionBoxContainer.Visibility == Visibility.Visible)
            {
                CompositionBoxContainer.Visibility = Visibility.Hidden;
                CompositionBoxContainer.Height = 0;
            }
            else
            {
                CompositionBoxContainer.Visibility = Visibility.Visible;
                CompositionBoxContainer.Height = Double.NaN;
                    // this sets the height to Auto; Auto will auto rize the reply area to fix the text ref remarks in: http://msdn.microsoft.com/en-us/library/system.windows.frameworkelement.height.aspx
                CompositionBox.FocusOnText();
                if (!String.IsNullOrEmpty(Text))
                    CompositionBox.Text = Text;
                CompositionBox.ScrollToEnd();
            }
        }
    }
}