using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MahTweets.Core.Commands;

namespace MahTweets.Commands
{
    public class SelectorSelectedCommandBehavior : CommandBehaviorBase<Selector>
    {
        public SelectorSelectedCommandBehavior(Selector selectableObject)
            : base(selectableObject)
        {
            selectableObject.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommandParameter = TargetObject.SelectedItem;
            ExecuteCommand();
        }
    }
}