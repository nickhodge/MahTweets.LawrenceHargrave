using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows.Controls;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace MahTweets.Core.Scripting
{
    public class DynamicLanguagesScriptEngine : IScriptingEngine
    {
        // datastructures

        private const string ScriptingDocumentationSuffix = "_doc";
                             // look for this appended to the entry points to look for non-python documentation

        private readonly IScriptingLibrarian _scriptLibrarian;

        private readonly IScriptingConfiguration _scriptingConfiguration;
        private readonly IScriptingManager _scriptingManager;
        private List<string> _librarypaths; // paths where common script files & DLLs can be found

        // scripting elements
        private ScriptRuntime _runtime; // read the languages in app.config
        private ScopeVariables _scopevariables; // variables added to each scope for universal scripting

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public DynamicLanguagesScriptEngine()
        {
            _scriptingManager = CompositionManager.Get<IScriptingManager>();
            _scriptLibrarian = CompositionManager.Get<IScriptingLibrarian>();
            _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();
        }

        #region IScriptingEngine Members

        public void Start()
        {
            try
            {
                _runtime = ScriptRuntime.CreateFromConfiguration();
                    // this reads from app.config for configured Dynamic Language Runtime libraries

                // now get all the file extensions of scripting types, as creatted from the Configuration
                foreach (LanguageSetup ls in _runtime.Setup.LanguageSetups)
                {
                    foreach (string fext in ls.FileExtensions)
                    {
                        _scriptingManager.RegisterLanguageExtensions(this, fext);
                    }
                }

                IEnumerable<string> lg = from setup in _runtime.Setup.LanguageSetups
                                         select setup.Names[0];
                foreach (string ln in lg)
                    _scriptingManager.RegisterLanguageNames(this, ln);

                _scopevariables = new ScopeVariables();
                    // setup the initial scopevariables that are shared by all scopes
                _librarypaths = new List<string>();
                    // list of Lib directories, or others that contain standard libraries for this script collection
            }
            catch (Exception ex)
            {
                _scriptingManager.IsScriptEngineActive = false;
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }

        public void ParseScript(string fullPathToScript) // will return true only if parsed OK by the script engine
        {
            if (_scriptingManager.IsScriptEngineActive)
            {
                string extension = Path.GetExtension(fullPathToScript);
                if (extension == null) return;

                string langExtension = extension.Substring(1);
                ScriptEngine engine = _runtime.GetEngine(langExtension);
                if (engine == null) return;

                if (_librarypaths != null && engine.GetSearchPaths() != null)
                {
                    engine.SetSearchPaths(_librarypaths);
                }

                try
                {
                    ScriptFile sf = _scriptLibrarian.GetOrAddItem(fullPathToScript);

                    if (sf.RunInScope == null)
                    {
                        sf.RunInScope = engine.CreateScope();
                        AddVariablesToScope(sf.RunInScope);
                        AddContextToScope(sf.RunInScope, sf);
                    }

                    ScriptScope parseScope = sf.RunInScope;

                    try
                    {
                        engine.ExecuteFile(fullPathToScript, parseScope);
                    }
                    catch (ArgumentException argex)
                    {
                        sf.RunInScope = engine.CreateScope(); // old scope borked, create a fresh new one
                        AddVariablesToScope(sf.RunInScope);
                        AddContextToScope(sf.RunInScope, sf);
#if DEBUG
                        System.Console.WriteLine("SCRIPTING ERROR IN:" + (string)fullPathToScript);
                        System.Console.WriteLine("DEACTIVATING SCRIPT:" + (string)fullPathToScript);
                        System.Console.WriteLine(argex.Message);
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(argex);
#endif
                        engine.ExecuteFile(fullPathToScript, parseScope);
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(argex);
                    }
                    catch (SyntaxErrorException scriptError)
                    {
                        // TODO should do something moar interesting here UI wise
#if DEBUG
                        System.Console.WriteLine("SCRIPTING ERROR IN:" + (string)fullPathToScript);
                        System.Console.WriteLine("DEACTIVATING SCRIPT:" + (string)fullPathToScript);
                        System.Console.WriteLine(scriptError.Message);
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(scriptError);
#endif
                        _scriptLibrarian.DeactiveScriptFile(fullPathToScript);
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(scriptError);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        System.Console.WriteLine("SCRIPTING ERROR IN:" + (string)fullPathToScript);
                        System.Console.WriteLine("DEACTIVATING SCRIPT:" + (string)fullPathToScript);
                        System.Console.WriteLine(ex.Message);
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
#endif
                        _scriptLibrarian.DeactiveScriptFile(fullPathToScript);
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }
                    sf.AttachedScriptEngine = this;
                    foreach (string scriptEntryPoint in _scriptingConfiguration.ScriptingEntryPoints)
                    {
                        try
                        {
                            dynamic callbackscript;
                            if (!parseScope.TryGetVariable(scriptEntryPoint, out callbackscript)) continue;
                            string blurb;
                            if (langExtension == "py")
                            {
                                blurb = callbackscript.__doc__; // Nick Hodge note: is this python specific?
                            }
                            else
                            {
                                dynamic callbackdoc;
                                if (parseScope.TryGetVariable(scriptEntryPoint + ScriptingDocumentationSuffix,
                                                              out callbackdoc))
                                {
                                    blurb = (string) callbackdoc();
                                }
                                else
                                {
                                    blurb = Path.GetFileNameWithoutExtension(fullPathToScript);
                                }
                            }

                            if (sf.DynamicMethodReferences == null)
                                sf.DynamicMethodReferences = new ScriptEntryPoints();

                            sf.DynamicMethodReferences.GetOrAddItem(scriptEntryPoint, callbackscript, blurb);
                        }
                        catch (MissingMemberException me)
                        {
                            CompositionManager.Get<IExceptionReporter>().ReportHandledException(me);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                }
            }
        }

        #endregion

        #region Script Management, scope & variable tasks

        public void SetScriptRuntimeOutput(ScriptStream scripterrorstream, ScriptStream scriptconsolestream)
        {
            _runtime.IO.SetOutput(scriptconsolestream, Encoding.UTF8);
            _runtime.IO.SetErrorOutput(scripterrorstream, Encoding.UTF8);
        }

        public void AddVariableToAllScopes(string varname, object theobject)
        {
            if (_scriptingManager != null && _scriptingManager.IsScriptEngineActive)
            {
                _scopevariables.GetOrAddItem(varname, theobject);
            }
        }

        private void AddVariablesToScope(ScriptScope sc)
        {
            if (_scriptingManager.IsScriptEngineActive)
            {
                if (_scopevariables != null)
                    foreach (ScopeVariable sv in _scopevariables)
                    {
                        sc.SetVariable(sv.variableName, sv.variableObject);
                    }
            }
        }

        private void AddContextToScope(ScriptScope sc, ScriptFile sf)
            // this adds a context to the script running scope so the script can find out who called it
        {
            if (_scriptingManager != null && _scriptingManager.IsScriptEngineActive)
            {
                sc.SetVariable(_scriptingConfiguration.ScriptExecutionContext, sf);
            }
        }

        #endregion

        #region Actual DLR script execution code stuff

        public object ExecuteScript(string key, string entrypoint, object inputthing, object context)
            // main script execution method
        {
            if (_scriptingManager.IsScriptEngineActive)
            {
                if (key != null)
                {
                    dynamic result = null;
                    dynamic x = _scriptLibrarian.ScriptFromKey(key, entrypoint);
                    if (x != null)
                    {
                        try
                        {
                            if (inputthing != null && context != null)
                                result = x(inputthing, context);
                            else if (context == null)
                                result = x(inputthing);
                            else if (inputthing == null && context == null)
                                result = x();
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            Console.WriteLine("SCRIPTING ERROR IN:" + key);
                            Console.WriteLine("DEACTIVATING SCRIPT:" + key);
                            Console.WriteLine(ex.Message);
                            CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
#endif
                            CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                            _scriptLibrarian.DeactiveScriptFile(key);
                        }
                        return result;
                    }
                }
                return null;
            }
            return null;
        }

        public void ExecuteScriptNoReturn(string key, string entrypoint, object inputthing, object context)
            // signature to eat the response
        {
            // absorb the response into bits in the ether
            dynamic d = ExecuteScript(key, entrypoint, inputthing, context);
        }

        public object ExecuteScriptGUID(string guidName, string entrypoint, object inputthing)
            // used by contextmenu selections as GUID is only thing passed in
        {
            if (_scriptingManager.IsScriptEngineActive && guidName != null)
            {
                string key = _scriptLibrarian.KeyFromGUID(guidName);
                return ExecuteScript(key, entrypoint, inputthing, null);
            }
            return null;
        }

        public void ExecuteScriptFromStringWithLanguage(string scriptString, TextBox bogusSyntaxError, string lang)
            // used by ScriptConsole
        {
            if (_scriptingManager.IsScriptEngineActive)
            {
                if (scriptString != null)
                {
                    try
                    {
                        string languageToRun = lang ?? "py";
                        ScriptEngine engine = _runtime.GetEngine(languageToRun);
                        ScriptScope scope = engine.CreateScope();
                            // create a new scope for this particular script to run
                        if (_librarypaths != null && engine.GetSearchPaths() != null)
                        {
                            engine.SetSearchPaths(_librarypaths); // add any whole-of-application Library paths
                        }
                        AddVariablesToScope(scope); // add the variables to this scope
                        AddContextToScope(scope, null); // add running context to scope

                        engine.Execute(scriptString, scope);
                    }
                    catch (ArgumentException argex)
                    {
                        bogusSyntaxError.Text = argex.ToString();
                    }
                    catch (SyntaxErrorException scriptError)
                    {
                        bogusSyntaxError.Text = scriptError.ToString();
                    }
                    catch (Exception ex) //failover
                    {
                        bogusSyntaxError.Text = ex.Message;
                    }
                }
            }
        }

        #endregion
    }
}