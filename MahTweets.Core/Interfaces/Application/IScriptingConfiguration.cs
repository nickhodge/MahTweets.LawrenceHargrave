using System.Collections.Generic;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IScriptingConfiguration
    {
        List<string> ScriptingEntryPoints { get; }

        string ScriptFolderName { get; }

        string ScriptedApplication { get; }
        string ScriptedApplicationHelper { get; }
        string ScriptExecutionContext { get; }

        string StatusUpdateContextMenuEntryPoint { get; }
        string StatusUpdateTextContextMenuEntryPoint { get; }
        string ComposeTextContextMenuEntryPoint { get; }
        string ColumnContextMenuEntryPoint { get; }
        string MainWindowContextMenuEntryPoint { get; }

        string MahTweetsStartupEntryPoint { get; }
        string MahTweetsShutdownEntryPoint { get; }

        string ScriptFilterEntryPoint { get; }
        string GlobalScriptFilterEntryPoint { get; }

        string TimedEventEntryPoint { get; }
    }
}