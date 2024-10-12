
using SkiaSharp;
using vega.Controllers.DTO;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class DrawService : IDrawService
    {
        public byte[] DrawDXF(List<FigureDTO> figures)
        {
            var maxX = figures.Max(f => float.Parse(f.Coorditanes.Split(';')[0]));
            var maxY = figures.Max(f => float.Parse(f.Coorditanes.Split(';')[1]));
            var minX = figures.Min(f => float.Parse(f.Coorditanes.Split(';')[0]));
            var minY = figures.Min(f => float.Parse(f.Coorditanes.Split(';')[1]));
            var width = (int)((maxX - minX) * 1.2) ;
            var height = (int)((maxY - minY)* 1.3) ;

            var bitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(bitmap);

            var blackPaint = new SKPaint();
            blackPaint.Color = SKColors.Black;

            var whitePaint = new SKPaint();
            whitePaint.Color = SKColors.White;
            whitePaint.Style = SKPaintStyle.Stroke;

            canvas.Clear(SKColors.Black);
            foreach (var figure in figures)
            {
                //line
                if (figure.TypeId == 1)
                {
                    var coorditanes = figure.Coorditanes.Split(';');
                    var startX = NormalizeXCoordinate(width, coorditanes[0]);
                    var startY = NormalizeYCoordinate(height, coorditanes[1]);
                    var endX = NormalizeXCoordinate(width, coorditanes[2]);
                    var endY = NormalizeYCoordinate(height, coorditanes[3]);
                    canvas.DrawLine(new SKPoint(startX, startY), new SKPoint(endX, endY), whitePaint);
                }
                //circle
                if (figure.TypeId == 2)
                {
                    var coorditanes = figure.Coorditanes.Split(';');
                    var centerX = NormalizeXCoordinate(width, coorditanes[0]);
                    var centerY = NormalizeYCoordinate(height, coorditanes[1]);
                    var radius = float.Parse(coorditanes[2]);
                    canvas.DrawCircle(centerX, centerY, radius, whitePaint);
                    canvas.DrawCircle(centerX, centerY, radius - 1, blackPaint);
                }
                //arc
                if (figure.TypeId == 3)
                {
                    var coorditanes = figure.Coorditanes.Split(';');
                    var centerX = NormalizeXCoordinate(width, coorditanes[0]);
                    var centerY = NormalizeYCoordinate(height, coorditanes[1]);
                    var radius = float.Parse(coorditanes[2]);
                    var startAngle = float.Parse(coorditanes[3]);
                    var endAngle = float.Parse(coorditanes[4]);
                    canvas.DrawArc(new SKRect(centerX - radius,
                        centerY - radius,
                        centerX + radius,
                        centerY + radius), startAngle, Math.Abs(endAngle+360 - startAngle)%360, false, whitePaint);
                }
            }
            var image = SKImage.FromBitmap(bitmap);
            return image.Encode().ToArray();
        }
        public byte[] Draw1DCutting(Cutting1DResult result)
        {


            var width = 800;
            var height = 500;
            var detailHeight = height/result.Workpieces.Count;
            var detailWidthCoeff = (double)width / result.Workpieces.Max(w => w.Length);

            var bitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(bitmap);
            
            var greenPaint = new SKPaint();
            var grayPaint = new SKPaint();
            var blackPaint = new SKPaint();
            var whitePaint = new SKPaint();
            greenPaint.Color = new SKColor(26, 188, 156);
            blackPaint.Color = SKColors.Black;
            whitePaint.Color = SKColors.White;
            whitePaint.TextSize = 20;
            grayPaint.Color = new SKColor(127, 140, 141);
            int x = 0;
            int y = 0;
            foreach (var workpiece in result.Workpieces)
            {
                foreach (var detailWidth in workpiece.Details)
                {
                    var newDetailWidth = (int)(detailWidth * detailWidthCoeff);
                    canvas.DrawRect(new SKRect(x,y, x + newDetailWidth, y + detailHeight), blackPaint);
                    canvas.DrawRect(new SKRect(x + 1, y + 1, x + newDetailWidth - 1, y + detailHeight - 1),
                        greenPaint);
                    canvas.DrawText(detailWidth.ToString(), x + 2, y + detailHeight / 2 + 10, whitePaint);
                    x += newDetailWidth;
                }
                canvas.DrawRect(new SKRect(x, y, (int)(workpiece.Length*detailWidthCoeff), y + detailHeight), blackPaint);
                canvas.DrawRect(new SKRect(x + 1, y + 1, (int)(workpiece.Length * detailWidthCoeff) - 1, y + detailHeight - 1), grayPaint);
                canvas.DrawText((workpiece.Length - workpiece.Details.Sum(d => d)).ToString(), x + 2, y + detailHeight / 2 + 10, whitePaint);
                x = 0;
                y += detailHeight;
            }
            var image = SKImage.FromBitmap(bitmap);
            return image.Encode().ToArray();

        }


        private float NormalizeXCoordinate(int width, string x)
        {
            return width/2 + float.Parse(x);
        }
        private float NormalizeYCoordinate(int height, string y)
        {
            return height/2 + float.Parse(y);
        }
    }
}
