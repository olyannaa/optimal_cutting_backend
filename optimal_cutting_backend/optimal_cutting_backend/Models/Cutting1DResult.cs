namespace vega.Models
{
    public class Cutting1DResult
    {
        public List<Workpiece> Workpieces { get; set; } = new List<Workpiece>();
        public double TotalPercentUsage { get; set; }
    }
    public class Workpiece
    {
        public int Length { get; set; }
        public List<int> Details { get; set; }
        public double PercentUsage { get; set; }
    }
}
