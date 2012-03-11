using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Scripting
{
    /// <summary>
    /// Interaction logic for ScriptConsole.xaml
    /// </summary>
    public partial class ScriptConsole : Window
    {
        private readonly IScriptingLibrarian _scriptlibrary;
        private readonly IScriptingManager _scriptmanager;
        private IScriptingConfiguration _scriptconfiguration;

        public ScriptConsole(Window w)
        {
            DataContext = this;
            _parent = w;
            _scriptmanager = CompositionManager.Get<IScriptingManager>();
            _scriptlibrary = CompositionManager.Get<IScriptingLibrarian>();
            _scriptconfiguration = CompositionManager.Get<IScriptingConfiguration>();
            InitializeComponent();
        }

        public IEnumerable<ScriptFile> linkedScriptCollection
        {
            get { return _scriptlibrary.ScriptFiles.OrderBy(sf => sf.ShortName); }
        }

        public IEnumerable<ScriptDirectory> linkedScriptDirectories
        {
            get { return _scriptlibrary.WatchedScriptDirectories.OrderBy(sd => sd.ShortPath); }
        }

        public IEnumerable<string> knownLanguages
        {
            get { return _scriptmanager.LanguageNames; }
        }

        private Window _parent { get; set; }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var scriptdebugout = new ScriptStream(_t_scriptdebugoutput);
            var scriptconsoleout = new ScriptStream(_t_scriptconsoleoutput);

            _scriptmanager.SetScriptRuntimeOutput(scriptdebugout, scriptconsoleout);
            _c_languages.SelectedIndex = 0; // select the first language
            Left = _parent.Left + ((_parent.Width - Width)/2);
            Top = _parent.Top + ((_parent.Height - Height)/3);
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // ScriptFile sf = sender.SelectedItems.Items[0] as ScriptFile;
            IList lsf = ((ListBox) sender).SelectedItems;
            var _scriptuihelper = CompositionManager.Get<IScriptingUIHelper>();

            if (lsf != null && _scriptuihelper != null)
            {
                var sf = lsf[0] as ScriptFile;
                if (sf != null)
                {
                    _scriptuihelper.IsEditScriptEvent(sf.LongPath, null, true);
                }
            }
        }

        private void ExecuteScript()
        {
            _t_scriptdebugoutput.Text = "";
            _t_scriptconsoleoutput.Text = "";
            var lang = (string) _c_languages.SelectedValue;
            _scriptmanager.ExecuteScriptFromStringWithLanguage(_t_scriptinput.Text, _t_scriptdebugoutput, lang);
        }

        private void execute_Click(object sender, RoutedEventArgs e)
        {
            ExecuteScript();
        }

        private void directories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IList lsd = ((ListBox) sender).SelectedItems;
            var _scriptuihelper = CompositionManager.Get<IScriptingUIHelper>();
            if (lsd != null && _scriptuihelper != null)
            {
                var sd = lsd[0] as ScriptDirectory;
                if (sd != null)
                {
                    _scriptuihelper.ShowDirectoryFromKey(sd.LongPath);
                }
            }
        }

        private void copypasta_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text))
            {
                _t_scriptinput.Text = Clipboard.GetDataObject().GetData(DataFormats.Text).ToString();
                ExecuteScript();
            }
        }

        private void Directory_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //DropListBox.Items.Clear();

                var droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                foreach (string droppedFilePath in droppedFilePaths)
                {
                    if (Directory.Exists(droppedFilePath))
                    {
                        _scriptlibrary.AddDirectoryToWatch(droppedFilePath, false);
                    }
                }
            }
        }
    }
}