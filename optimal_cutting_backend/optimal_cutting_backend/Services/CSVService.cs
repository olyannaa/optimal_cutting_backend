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

            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<T>();
            return records;
        }
    }
}
