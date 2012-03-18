using System.Collections.ObjectModel;
using LINQtoCSV;

namespace MahTweets.Core.Interfaces.Settings
{
    public interface IHighlighterSettings
    {
        ObservableCollection<HighlightWordsItem> HighlightWords { get; set; }
        void Add(string newAddition);
        void Remove(string deleteAddition);
        void Read();
        void Write();
    }

    public class HighlightWordsItem
    {
        [CsvColumn(Name = "TextStringToHighlight", FieldIndex = 1)]
        public string Text { get; set; }
    }

}
