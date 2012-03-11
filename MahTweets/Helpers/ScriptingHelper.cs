using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using MahTweets.Core;
using MahTweets.Core.Collections;
using MahTweets.Core.Composition;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.ViewModels;
using MahTweets.Core.Scripting;
using MahTweets.UI.Controls;
using MahTweets.ViewModels;

namespace MahTweets.Helpers
{
    public class ScriptingHelper : IScriptingHelper
    {
        /*      Sometimes, doing scripting with long, convoluted lines of code can be daunting to the average scripter. It sometimes requires
         *      knowledge of the inner workings of the application.
         *      Instead, this is a little helper class to aid script implementations and provides a means to separate the Script from the Model.
         *      So you could say, this is S-V-VM-M development. Say that drunk.
         */

        private readonly Timer _scriptingHeartbeat;
        private IScriptingConfiguration _scriptingConfiguration;
        private IScriptingManager _scriptingManager;

        public ScriptingHelper()
        {
            _scriptingHeartbeat = new Timer();
        }

        [ImportMany(typeof (ISearchProvider), AllowRecomposition = true)]
        public ThreadSafeObservableCollection<ISearchProvider> AvailableSearchProviders { get; set; }

        public AppViewModel mtapp { get; private set; }
        public IMainViewModel mtmvm { get; private set; }

        #region IScriptingHelper Members

        public void Start()
        {
            _scriptingManager = CompositionManager.Get<IScriptingManager>();
            _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

            mtapp = CompositionManager.Get<AppViewModel>();
            mtmvm = mtapp.Model;
            _scriptingManager.AddVariableToAllScopes(_scriptingConfiguration.ScriptedApplication, mtmvm);
                // set a variable called 'MahTweets' pointing to the MainWindow, via static mechanism
            _scriptingManager.AddVariableToAllScopes(_scriptingConfiguration.ScriptedApplicationHelper, this);
                // set a variable called 'ScriptingHelper' pointing to this singleton method
            StartScriptingHeartbeat();
        }

        #endregion

        #region private methods, not for export to external scripts, please. Thanks.

        private bool IsWired
        {
            get { return mtapp != null && mtmvm != null; }
        }

        // Start the heartbeat for callbacks into 
        private void StartScriptingHeartbeat()
        {
            _scriptingHeartbeat.Stop();
            _scriptingHeartbeat.Elapsed += (o, args) => ExecutePeriodicScripts();
            _scriptingHeartbeat.Interval = 1000 * 5; // milliseconds x seconds
            _scriptingHeartbeat.Start();
        }

        // Periodic heartbeat
        private static void ExecutePeriodicScripts()
        {
            var _scriptingManager = CompositionManager.Get<IScriptingManager>();
            if (!_scriptingManager.IsScriptEngineActive) return;
            var _scriptingLibrarian = CompositionManager.Get<IScriptingLibrarian>();
            var _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();

            if (_scriptingLibrarian.CountScriptEntryPoints(_scriptingConfiguration.TimedEventEntryPoint) <= 0) return;

            foreach (
                ScriptFile sf in _scriptingLibrarian.ScriptEntryPoints(_scriptingConfiguration.TimedEventEntryPoint))
            {
                ScriptFile sf1 = sf;
                Task.Run(
                    () =>
                    _scriptingManager.ExecuteScriptNoReturn(sf1.LongPath, _scriptingConfiguration.TimedEventEntryPoint,
                                                            null, null));
            }
        }

        private static Color? CreateColour(string colourAsHex)
        {
            var cc = new ColorConverter();
            object convertFrom = cc.ConvertFrom(colourAsHex);
            if (convertFrom != null) return (Color) convertFrom;
            return null;
        }

        private Contact FindOrCreateContact(string contactname, IMicroblogSource im)
        {
            if (!IsWired) return null;

            IContactService icm = mtmvm.ContactService;
            return icm.GetOrCreate(contactname, im);
        }

