using vega.Models;

namespace vega.Services.Interfaces
{
    public interface ICutting1DService
    {
        public Cutting1DResult CalculateCutting(List<int> details, int workpiece);
    }
}
