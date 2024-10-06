using vega.Models;

namespace vega.Services.Interfaces
{
    public interface IDrawService
    {
        public byte[] Draw1DCutting(Cutting1DResult result);
    }
}
