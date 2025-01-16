using vega.Migrations.DAL;
using vega.Models;

namespace vega.Controllers.DTO
{
    public class Calculate2DDTO
    {
        public List<Detail2DDTO> Details { get; set; }
        public Workpiece2DDTO Workpiece { get; set; }
        public float CuttingThickness { get; set; }
    }

    public class Workpiece2DDTO
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
