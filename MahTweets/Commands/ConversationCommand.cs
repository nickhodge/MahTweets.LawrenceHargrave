using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using MahTweets.Core.Collections;
using MahTweets2.Library;

namespace MahTweets2.Commands
{
    public class Conversation
    {
        public string Title { get; set;}
        public ThreadSafeObservableCollection<IStatusUpdate> Source{ get; set;}
    }

    public class ConversationCommand : ICommand
    {
        private Action<Conversation> ExecuteFunc { get; set; }
        private Func<Conversation, bool> CanExecuteFunc { get; set; }

        public ConversationCommand(Action<Conversation> execute)
        {
            ExecuteFunc = execute;
            CanExecuteFunc = o => true;
        }

        public ConversationCommand(Func<Conversation, bool> canExecute, Action<Conversation> execute)
        {
            ExecuteFunc  = execute;
            CanExecuteFunc = canExecute;
        }

        public void Execute(Conversation conversation)
        {
            ExecuteFunc(conversation);
        }

        public void Execute(object parameter)
        {
            var conversation = parameter as Conversation;
            ExecuteFunc(conversation);
        }

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;
    }
}
