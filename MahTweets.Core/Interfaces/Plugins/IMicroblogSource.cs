using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Interfaces.Plugins
{
    [InheritedExport]
    public interface IMicroblogSource : ICreator
    {
        string Name { get; }

        string Protocol { get; }

        BitmapImage Icon { get; }

        void CreateAndDisplayProfile(IContact contact);
    }

    public interface ICreator
    {
        IPlugin Create();
    }
}