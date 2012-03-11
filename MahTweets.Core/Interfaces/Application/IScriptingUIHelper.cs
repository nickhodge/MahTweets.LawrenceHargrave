using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MahTweets.Core.Scripting;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IScriptingUIHelper
    {
        ScriptConsole ScriptConsoleView { get; set; }

        List<object> CreateContextMenusFromScripts(Boolean addSeparator, string scriptEntryPoint,
                                                   RoutedEventHandler miClick);

        void createCCPContextualMenus(ContextMenu newcontextualmenu, RoutedEventHandler routeCCPHere);

        void createCCPContextualMenus(ContextMenu newcontextualmenu, RoutedEventHandler routeCCPHere, bool incCut,
                                      bool incCopy, bool incPaste);

        void IsEditScriptEvent(string scriptkey, string guidintoscripts, bool editalways);
        Boolean IsEditScriptEvent(string scriptkey, string guidintoscripts);
        void EditScriptFromKey(string sk);
        void ShowDirectoryFromKey(string sk);
    }
}