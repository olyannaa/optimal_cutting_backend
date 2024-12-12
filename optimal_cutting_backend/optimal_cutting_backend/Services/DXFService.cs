
using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.IO;
using SkiaSharp;
using System.Text;
using vega.Controllers.DTO;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class DXFService : IDXFService
    {
        public async Task<List<byte[]>> Create2DDXFAsync(Cutting2DResult result)
        {
            
            var answer = new List<byte[]>();
            foreach (var item in result.Details)
            {
                var dxf = new DxfDocument();
                foreach(var detail in item)
                {
                    dxf.Entities.Add(new Line(new Vector2(detail.X, detail.Y), new Vector2(detail.X + detail.Width, detail.Y)));
                    dxf.Entities.Add(new Line(new Vector2(detail.X, detail.Y), new Vector2(detail.X, detail.Y + detail.Height)));
                    dxf.Entities.Add(new Line(new Vector2(detail.X + detail.Width, detail.Y), new Vector2(detail.X + detail.Width, detail.Y + detail.Height)));
                    dxf.Entities.Add(new Line(new Vector2(detail.X, detail.Y + detail.Height), new Vector2(detail.X + detail.Width, detail.Y + detail.Height)));
                }
                using (MemoryStream stream = new MemoryStream())
                {
                    dxf.Save(stream);
                    byte[] dxfBytes = stream.ToArray();
                    answer.Add(dxfBytes);
                }
            }
            return answer;

        }

        public async Task<List<FigureDTO>> GetDXFAsync(byte[] fileBytes)
        {
            var ans = new List<FigureDTO>();
            using var stream = new MemoryStream(fileBytes);
            var dxf = DxfDocument.Load(stream);
            foreach (var obj in dxf.Entities.All)
            {
                if (obj is Line line)
                    ans.Add(new FigureDTO { TypeId = 1, Coorditanes = $"{line.StartPoint.X}; {line.StartPoint.Y}; {line.EndPoint.X}; {line.EndPoint.Y}" });
                if (obj is Circle circle)
                    ans.Add(new FigureDTO { TypeId = 2, Coorditanes = $"{circle.Center.X}; {circle.Center.Y}; {circle.Radius}" });
                if (obj is Arc arc)
                    ans.Add(new FigureDTO { TypeId = 3, Coorditanes = $"{arc.Center.X}; {arc.Center.Y}; {arc.Radius}; {arc.StartAngle}; {arc.EndAngle}" });
                if(obj is Spline spline)
                {
                    var coordinates = new StringBuilder();
                    foreach (var item in spline.ControlPoints)
                        coordinates.Append($"{item.ToString()}/");
                    ans.Add(new FigureDTO { TypeId = 4, Coorditanes = coordinates.ToString() });
                }
                    
            }
            return ans;
        }

    }
}
