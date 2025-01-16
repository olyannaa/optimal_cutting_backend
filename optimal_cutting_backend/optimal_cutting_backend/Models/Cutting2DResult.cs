using vega.Controllers.DTO;
using vega.Migrations.DAL;

namespace vega.Models
{
    public class Cutting2DResult
    {
        public List<Workpiece2D> Workpieces { get; set;}
        public double TotalPercentUsage { get; set;}
    }

    public class Workpiece2D
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Detail2D> Details { get; set; }
        public double ProcentUsage { get; set; }
    }

}
