using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Scripting
{
    public class ScriptingManager : IScriptingManager
    {
        private readonly List<IScriptingEngine> _scriptEngines = new List<IScriptingEngine>();
        private readonly ScriptEngineRegister _scriptExtensionRegister = new ScriptEngineRegister();
        private readonly ScriptEngineRegister _scriptLanguageRegister = new ScriptEngineRegister();
        private IScriptingLibrarian _scriptLibrarian;

        private bool _scriptengineActive;

        public IScriptingLibrarian ScriptLibrarian
        {
            get { return _scriptLibrarian; }
            set { _scriptLibrarian = value; }
        }

        #region IScriptingManager Members

        public void Start()
        {
            _scriptengineActive = true;
            _scriptLibrarian = CompositionManager.Get<IScriptingLibrarian>();
            var scriptingHelper = CompositionManager.Get<IScriptingHelper>();
            var dlr = new DynamicLanguagesScriptEngine();
            dlr.Start();
            _scriptEngines.Add(dlr);
            scriptingHelper.Start();
            _scriptLibrarian.Start();
        }

        public bool IsScriptEngineActive
        {
            get { return _scriptengineActive; }
            set { _scriptengineActive = value; }
        }

        public void ParseScript(string fullpathtoscript)
        {
            var extension = Path.GetExtension(fullpathtoscript);
            if (extension == null) return;
            string fsExt = extension.Substring(1);
            var fsEng = _scriptExtensionRegister.EngineFromString(fsExt);
            if (fsEng != null)
            {
                fsEng.ParseScript(fullpathtoscript);
            }
        }

        public object ExecuteScript(string key, string entrypoint, object inputthing, object context)
        {
            if (!_scriptLibrarian.ScriptCollection.Contains(key)) return null;
            var sf = _scriptLibrarian.ScriptCollection[key];
            if (sf != null)
            {
                return sf.AttachedScriptEngine.ExecuteScript(key, entrypoint, inputthing, context);
            }
            return null;
        }

        public void SetScriptRuntimeOutput(ScriptStream scripterrorstream, ScriptStream scriptconsolestream)
        {
            // hack
            foreach (var eng in _scriptEngines)
                eng.SetScriptRuntimeOutput(scripterrorstream, scriptconsolestream);
        }

        public void ExecuteScriptFromStringWithLanguage(string scriptString, TextBox bogusSyntaxError, string lang)
        {
            var fsEng = _scriptLanguageRegister.EngineFromString(lang);
            if (fsEng != null)
            {
                fsEng.ExecuteScriptFromStringWithLanguage(scriptString, bogusSyntaxError, lang);
            }
        }

        public void ExecuteScriptNoReturn(string key, string entrypoint, object inputthing, object context)
        {
            var sf = _scriptLibrarian.ScriptCollection[key];
            if (sf != null)
            {
                sf.AttachedScriptEngine.ExecuteScriptNoReturn(key, entrypoint, inputthing, context);
            }
        }

        public void AddVariableToAllScopes(string varname, object theobject)
        {
            foreach (var eng in _scriptEngines)
                eng.AddVariableToAllScopes(varname, theobject);
        }

        public void RegisterLanguageExtensions(IScriptingEngine se, String languageExtension)
        {
            _scriptExtensionRegister.Add(new Tuple<string, IScriptingEngine>(languageExtension.Substring(1), se));
        }

        public void RegisterLanguageNames(IScriptingEngine se, String languageName)
        {
            _scriptLanguageRegister.Add(new Tuple<string, IScriptingEngine>(languageName, se));
        }

        public IEnumerable<string> LanguageNames
        {
            get
            {
                return from ln in _scriptLanguageRegister
                       select ln.Item1;
            }
        }

        public IEnumerable<string> LanguageExtensions
        {
            get
            {
                return from le in _scriptExtensionRegister
                       select le.Item1;
            }
        }

        #endregion
    }

    public class ScriptEngineRegister : List<Tuple<string, IScriptingEngine>>
    {
        public IScriptingEngine EngineFromString(string extension)
        {
            var fSE = (from x in this
                       where x.Item1 == extension
                       select x.Item2).FirstOrDefault<IScriptingEngine>();
            return fSE;
        }
    }
}