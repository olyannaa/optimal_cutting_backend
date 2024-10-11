
using SkiaSharp;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class DrawService : IDrawService
    {
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
    }
}
