using System;
using System.IO;
using MahTweets.Core.Interfaces.Application;
using Microsoft.Scripting.Hosting;

namespace MahTweets.Core.Scripting
{
    /// <summary>
    /// base class for each script file entry
    /// </summary>
    public class ScriptFile
    {
        public ScriptFile(string lp, string sp)
        {
            LongPath = lp; // the longPath is the file path to the script. these are unique
            ShortName = sp; // this is the name to display in various places
            string extension = Path.GetExtension(lp);
            if (extension != null)
                LanguageExtension = extension.Substring(1);
            var dynamicMethodReferences = new ScriptEntryPoints();
            UIWiringGUID = "cm" + Guid.NewGuid().ToString().Replace("-", "");
                // create a unique ID for this element, bound now, to associated script file. add 'cm' to beginning to create a X[a]ML valid name
            AssociatedScriptFilter = null;
            ScriptActive = true;
        }

        #region properties

        public string LongPath { get; set; }
        public string ShortName { get; set; }
        public string LanguageExtension { get; set; }
        public ScriptEntryPoints DynamicMethodReferences { get; set; }
        public string UIWiringGUID { get; set; }
        public ScriptScope RunInScope { get; set; }
        public ScriptFilter AssociatedScriptFilter { get; set; }
        public ScriptDirectory ParentScriptDirectory { get; set; }
        public bool ScriptActive { get; set; }
        public IScriptingEngine AttachedScriptEngine { get; set; }

        #endregion

        #region management methods

        public string ContextMenuDescription(string cmtype)
        {
            if (DynamicMethodReferences != null)
            {
                if (DynamicMethodReferences.Contains(cmtype))
                {
                    //if (DynamicMethodReferences[cmtype].ScriptDescription != null)
                    return DynamicMethodReferences[cmtype].ScriptDescription;
                }
            }
            return null;
        }

        public Boolean HasEntryPoint(string entrypointsignature)
        {
            if (DynamicMethodReferences != null)
            {
                if (DynamicMethodReferences.Contains(entrypointsignature))
                {
                    // if (DynamicMethodReferences[entrypointsignature].DynamicMethod != null)
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region execute script frontend method

        public dynamic ExecuteScript(string entrypoint, object inputthing, object context)
        {
            return null;
        }

        #endregion

        public string Key
        {
            get { return LongPath; }
        }
    }
}