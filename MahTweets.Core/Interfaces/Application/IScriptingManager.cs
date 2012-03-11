using System;
using System.Collections.Generic;
using System.Windows.Controls;
using MahTweets.Core.Scripting;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IScriptingManager
    {
        Boolean IsScriptEngineActive { get; set; }
        IEnumerable<string> LanguageNames { get; }
        IEnumerable<string> LanguageExtensions { get; }
        void Start();

        // Parse Scripts
        void RegisterLanguageExtensions(IScriptingEngine se, String languageExtension);
        void RegisterLanguageNames(IScriptingEngine se, String languageExtension);
        void ParseScript(string fullPathToScript); // proxy to the individual engine's parse script

        object ExecuteScript(string key, string entrypoint, object inputthing, object context);
        void SetScriptRuntimeOutput(ScriptStream scripterrorstream, ScriptStream scriptconsolestream);
        void ExecuteScriptFromStringWithLanguage(string scriptString, TextBox bogusSyntaxError, string lang);
        void ExecuteScriptNoReturn(string key, string entrypoint, object input, object context);

        // probably needs to be refactored
        void AddVariableToAllScopes(string varname, object theobject);
    }
}