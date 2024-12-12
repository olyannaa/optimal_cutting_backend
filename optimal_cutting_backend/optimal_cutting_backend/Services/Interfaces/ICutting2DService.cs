using vega.Migrations.DAL;
using vega.Models;

namespace vega.Services.Interfaces
{
    public interface ICutting2DService
    {
        public Task<Cutting2DResult> CalculateCuttingAsync(List<Detail2D> details, Workpiece workpiece, float thickness);
    }
}
