using vega.Controllers.DTO;
using vega.Migrations.DAL;

namespace vega.Models
{
    public class Cutting2DResult
    {
        public List<List<Detail2D>> Details { get; set; }
        public Workpiece2D Workpiece { get; set;}
    }
    
}
