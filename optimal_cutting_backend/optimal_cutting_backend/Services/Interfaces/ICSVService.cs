namespace vega.Services.Interfaces
{
    public interface ICSVService
    {
        public IEnumerable<T> ReadCSV<T>(Stream file);
        public byte[] WriteCSV<T>(IEnumerable<T> items);
    }
}
