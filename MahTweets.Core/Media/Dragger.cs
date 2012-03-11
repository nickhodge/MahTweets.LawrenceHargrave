using System;
using System.IO;
using System.Text;
using System.Windows;

namespace MahTweets.Core.Media

{
    public class Dragger
    {
        public DataObject BuildDesktopInternetShortcutDragger(string filetitle, string url)
        {
            // The magic happens here thanks to: http://www.codeproject.com/KB/cs/draginternetshortcut.aspx
            byte[] title = Encoding.ASCII.GetBytes(filetitle + ".url");
            var fileGroupDescriptor = new byte[336];
            title.CopyTo(fileGroupDescriptor, 76);
            fileGroupDescriptor[0] = 0x1;
            fileGroupDescriptor[4] = 0x40;
            fileGroupDescriptor[5] = 0x80;
            fileGroupDescriptor[72] = 0x78;
            var fileGroupDescriptorStream = new MemoryStream(fileGroupDescriptor);

            byte[] urlByteArray = Encoding.ASCII.GetBytes(url);
            var urlStream = new MemoryStream(urlByteArray);

            string contents = "[InternetShortcut]" + Environment.NewLine + "URL=" + url + Environment.NewLine;
            byte[] contentsByteArray = Encoding.ASCII.GetBytes(contents);
            var contentsStream = new MemoryStream(contentsByteArray);

            var dataobj = new DataObject();
            dataobj.SetData("FileGroupDescriptor", fileGroupDescriptorStream);
            dataobj.SetData("FileContents", contentsStream);
            dataobj.SetData("UniformResourceLocator", urlStream);

            return dataobj;
        }
    }
}