using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using vega.Services.Interfaces;

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

        public byte[] WriteCSV<T>(IEnumerable<T> items)
        {
            using (var stream = new MemoryStream())
            {
                using (var writeFile = new StreamWriter(stream, leaveOpen: true))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", HasHeaderRecord = false };
                    var csv = new CsvWriter(writeFile, config);
                    csv.WriteRecords(items);
                }
                stream.Position = 0;
                return stream.ToArray();
            }
        }
    }
}
