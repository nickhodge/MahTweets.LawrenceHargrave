using MahTweets.Core.ViewModels;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IContainerCreator<out T> where T : ContainerViewModel
    {
        T CreateContainer(IContainerConfiguration configuration);
        bool CanCreateContainer(string type);
    }
}