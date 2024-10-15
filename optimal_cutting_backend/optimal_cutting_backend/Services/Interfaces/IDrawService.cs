using vega.Controllers.DTO;
using vega.Models;

namespace vega.Services.Interfaces
{
    public interface IDrawService
    {
        public Task<byte[]> DrawDXFAsync(List<FigureDTO> figures);
        public Task<byte[]> Draw1DCuttingAsync(Cutting1DResult result);
    }
}
