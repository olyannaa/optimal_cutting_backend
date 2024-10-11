
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
using netDxf;
using netDxf.Entities;
using SkiaSharp;
using vega.Controllers.DTO;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class DXFService : IDXFService
    {
        

        public List<FigureDTO> GetDXF(byte[] fileBytes)
        {
            var ans = new List<FigureDTO>();
            using var stream = new MemoryStream(fileBytes);
            var dxf = DxfDocument.Load(stream);
            foreach (var obj in dxf.Entities.All)
            {
                if (obj is Circle circle)
                    ans.Add(new FigureDTO { TypeId = 2, Coorditanes = $"{circle.Center.X}; {circle.Center.Y}; {circle.Radius}" });
                if (obj is Line line)
                    ans.Add(new FigureDTO { TypeId = 1, Coorditanes = $"{line.StartPoint.X}; {line.StartPoint.Y}; {line.EndPoint.X}; {line.EndPoint.Y}" });
                if (obj is Arc arc)
                    ans.Add(new FigureDTO { TypeId = 3, Coorditanes = $"{arc.Center.X}; {arc.Center.Y}; {arc.Radius}; {arc.StartAngle}; {arc.EndAngle}" });
            }

            return ans;
        }

    }
}
