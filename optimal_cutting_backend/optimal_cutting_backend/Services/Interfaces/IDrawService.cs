using vega.Controllers.DTO;
using vega.Models;

namespace vega.Services.Interfaces
{
    public interface IDrawService
    {
        public byte[] DrawDXF(List<FigureDTO> figures);
        public byte[] Draw1DCutting(Cutting1DResult result);
    }
}
