using CsvHelper.Configuration.Attributes;

namespace vega.Controllers.DTO
{
    public class DetailOneDivisionDTO
    {
        [Index(0)]
        public int Length { get; set; }
        [Index(1)]
        public int Count { get; set; }
    }
}
