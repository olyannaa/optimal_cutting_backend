using System.Text.Json.Serialization;

namespace vega.Controllers.DTO
{
    public class FigureDTO
    {
        [JsonPropertyName("type")]
        public int TypeId { get; set; }
        [JsonPropertyName("coordinates")]
        public string Coordinates { get; set; }
    }
}
