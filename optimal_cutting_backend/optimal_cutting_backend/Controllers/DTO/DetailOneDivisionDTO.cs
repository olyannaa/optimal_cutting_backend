using CsvHelper.Configuration.Attributes;

namespace vega.Controllers.DTO
{
    public class DetailOneDivisionDTO
    {
        [Name("Length")]
        [Index(0)]
        public int Length { get; set; }
        [Name("Count")]
        [Index(1)]
        public int Count { get; set; }
    }
}
