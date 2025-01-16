using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Models;

namespace vega.Services.Interfaces
{
    public interface IDXFService
    {
        public Task<List<Figure>> GetDXFAsync(byte[] fileBytes);
        public Task<List<byte[]>> CreateDXFAsync(Cutting2DResult result);
        public Task<List<byte[]>> Create2DDXFAsync(Cutting2DResult result);
    }
}
