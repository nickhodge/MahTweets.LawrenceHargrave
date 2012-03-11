using System;
using System.Windows.Input;

namespace MahTweets2.Commands
{
    public class ActionCommand : ICommand
    {
        private Action<object> ExecuteFunc { get; set; }
        private Func<object, bool> CanExecuteFunc { get; set; }

        public ActionCommand(Action<object> execute)
        {
            ExecuteFunc = execute;
            CanExecuteFunc = o => true;
        }

        public ActionCommand(Func<object,bool> canExecute, Action<object> execute )
        {
            ExecuteFunc  = execute;
            CanExecuteFunc = canExecute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            ExecuteFunc(parameter);
        }

        #endregion
    }
}


