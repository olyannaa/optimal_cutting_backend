using vega.Migrations.DAL;
using vega.Models;

namespace vega.Controllers.DTO
{
    public class Calculate2DDTO
    {
        public List<Detail2DDTO> Details { get; set; }
        public Workpiece2D Workpiece { get; set; }
        public float CuttingThickness { get; set; }
    }

    public class Workpiece2D
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
