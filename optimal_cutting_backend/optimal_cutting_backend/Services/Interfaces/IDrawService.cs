using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Models;

namespace vega.Services.Interfaces
{
    public interface IDrawService
    {
        public Task<byte[]> DrawDXFAsync(List<Figure> figures);
        public Task<byte[]> Draw1DCuttingAsync(Cutting1DResult result);
        public Task<List<byte[]>> Draw2DCuttingAsync(Cutting2DResult result);
        public Task<List<byte[]>> DrawDXFCuttingAsync(Cutting2DResult result);
    }
}
