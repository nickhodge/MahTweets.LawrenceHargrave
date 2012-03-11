using System;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Scripting
{
    // TODO: this should not depend on the concrete class
    public class ScriptCollectionEventListener
    {
        private readonly dynamic _parent;
        private IScriptingLibrarian _list;

        public ScriptCollectionEventListener(dynamic parent, IScriptingLibrarian list)
        {
            _list = list;
            _parent = parent;
            _list.ScriptListChanged += ScriptListChanged;
        }

        private void ScriptListChanged(object sender, EventArgs e)
        {
            try
            {
                _parent.ScriptListChanged();
                    //marvellous use of the dynamic keyword here. Fires the event off in the parent to refresh the variable that is bound
            }
            catch (NullReferenceException me)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(me);
            }
        }

        public void Detach()
        {
            if (_list != null) _list.ScriptListChanged -= ScriptListChanged;
            _list = null;
        }
    }
}