using System.Collections.ObjectModel;
using LINQtoCSV;

namespace MahTweets.Core.Interfaces.Settings
{
    public interface IAcronymSettingsProvider
    {
        ObservableCollection<AcronymMappingItem> AcronymMapping { get; set; }
        void Add(string newAcronym, string newMeaning);
        void Remove(string deleteAcronym);
        void Read();
        void Write();
    }

    public class AcronymMappingItem
    {
        [CsvColumn(Name = "Acronym", FieldIndex = 1)]
        public string Acronym { get; set; }

        [CsvColumn(Name = "Meaning", FieldIndex = 2)]
        public string Meaning { get; set; }
    }

}
