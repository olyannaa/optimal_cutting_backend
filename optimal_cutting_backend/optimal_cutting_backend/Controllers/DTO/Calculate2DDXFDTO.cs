using vega.Migrations.DAL;
using vega.Models;

namespace vega.Controllers.DTO
{
    public class Calculate2DDXFDTO
    {
        public List<int> DetailsId { get; set; }
        public Workpiece2D Workpiece { get; set; }
        public float CuttingThickness { get; set; }
    }
}
