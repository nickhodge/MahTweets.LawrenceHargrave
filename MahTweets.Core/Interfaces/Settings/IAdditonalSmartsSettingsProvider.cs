using System.Collections.ObjectModel;
using LINQtoCSV;

namespace MahTweets.Core.Interfaces.Settings
{
    public interface IAdditonalSmartsSettingsProvider
    {
        ObservableCollection<AdditonalSmartsMappingItem> AdditionalSmartsMapping { get; set; }
        void Add(string newUrl, string newProcessing);
        void Remove(string deleteUrl);
        void Read();
        void Write();
    }

    public class AdditonalSmartsMappingItem
    {
        [CsvColumn(Name = "Url", FieldIndex = 1)]
        public string Url { get; set; }

        [CsvColumn(Name = "ProcessType", FieldIndex = 2)]
        public string ProcessType { get; set; }
    }

}
