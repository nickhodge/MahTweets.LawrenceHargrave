using System.Collections.Generic;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Scripting
{
    public class ScriptingConfiguration : IScriptingConfiguration
    {
        // Script folder name. this will be predicated with the long path
        private const string _ScriptFolderName = "Scripts";

        // Variables added to ScriptScopes
        private const string _ScriptedApplication = "MahTweets";
        private const string _ScriptedApplicationHelper = "ScriptingHelper";
        private const string _ScriptExecutionContext = "_mt_context";

        #region Scripting Method signatures

        // properties, UI clicks
        private const string _StatusUpdateContextMenuEntryPoint = "context_menu_click";
        private const string _StatusUpdateTextContextMenuEntryPoint = "text_select_context_menu_click";
        private const string _ComposeTextContextMenuEntryPoint = "compose_text_context_menu_click";
        private const string _ColumnContextMenuEntryPoint = "column_context_menu_click";
        private const string _MainWindowContextMenuEntryPoint = "mainwindow_context_menu_click";

        // properties, major user events
        private const string _MahTweetsStartupEntryPoint = "mahtweets_startup";
        private const string _MahTweetsShutdownEntryPoint = "mahtweets_shutdown";

        // filters
        private const string _ScriptFilterEntryPoint = "script_filter";
        private const string _GlobalScriptFilterEntryPoint = "global_script_filter";

        // properties, Timed events
        private const string _TimedEventEntryPoint = "timed_event";

        #endregion

        private readonly List<string> _scriptingEntryPoints = new List<string>
                                                                  {
                                                                      _StatusUpdateContextMenuEntryPoint,
                                                                      _StatusUpdateTextContextMenuEntryPoint,
                                                                      _ComposeTextContextMenuEntryPoint,
                                                                      _ScriptFilterEntryPoint,
                                                                      _ColumnContextMenuEntryPoint,
                                                                      _MainWindowContextMenuEntryPoint,
                                                                      _TimedEventEntryPoint,
                                                                      _MahTweetsStartupEntryPoint,
                                                                      _MahTweetsShutdownEntryPoint,
                                                                      _GlobalScriptFilterEntryPoint
                                                                  };
                                      // all the 'methods' that can be called into via scripts

        #region IScriptingConfiguration Members

        public List<string> ScriptingEntryPoints
        {
            get { return _scriptingEntryPoints; }
        }

        public string ScriptFolderName
        {
            get { return _ScriptFolderName; }
        }

        public string ScriptedApplication
        {
            get { return _ScriptedApplication; }
        }

        public string ScriptedApplicationHelper
        {
            get { return _ScriptedApplicationHelper; }
        }

        public string ScriptExecutionContext
        {
            get { return _ScriptExecutionContext; }
        }

        public string StatusUpdateContextMenuEntryPoint
        {
            get { return _StatusUpdateContextMenuEntryPoint; }
        }

        public string StatusUpdateTextContextMenuEntryPoint
        {
            get { return _StatusUpdateTextContextMenuEntryPoint; }
        }

        public string ComposeTextContextMenuEntryPoint
        {
            get { return _ComposeTextContextMenuEntryPoint; }
        }

        public string ColumnContextMenuEntryPoint
        {
            get { return _ColumnContextMenuEntryPoint; }
        }

        public string MainWindowContextMenuEntryPoint
        {
            get { return _MainWindowContextMenuEntryPoint; }
        }

        public string MahTweetsStartupEntryPoint
        {
            get { return _MahTweetsStartupEntryPoint; }
        }

        public string MahTweetsShutdownEntryPoint
        {
            get { return _MahTweetsShutdownEntryPoint; }
        }

        public string ScriptFilterEntryPoint
        {
            get { return _ScriptFilterEntryPoint; }
        }

        public string GlobalScriptFilterEntryPoint
        {
            get { return _GlobalScriptFilterEntryPoint; }
        }

        public string TimedEventEntryPoint
        {
            get { return _TimedEventEntryPoint; }
        }

        #endregion
    }
}