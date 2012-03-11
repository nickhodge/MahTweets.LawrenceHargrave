using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MahTweets.Core.Composition;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Scripting
{
    public delegate void ScriptListChangedEventHandler(object sender, EventArgs e);

    #region How to: add a new idiom to MahTweets for Scripting.

    /*
    * Steps
     * 1. In ScriptingConfiguration.cs, add the 'xxxEntryPoint' as a private string
     * 2. Add the new 'xxxEntryPoint' into the List of _ScriptEntryPoints
     * 3. Add an accessor property
     * 4. in IScriptingConfiguration, add the accessor property into the Interface (get read only)
    */

    #endregion

    #region The Datastructure of Scripting

    /*
     * 1. There are a collection of ScriptDirectory held in ScriptDirectories. This is where scripts are found/parsed/added
     * 2. Each ScriptDirectory is a collection of ScriptFiles
     * 3. A ScriptFile is the workhorse. All the magics really sit right in here
     * 4. Each ScriptFile contains a collection of ScriptEntryPoints. A ScriptFile could have multiple functions
     * 5. A ScriptEntryPoint has a method (dynamic) that can be called when required
     * 6. These EntryPoints are the base elements within Scripting; and each has a Signature as described in ScriptingConfiguration eg: "script_filter"
     * 7. ScopeVariables are where universal clr objects are stored (eg: MahTweets as variable name, pointing to the MainViewModel)
    */

    #endregion

    public class ScriptLibrarian : KeyedCollection<string, ScriptFile>, IScriptingLibrarian
    {
        // related important things
        private readonly List<string> _librarypaths = new List<string>();
                                      // paths where common script files & DLLs can be found

        private readonly ScriptDirectories _pathstowatch = new ScriptDirectories();
                                           // path that this particular watcher is watching

        private IScriptingConfiguration _scriptingConfiguration;
        private IScriptingManager _scriptingManager;
        private IStorage _storage;

        // datastructures

        // scripting elements
        private Boolean _scriptengine_switch
        {
            get { return _scriptingManager.IsScriptEngineActive; }
        }

        #region IScriptingLibrarian Members

        public KeyedCollection<string, ScriptFile> ScriptCollection
        {
            get { return this; }
        }

        public ScriptListChangedEventHandler ScriptListChanged
        {
            get { return _scriptlistchanged; }
            set { _scriptlistchanged = value; }
        }

        public void Start()
        {
            _scriptingManager = CompositionManager.Get<IScriptingManager>();
            _scriptingConfiguration = CompositionManager.Get<IScriptingConfiguration>();
            _storage = CompositionManager.Get<IStorage>();
            string assemblyPath = _storage.GetApplicationFullPath();

            //setup the GlobalScriptCollection ready to roll & take over the world
            AddDirectoryToLibraryPaths(assemblyPath);
            AddDirectoryToLibraryPaths(Path.Combine(assemblyPath, "Plugins/"));

            char separator = Path.DirectorySeparatorChar;

            // Current running application / Scripts / available to directories where scripts will be located
            AddDirectoryToWatch(assemblyPath + separator + _scriptingConfiguration.ScriptFolderName + separator, false);

            // MyDocuments / MahTweets / Scripts / to available directories where scripts will be located
            string myDocumentsScripts = string.Concat(_storage.GetDocumentsFullPath(),
                                                      separator,
                                                      _scriptingConfiguration.ScriptFolderName,
                                                      separator);

            AddDirectoryToWatch(myDocumentsScripts, true);
        }

        public ScriptDirectories WatchedScriptDirectories
        {
            get { return _pathstowatch; }
        }

        public Boolean IsScriptEngineActive
        {
            get { return _scriptingManager.IsScriptEngineActive; }
        }

        public void AddDirectoryToWatch(string pathToWatch, Boolean createOnAdd)
        {
            if (_scriptengine_switch)
            {
                // firstly, create a list of script files in the path. if permitted on this particular directory
                if (Directory.Exists(pathToWatch) == false && createOnAdd)
                {
                    try
                    {
                        Directory.CreateDirectory(pathToWatch);
                    }
                    catch (Exception ex) //there are multiple types of IO exceptions, just catch general exception
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }
                }

                if (Directory.Exists(pathToWatch))
                {
                    try
                    {
                        var dir = new DirectoryInfo(pathToWatch);
                        ScriptDirectory nsd = _pathstowatch.GetOrAddItem(pathToWatch);

                        if (nsd != null)
                        {
                            IEnumerable<FileInfo> filelist = dir.EnumerateFiles();
                                // enumerate for every file type, and filter on file extension in addscript.

                            foreach (FileInfo fi in filelist)
                            {
                                AddScript(fi.FullName);
                            }

                            // now create a watcher to see changes, etc

                            ScriptDirectory directory = _pathstowatch[pathToWatch];

                            directory.Watcher = new FileSystemWatcher
                                                    {
                                                        Path = pathToWatch,
                                                        NotifyFilter =
                                                            NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                                                            NotifyFilters.FileName | NotifyFilters.DirectoryName,
                                                        Filter = ""
                                                    };
                            // watch for every file type, and filter on file extension later. Check only needed on "add" or "rename"

                            // Add event handlers.
                            directory.Watcher.Changed += OnScriptChanged;
                            directory.Watcher.Created += OnScriptCreated;
                            directory.Watcher.Deleted += OnScriptDeleted;
                            directory.Watcher.Renamed += OnScriptRenamed;

                            directory.Watcher.EnableRaisingEvents = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }
                }
            }
        }

        public IEnumerable<ScriptFile> ScriptFiles
        {
            get
            {
                if (Items != null)
                {
                    return from sf in this
                           select sf;
                }
                return null;
            }
        }

        public dynamic ScriptFromKey(string key, string entrypoint)
        {
            if (key != null)
            {
                dynamic tdyn = null;
                try
                {
                    ScriptEntryPoints tsep = (from sf in this
                                              where sf.LongPath == key && sf.ScriptActive
                                              select sf.DynamicMethodReferences).FirstOrDefault();
                    if (tsep != null)
                        tdyn = (from sep in tsep
                                where sep.EntryPoint == entrypoint
                                select sep.DynamicMethod).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                }
                return tdyn;
            }
            return null;
        }


        public IEnumerable<ScriptFile> ScriptEntryPoints(String entrypointname)
        {
            if (Items != null)
                return from cm in this
                       where cm.HasEntryPoint(entrypointname)
                       select cm;
            return null;
        }

        public int CountScriptEntryPoints(String entrypointname)
        {
            if (Items != null)
                return (from cm in this
                        where cm.HasEntryPoint(entrypointname)
                        select cm).Distinct().Count();
            return 0;
        }

        public IEnumerable<ScriptFilter> ScriptFilters
        {
            get
            {
                if (_scriptingConfiguration != null)
                {
                    if (CountScriptEntryPoints(_scriptingConfiguration.ScriptFilterEntryPoint) > 0)
                    {
                        return (from sf in this
                                where sf.HasEntryPoint(_scriptingConfiguration.ScriptFilterEntryPoint)
                                select sf).Select(fsf => GetOrAddScriptFilter(fsf.LongPath)).ToList();
                    }
                }
                return null;
            }
        }

        public ScriptFilter GetOrAddScriptFilter(string scriptkey)
        {
            if (Contains(scriptkey))
            {
                ScriptFile fsf = this[scriptkey];
                if (_scriptingConfiguration != null && fsf.HasEntryPoint(_scriptingConfiguration.ScriptFilterEntryPoint))
                {
                    if (fsf.AssociatedScriptFilter != null)
                        return fsf.AssociatedScriptFilter;
                    fsf.AssociatedScriptFilter = new ScriptFilter
                                                     {
                                                         ScriptKey = fsf.LongPath,
                                                         ScriptDescription =
                                                             fsf.DynamicMethodReferences[
                                                                 _scriptingConfiguration.ScriptFilterEntryPoint].
                                                             ScriptDescription,
                                                         ScriptFilterColor = StreamModel.DefaultColor
                                                     };
                    return fsf.AssociatedScriptFilter;
                }
            }
            return null;
        }

        public string KeyFromGUID(string guidName)
        {
            if (guidName != null)
            {
                return (from sf in this
                        where sf.UIWiringGUID == guidName
                        select sf.LongPath).FirstOrDefault();
            }

            return null;
        }

        public bool IsAScript(string scriptkey, string entrypointtype)
        {
            bool rb = false;
            if (Contains(scriptkey))
            {
                ScriptFile sf = this[scriptkey];
                if (sf != null)
                {
                    rb = sf.HasEntryPoint(entrypointtype);
                }
            }
            return rb;
        }

        public void DeactiveScriptFile(string key)
        {
            if (key != null)
            {
                this[key].ScriptActive = false;
            }
        }

        #endregion

        #region dictionary housekeeping

        public bool HasScript(string sk)
        {
            return Contains(sk);
        }

        public virtual void OnChanged(FileSystemEventArgs e)
        {
            if (ScriptListChanged != null)
                ScriptListChanged(this, EventArgs.Empty);
        }

        private void DeleteScript(string fullpath)
        {
            if (Contains(fullpath))
            {
                Remove(fullpath);
            }
        }

        private void AddScript(string fullpath, ScriptDirectory nsd)
        {
            if (Contains(fullpath))
            {
                DeleteScript(fullpath);
            }

            string extension = Path.GetExtension(fullpath);
            if (extension != null && _scriptingManager.LanguageExtensions.Contains(extension.Substring(1)))
            {
                _scriptingManager.ParseScript(fullpath);
            }
        }

        private void AddScript(string fullpath)
        {
            ScriptDirectory nnsd = _pathstowatch.GetParentDirectoryForScript(fullpath);
            if (nnsd != null)
                AddScript(fullpath, nnsd);
        }

        private void ChangeScript(string fullpath)
        {
            if (Contains(fullpath))
            {
                _scriptingManager.ParseScript(fullpath);
            }
        }

        #endregion

        #region what to do when the scripts being managed change

        public void OnScriptDeleted(object source, FileSystemEventArgs e)
        {
            DeleteScript(e.FullPath);
            OnChanged(e);
        }

        public void OnScriptCreated(object source, FileSystemEventArgs e)
        {
            AddScript(e.FullPath);
            OnChanged(e);
        }

        public void OnScriptChanged(object source, FileSystemEventArgs e)
        {
            ChangeScript(e.FullPath);
            OnChanged(e);
        }

        public void OnScriptRenamed(object source, RenamedEventArgs e)
        {
            DeleteScript(e.OldFullPath);
            AddScript(e.FullPath);
            OnChanged(e);
        }

        #endregion

        #region Base KeyedCollection management methods

        public ScriptFile GetOrAddItem(string fp)
        {
            if (Contains(fp))
            {
                return this[fp];
            }

            var sf = new ScriptFile(fp, Path.GetFileName(fp))
                         {ParentScriptDirectory = _pathstowatch.GetParentDirectoryForScript(fp)};
            Add(sf); // add this ScriptFile to this Collection
            return sf;
        }

        protected override string GetKeyForItem(ScriptFile item)
        {
            return item.LongPath;
        }

        #endregion

        public event ScriptListChangedEventHandler _scriptlistchanged;
            // event for when the underlying List of scripts changes

        private void AddDirectoryToLibraryPaths(string pathToWatch)
        {
            if (_scriptengine_switch)
            {
                if (Directory.Exists(pathToWatch))
                {
                    _librarypaths.Add(pathToWatch);
                }
            }
        }
    }
}