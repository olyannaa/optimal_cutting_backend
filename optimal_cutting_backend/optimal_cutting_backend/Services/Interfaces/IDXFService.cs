using vega.Controllers.DTO;

namespace vega.Services.Interfaces
{
    public interface IDXFService
    {
        public Task<List<FigureDTO>> GetDXFAsync(byte[] fileBytes);
    }
}
