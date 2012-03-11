using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MahTweets2.Commands
{
    public class ProfileContainerCommand : ICommand
    {
        private Action<object> _action;

        public ProfileContainerCommand(Action<object> action)
        {
            _action = action;
        }


        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        #endregion
    }
}
