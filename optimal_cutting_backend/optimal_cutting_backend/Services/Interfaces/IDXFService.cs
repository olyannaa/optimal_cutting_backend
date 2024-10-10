using vega.DTO;

namespace vega.Services.Interfaces
{
    public interface IDXFService
    {
        public List<FigureDTO> GetDXF(byte[] fileBytes);
        public byte[] DrawPng(List<FigureDTO> figures);
    }
}
