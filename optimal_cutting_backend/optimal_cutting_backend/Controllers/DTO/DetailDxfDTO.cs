using CsvHelper.Configuration.Attributes;

namespace vega.Controllers.DTO
{
    public class DetailDxfDTO
    {
        [Name("Id")]
        [Index(0)]
        public int Id { get; set; }
        [Name("Designation")]
        [Index(1)]
        public string Designation { get; set; }
        [Name("Count")]
        [Index(2)]
        public int Count { get; set; }
        [Name("MaterialId")]
        [Index(3)]
        public int MaterialId { get; set; }
        [Name("Thickness")]
        [Index(4)]
        public int Thickness { get; set; }
    }
}
