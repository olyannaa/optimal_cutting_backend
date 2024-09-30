using System.Drawing;
using System.Drawing.Imaging;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class DrawService : IDrawService
    {
        public byte[] Draw1DCutting(Cutting1DResult result)
        {
            var lineHeight = 100;
            var bitmap = new Bitmap(result.Workpieces.Max(w => w.Length),
                result.Workpieces.Count * lineHeight + 20);
            var graphics = Graphics.FromImage(bitmap);
            var x = 0;
            var y = 10;
            var greenBrush = new SolidBrush(Color.FromArgb(26, 188, 156));
            var whiteBrush = new SolidBrush(Color.White);
            var grayBrush = new SolidBrush(Color.FromArgb(127, 140, 141));
            var font = new Font("Arial", 12, FontStyle.Italic);
            var blackPen = new Pen(Color.Black);

            foreach (var workpiece in result.Workpieces)
            {
                foreach(var detailWidth in workpiece.Details)
                {
                    
                    graphics.FillRectangle(greenBrush, new Rectangle(x, y, detailWidth, lineHeight));
                    graphics.DrawRectangle(blackPen, new Rectangle(x, y, detailWidth, lineHeight));
                    graphics.DrawString(detailWidth.ToString(), font,
                        whiteBrush, x, y + (lineHeight / 2 - 12));
                    x += detailWidth;
                }
                graphics.FillRectangle(grayBrush, new Rectangle(x, y, workpiece.Length - x, lineHeight));
                graphics.DrawRectangle(blackPen, new Rectangle(x, y, workpiece.Length - x, lineHeight));
                graphics.DrawString((workpiece.Length - x).ToString(), font,
                        whiteBrush, x, y + (lineHeight / 2 - 12));
                x = 0;
                y += lineHeight;
            }
            
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);

            byte[] imageBytes = stream.ToArray();
            return imageBytes;
        }
    }
}
