using System.Windows.Input;
using MahTweets.Core.Commands;

namespace MahTweets.Core.Extensions
{
    public static class CommandExtensions
    {
        public static void RaiseCanExecuteChanged(this ICommand command)
        {
            var cmd = command as DelegateCommand;
            if (cmd != null)
            {
                cmd.RaiseCanExecuteChanged();
            }
        }
    }
}