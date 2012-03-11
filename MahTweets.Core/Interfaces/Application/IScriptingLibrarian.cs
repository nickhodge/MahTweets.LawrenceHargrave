using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MahTweets.Core.Scripting;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IScriptingLibrarian
    {
        Boolean IsScriptEngineActive { get; } //shims over to the IScriptingManager

        KeyedCollection<string, ScriptFile> ScriptCollection { get; }
        IEnumerable<ScriptFile> ScriptFiles { get; }
        ScriptDirectories WatchedScriptDirectories { get; }
        IEnumerable<ScriptFilter> ScriptFilters { get; }

        ScriptListChangedEventHandler ScriptListChanged { get; set; }
        void Start();
        ScriptFile GetOrAddItem(string fp);

        void AddDirectoryToWatch(string pathToWatch, Boolean createOnAdd);
        IEnumerable<ScriptFile> ScriptEntryPoints(String entrypointname);
        int CountScriptEntryPoints(String entrypointname);
        ScriptFilter GetOrAddScriptFilter(string scriptkey);
        string KeyFromGUID(string guidName);
        dynamic ScriptFromKey(string key, string entrypoint);
        bool IsAScript(string scriptkey, string entrypointtype);
        bool HasScript(string sk);
        void DeactiveScriptFile(string sk);
    }
}