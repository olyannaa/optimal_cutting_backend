using vega.Models;

namespace vega.Services.Interfaces
{
    public interface ICutting1DService
    {
        public Task<Cutting1DResult> CalculateCuttingAsync(List<int> details, List<int> workpiece);
    }
}
