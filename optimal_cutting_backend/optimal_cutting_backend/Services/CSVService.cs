using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace vega.Services
{
    public class CSVService : ICSVService
    {
        public IEnumerable<T> ReadCSV<T>(Stream file)
        {
            var reader = new StreamReader(file);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", HasHeaderRecord = false };
            var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<T>();
            return records;
        }
    }
}
