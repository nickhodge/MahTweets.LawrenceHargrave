using System;
using System.Windows.Input;
using System.Windows;

namespace MahTweets2.Library.Gui.Commands
{
    public class SendMessageCommand : RoutedCommand
    {
        private readonly Action<object, IInputElement> _func;
        private readonly Action<object> _funcObj;

        public SendMessageCommand(Action<object> func)
        {
            _funcObj = func;
        }

        public SendMessageCommand(Action<object, IInputElement> func)
        {
            _func = func;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return parameter.ToString().Length > 0;
        }
        
        public new void Execute(object parameter, IInputElement element)
        {
            if (_func != null)
                _func(parameter, element);
            else if (_funcObj != null)
                _funcObj(parameter);
        }

        #endregion
    }
}