        private IMicroblog FindMicroblog(string microblogname)
        {
            IMicroblog retim = null;
            if (IsWired && microblogname != null)
            {
                Collection<IMicroblog> icm = mtmvm.SelectedMicroblogs;

                retim = (from mblogs in icm
                         where (mblogs.Name).ToLower() == TrimText(microblogname).ToLower()
                         select mblogs).FirstOrDefault();
            }
            return retim;
        }

        /*
                private UpdateType FindUpdateType(string updatetype, IMicroblog im)
                {
                    UpdateType ut = new UpdateType();

                    return ut;
                }
        */

        private static ScriptFilter FindScriptFilter(string sfname)
        {
            var _scriptingLibrarian = CompositionManager.Get<IScriptingLibrarian>();
            var _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();
            if (!_scriptingLibrarian.IsAScript(sfname, _scriptingConfiguration.ScriptFilterEntryPoint))
                return null;

            return _scriptingLibrarian.GetOrAddScriptFilter(sfname);
        }

        #endregion

        #region These methods are exposed to scripts via the object named "ScriptingHelper" as per MasterScriptingStartup method in AppViewModel startup

        // not quite how I am going to use this yet, but it is in there for future purposes
        public string MahTweetsVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            if (fvi.FileVersion != null) return fvi.FileVersion.ToString(CultureInfo.InvariantCulture);
            return null;
        }

