
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
using netDxf;
using netDxf.Entities;
using SkiaSharp;
using vega.DTO;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class DXFService : IDXFService
    {
        public byte[] DrawPng(List<FigureDTO> figures)
        {
            var width = 800;
            var height = 500;

            var bitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(bitmap);

            var blackPaint = new SKPaint();
            blackPaint.Color = SKColors.Black;

            var whitePaint = new SKPaint();
            whitePaint.Color = SKColors.White;

            canvas.Clear(SKColors.Black);
            foreach (var figure in figures)
            {
                if (figure.TypeId == 1)
                {
                    var coorditanes = figure.Coorditanes.Split(';');
                    var startX = NormalizeXCoordinate(coorditanes[0]);
                    var startY = NormalizeYCoordinate(coorditanes[1]);
                    var endX = NormalizeXCoordinate(coorditanes[3]);
                    var endY = NormalizeYCoordinate(coorditanes[4]);
                    canvas.DrawLine(new SKPoint(startX, startY), new SKPoint(endX, endY), whitePaint);
                }
                if(figure.TypeId == 2)
                {
                    var coorditanes = figure.Coorditanes.Split(';');
                    var centerX = NormalizeXCoordinate(coorditanes[0]);
                    var centerY = NormalizeYCoordinate(coorditanes[1]);
                    var radius = float.Parse(coorditanes[3]);
                    canvas.DrawCircle(centerX, centerY, radius, whitePaint);
                    canvas.DrawCircle(centerX, centerY, radius - 1, blackPaint);
                }
                if (figure.TypeId == 3)
                {
                    var coorditanes = figure.Coorditanes.Split(';');
                    var centerX = NormalizeXCoordinate(coorditanes[0]);
                    var centerY = NormalizeYCoordinate(coorditanes[1]);
                    var radius = float.Parse(coorditanes[3]);
                    var startAngle = float.Parse(coorditanes[4]);
                    var endAngle = float.Parse(coorditanes[5]);
                    canvas.DrawArc(new SKRect(centerX - radius,
                        centerY + radius,
                        centerX + radius,
                        centerY - radius), startAngle, (360 - endAngle), true, whitePaint);

                }
            }
            var image = SKImage.FromBitmap(bitmap);
            return image.Encode().ToArray();
        }

        public List<FigureDTO> GetDXF(byte[] fileBytes)
        {
            var ans = new List<FigureDTO>();
            using var stream = new MemoryStream(fileBytes);
            var dxf = DxfDocument.Load(stream);
            foreach (var obj in dxf.Entities.All)
            {
                if (obj is Circle circle)
                    ans.Add(new FigureDTO { TypeId = 2, Coorditanes = $"{circle.Center}; {circle.Radius}" });
                if (obj is Line line)
                    ans.Add(new FigureDTO { TypeId = 1, Coorditanes = $"{line.StartPoint}; {line.EndPoint}" });
                if (obj is Arc arc)
                    ans.Add(new FigureDTO { TypeId = 3, Coorditanes = $"{arc.Center}; {arc.Radius}; {arc.StartAngle}; {arc.EndAngle}" });
            }

            return ans;
        }

        private float NormalizeXCoordinate(string x)
        {
            return 400 + float.Parse(x);
        }
        private float NormalizeYCoordinate(string y)
        {
            return 250 + float.Parse(y);
        }
    }
}
