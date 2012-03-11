using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Scripting
{
    public class ScriptingUIHelper : IScriptingUIHelper
    {
        #region IScriptingUIHelper Members

        public ScriptConsole ScriptConsoleView { get; set; }

        public List<object> CreateContextMenusFromScripts(Boolean addSeparator, string scriptEntryPoint,
                                                          RoutedEventHandler miClick)
        {
            var scriptmanager = CompositionManager.Get<IScriptingManager>();
            var scriptlibrary = CompositionManager.Get<IScriptingLibrarian>();

            if (scriptmanager == null)
                return null;

            if (!scriptmanager.IsScriptEngineActive)
                return null;

            var ctm = new List<object>();

            if (scriptlibrary.CountScriptEntryPoints(scriptEntryPoint) > 0 && addSeparator)
                ctm.Add(new Separator());

            foreach (ScriptFile sf in scriptlibrary.ScriptEntryPoints(scriptEntryPoint))
                // is this kosher? I am not sure I am separating concerns correctly here
            {
                var mi = new MenuItem
                             {
                                 Header = sf.ContextMenuDescription(scriptEntryPoint),
                                 Name = sf.UIWiringGUID,
                                 FontStyle = FontStyles.Italic
                             };
                mi.Click += miClick;
                ctm.Add(mi);
            }
            return ctm;
        }

        public void createCCPContextualMenus(ContextMenu newcontextualmenu, RoutedEventHandler routeCCPHere)
        {
            createCCPContextualMenus(newcontextualmenu, routeCCPHere, true, true, true);
        }

        public void createCCPContextualMenus(ContextMenu newcontextualmenu, RoutedEventHandler routeCCPHere, bool incCut,
                                             bool incCopy, bool incPaste)
        {
            if (incCut)
            {
                var sp1 = new MenuItem {Header = "Cut"};
                sp1.Click += routeCCPHere;
                sp1.InputGestureText = "Ctrl-X";
                sp1.Name = "Cut";
                newcontextualmenu.Items.Add(sp1);
            }
            if (incCopy)
            {
                var sp2 = new MenuItem {Header = "Copy"};
                sp2.Click += routeCCPHere;
                sp2.InputGestureText = "Ctrl-C";
                sp2.Name = "Copy";
                newcontextualmenu.Items.Add(sp2);
            }
            if (incPaste)
            {
                var sp3 = new MenuItem {Header = "Paste"};
                sp3.Click += routeCCPHere;
                sp3.InputGestureText = "Ctrl-V";
                sp3.Name = "Paste";
                newcontextualmenu.Items.Add(sp3);
            }
        }

        public void IsEditScriptEvent(string scriptkey, string guidintoscripts, bool editalways)
        {
            if (editalways)
            {
                string keyintoscripts;
                var scriptlibrary = CompositionManager.Get<IScriptingLibrarian>();
                keyintoscripts = guidintoscripts != null ? scriptlibrary.KeyFromGUID(guidintoscripts) : scriptkey;
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    ShowDirectoryFromKey(keyintoscripts);
                }
                else
                {
                    EditScriptFromKey(keyintoscripts);
                }
            }
        }

        public Boolean IsEditScriptEvent(string scriptkey, string guidintoscripts)
        {
            string keyintoscripts;
            var scriptlibrary = CompositionManager.Get<IScriptingLibrarian>();

            keyintoscripts = guidintoscripts != null ? scriptlibrary.KeyFromGUID(guidintoscripts) : scriptkey;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                EditScriptFromKey(keyintoscripts);
                return true;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ShowDirectoryFromKey(keyintoscripts);
                return true;
            }
            return false;
        }

        public void EditScriptFromKey(string sk)
        {
            var si = new ProcessStartInfo();
            si.UseShellExecute = true;
            si.FileName = sk;
            si.Verb = "open";
            try
            {
                Process.Start(si);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        public void ShowDirectoryFromKey(string sk)
        {
            String spath;
            spath = Directory.Exists(sk) ? sk : Path.GetDirectoryName(sk);
            try
            {
                Process.Start("explorer.exe", spath);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        #endregion
    }

    #region Scripting output redirection elements

    public class ScriptStream : MemoryStream
        // use a memory stream as the catcher for console style output; and wire this to a WPF TextBox to display the output via append
        // reference from IronPython In Action, pg 406
    {
        private readonly TextBox _output;

        public ScriptStream(TextBox textbox)
        {
            _output = textbox;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            string text = Encoding.UTF8.GetString(buffer, offset, count);
            try
            {
                _output.AppendText(text);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            base.Write(buffer, offset, count);
        }
    }

    #endregion
}