using vega.Migrations.DAL;

namespace vega.Models
{
    public class Cutting2DResult
    {
        public List<List<Detail2D>> Details { get; set; }
        public Workpiece Workpiece { get; set;}
    }
    
}
