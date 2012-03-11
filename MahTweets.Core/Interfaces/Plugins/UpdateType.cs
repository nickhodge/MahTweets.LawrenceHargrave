using System.ComponentModel.Composition;

namespace MahTweets.Core.Interfaces.Plugins
{
    [InheritedExport]
    public class UpdateType
    {
        public UpdateType()
        {
            SaveType = false;
        }

        public string Type { get; set; }

        public string ColorARGB { get; set; }

        public int Order { get; set; }

        public bool SaveType { get; set; }

        public IMicroblog Parent { get; set; }
    }
}