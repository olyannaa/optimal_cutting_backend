using CsvHelper.Configuration.Attributes;

namespace vega.Controllers.DTO
{
    public class Detail2DDTO
    {
        [Name("Width")]
        [Index(0)]
        public int Width { get; set; }
        [Name("Height")]
        [Index(1)]
        public int Height { get; set; }
        [Name("Count")]
        [Index(2)]
        public int Count { get; set; }
    }
}
