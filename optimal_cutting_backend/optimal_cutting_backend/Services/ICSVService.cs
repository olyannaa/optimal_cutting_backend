namespace vega.Services
{
    public interface ICSVService
    {
        public IEnumerable<T> ReadCSV<T>(Stream file);
        public MemoryStream WriteCSV<T>(IEnumerable<T> items);
    }
}
