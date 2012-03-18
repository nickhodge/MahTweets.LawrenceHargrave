using System.Collections.ObjectModel;
using LINQtoCSV;

namespace MahTweets.Core.Interfaces.Settings
{
    public interface IGlobalExcludeSettings
    {
        ObservableCollection<GlobalExcludeItem> GlobalExcludeItems { get; set; }
        void Add(string newAddition);
        void Remove(string deleteAddition);
        void Read();
        void Write();
    }

    public class GlobalExcludeItem
    {
        [CsvColumn(Name = "TextStringToExclude", FieldIndex = 1)]
        public string Text { get; set; }
    }

}
