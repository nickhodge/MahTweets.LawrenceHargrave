using System.Windows.Controls;
using MahTweets.Core.Scripting;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IScriptingEngine
    {
        void Start();

        // script execution (Dynamic Language Runtime specific)
        void SetScriptRuntimeOutput(ScriptStream scripterrorstream, ScriptStream scriptconsolestream);
        void ParseScript(string fullPathToScript);
        object ExecuteScriptGUID(string guidName, string entrypoint, object inputthing);
        // guid are passed around in UI elements like contextualmenus
        void ExecuteScriptNoReturn(string key, string entrypoint, object input, object context);
        // used when script is run on background thread
        dynamic ExecuteScript(string key, string entrypoint, object inputthing, object context);
        // ExecuteScriptGUID & ExecuteScriptNoReturn use this
        void ExecuteScriptFromStringWithLanguage(string scriptString, TextBox bogusSyntaxError, string lang);
        // used for ScriptConsole style scripts

        // probably needs further refactoring
        void AddVariableToAllScopes(string varname, object theobject);
    }
}