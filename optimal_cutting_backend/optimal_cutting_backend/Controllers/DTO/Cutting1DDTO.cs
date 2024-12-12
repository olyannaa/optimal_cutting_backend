namespace vega.Controllers.DTO
{
    public class Cutting1DDTO
    {
        public List<Workpiece1D> Workpieces { get; set; } = new List<Workpiece1D>();
        public double TotalPercentUsage { get; set; }
    }
    public class Workpiece1D
    {
        public int Length { get; set; }
        public List<int> Details { get; set; }
        public double PercentUsage { get; set; }
    }
}
