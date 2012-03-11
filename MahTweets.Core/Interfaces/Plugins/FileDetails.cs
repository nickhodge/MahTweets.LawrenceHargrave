using System.IO;

namespace MahTweets.Core.Interfaces.Plugins
{
    public class FileDetails
    {
        public string UploadedUrl { get; set; }
        public FileInfo FileInfo { get; set; }
        public FileType FileType { get; set; }
        public string ContentType { get; set; }
        public string Caption { get; set; }
    }
}