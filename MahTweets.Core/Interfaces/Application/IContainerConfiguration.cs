using System.Collections.Generic;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IContainerConfiguration
    {
        string ContainerType { get; set; }
        string Title { get; set; }
        string Uuid { get; set; }
        int Position { get; set; }
        double Width { get; set; }
        IDictionary<string, string> Mapping { get; set; }
    }
}