        // path to executing assembly, from which many things can be found
        public string ExecutingPath()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(asm.Location);
        }

        // path to the scripts directory.
        public string ScriptsPath()
        {
            if (_scriptingConfiguration != null)
                return ExecutingPath() + Path.DirectorySeparatorChar + _scriptingConfiguration.ScriptFolderName +
                       Path.DirectorySeparatorChar;
            return null;
        }

        // given some text, change the current selected text. If there is any text actually selected.
        public void ChangeSelectedText(string texttosub)
        {
            var ewf = Keyboard.FocusedElement as UIElement; // this retrieves the current textbox that has selection
            if (ewf is StatusUpdateTextbox) // if it is a textbox
            {
                var tbwf = ewf as StatusUpdateTextbox;

                if (texttosub != null)
                {
                    tbwf.Selection.Text = texttosub;
                }
            }
            if (ewf is CompositionTextBox) // if it is a composition textbox
            {
                var tbwf = ewf as CompositionTextBox;
                if (texttosub != null)
                {
                    tbwf.Selection.Text = texttosub;
                }
            }
        }

        // given Xaml, load as window and return the window
        public Window LoadWindow(string pathToXaml)
        {
            Window w = null;
            if (File.Exists(pathToXaml))
            {
                try
                {
                    using (var fs = new FileStream(pathToXaml, FileMode.Open, FileAccess.Read))
                    {
                        w = (Window) XamlReader.Load(fs);
                        // add calcs for window positioning
                        var mw = mtmvm.View as Window;
                        if (mw != null)
                        {
                            w.Left = mw.Left + ((mw.Width - w.Width)/2);
                            w.Top = mw.Top + ((mw.Height - w.Height)/3);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                }
            }
            return w;
        }

        // given some text, remove any trailing spaces & leading '@' symbols to leave plain text
        public string TrimText(string inputtext)
        {
            string rettext = inputtext.Trim(); // remove spaces
            rettext = rettext.TrimStart('@'); // remove at-sign
            return rettext;
        }

        // pass in string which is hopefully a name, return the first found name, no matter the microblog
        public IContact FindContact(string contactname)
        {
            IContact retcontact = null;

            if (IsWired)
            {
                Collection<IContact> icr = mtmvm.ContactService.ContactsList;

                retcontact = (from cntcts in icr
                              where (cntcts.Name).ToLower() == TrimText(contactname).ToLower()
                              select cntcts).FirstOrDefault();
            }
            return retcontact;
        }

        // pass in string with is hopefully a name, within a microblog
        public IContact FindContact(string contactname, string microblogname)
        {
            IContact retcontact = null;

            if (IsWired)
            {
                Collection<IContact> icr = mtmvm.ContactService.ContactsList;

                retcontact = (from cntcts in icr
                              where (cntcts.Name).ToLower() == TrimText(contactname).ToLower() &&
                                    (cntcts.Source.Name).ToLower() == microblogname.ToLower()
                              select cntcts).FirstOrDefault();
            }
            return retcontact;
        }

        //pass in stream name, return true if exists
        public bool DoesStreamExist(string streamname)
        {
            if (IsWired)
            {
                Collection<StreamModel> sc = mtmvm.FilterGroups;
                return sc.Any(sm => sm.GroupName.ToLower() == streamname.ToLower());
            }
            return false;
        }

        //pass in stream name, return the stream
        public StreamModel FindStream(string streamname)
        {
            if (IsWired)
            {
                Collection<StreamModel> sc = mtmvm.FilterGroups;
                return sc.FirstOrDefault(smi => smi.GroupName.ToLower() == streamname.ToLower());
            }

            return null;
        }

        public StreamViewModel FindStreamView(string streamviewname)
        {
            if (IsWired)
            {
                IEnumerable<StreamViewModel> sc = mtmvm.StreamContainers.OfType<StreamViewModel>();
                return
                    sc.FirstOrDefault(
                        sci => sci.StreamConfiguration.Filters.GroupName.ToLower() == streamviewname.ToLower());
            }
            return null;
        }

        // to the outside world, this looks like you are creating a new column in MahTweets, but its only part of the story
        public StreamModel CreateStream(string streamname)
        {
            if (!IsWired) return null;

            return new StreamModel {SelectAll = false, GroupName = streamname};
        }

        // given a StreamModel, probably as created above, show it now. It is actually a little more complex, but it will work to the script
        public void ShowStream(StreamModel sm)
        {
            if (!IsWired) return;

            mtmvm.CreateStream(sm, sm.GroupName);
        }

        public void EmptyContactFiltersInStreamView(StreamViewModel svm)
        {
            if (!IsWired) return;
            svm.StreamConfiguration.Filters.ClearAllContactFilters();
            svm.StreamConfiguration.SaveChanges();
        }

        // given a contact, add/exclude it to the filters of a given streamviewmodel
        public void CreateContactFilterInStream(string contactname, string microblogname, string colour, string fb,
                                                StreamModel sm)
        {
            if (!IsWired) return;

            if (FindMicroblog(microblogname) != null && sm != null)
            {
                IMicroblog im = FindMicroblog(microblogname);
                if (im != null)
                {
                    Contact c = FindOrCreateContact(contactname, im.Source);
                    if (c != null)
                    {
                        int fbi = Convert.ToInt32(fb);

                        if (fbi == (int) FilterBehaviour.Include)
                        {
                            Color? color = CreateColour(colour);
                            if (color != null) sm.Color(c, (Color) color);
                        }
                        if (fbi == (int) FilterBehaviour.Exclude)
                            sm.Exclude(c);
                    }
                }
            }
        }

        // given a microblog & updatetype, add/exclude it to the filters of a given streamviewmodel
        public void CreateUpdateFilterInStream(string accountname, string updatetype, string microblogname,
                                               string colour, string fb, StreamModel sm)
        {
            // signature only at the moment
        }

        // given a script filter, add/exclude it to the filters of a given streamviewmodel
        public void CreateScriptFilterInStream(string scriptfilt, string scriptfiltcolour, StreamModel sm)
        {
            if (!IsWired) return;

            ScriptFilter sf = FindScriptFilter(scriptfilt);

            if (sf == null) return;

            sm.AddActiveScriptFilter(sf);
        }

//        public void CreateSearch(string searchText)
//        {
//            if (!IsWired) return;
//            mtmvm.CreateNewSearch(searchText);
//        }

        #endregion
    }
}