using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MahTweets.UI.Converters
{
    public class ViewSelector : DataTemplateSelector
    {
        public ViewSelector()
        {
        }

        public ViewSelector(IContainer container) : this()
        {
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return base.SelectTemplate(item, container);
        }
    }
}