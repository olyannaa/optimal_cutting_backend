using vega.Models;

namespace vega.Services
{
    public interface ICutting1DService
    {
        public Cutting1DResult CalculateCutting(List<int> details, List<int> workpieces);
    }
}
