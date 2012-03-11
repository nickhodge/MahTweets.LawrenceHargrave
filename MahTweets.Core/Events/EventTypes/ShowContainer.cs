using MahTweets.Core.ViewModels;

namespace MahTweets.Core.Events.EventTypes
{
    public class ShowContainer : CompositePresentationEvent<ShowContainerPayload>
    {
    }

    public class ShowContainerPayload
    {
        public ShowContainerPayload(ContainerViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public ContainerViewModel ViewModel { get; private set; }
    }
}