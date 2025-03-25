
using System.Globalization;
using SkiaSharp;
using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class DrawService : IDrawService
    {
        public static int Indent = 10;
        public List<SKColor> Colors = new List<SKColor> { new SKColor(175, 244, 158), new SKColor(128, 241, 205), new SKColor(124, 232, 237), new SKColor(158, 219, 244), new SKColor(152, 206, 255), new SKColor(180, 186, 255), new SKColor(227, 158, 244), new SKColor(255, 157, 203), new SKColor(255, 167, 172), new SKColor(244, 183, 158) };

        public async Task<byte[]> Draw1DCuttingAsync(Cutting1DResult result)
        {
            var width = result.Workpieces.Max(w => w.Details.Count()) * 100 + 10;
            var height = result.Workpieces.Count * 40 + 10;
            var detailHeight = height / result.Workpieces.Count;
            var detailWidthCoeff = (double)width / result.Workpieces.Max(w => w.Length);

            var bitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(bitmap);

            var detailColorsDict = new Dictionary<int, SKColor>();

            var detailPaint = new SKPaint();
            var grayPaint = new SKPaint();
            var blackPaint = new SKPaint();
            var textPaint = new SKPaint();
            blackPaint.Color = SKColors.Black;
            textPaint.Color = SKColors.Black;
            textPaint.TextSize = 14;
            grayPaint.Color = SKColors.LightGray;
            int x = 0;
            int y = 0;
            foreach (var workpiece in result.Workpieces)
            {
                foreach (var detailWidth in workpiece.Details)
                {
                    var key = detailWidth;
                    var rnd = new Random();
                    if (!detailColorsDict.ContainsKey(key)) detailColorsDict[key] = Colors[rnd.Next(0, Colors.Count)];
                    detailPaint.Color = detailColorsDict[key];
                    var newDetailWidth = (int)(detailWidth * detailWidthCoeff);
                    canvas.DrawRect(new SKRect(x, y, x + newDetailWidth, y + detailHeight), blackPaint);
                    canvas.DrawRect(new SKRect(x + 1, y + 1, x + newDetailWidth - 1, y + detailHeight - 1),
                        detailPaint);
                    canvas.DrawText(detailWidth.ToString(), x + 2, y + detailHeight / 2 + 10, textPaint);
                    x += newDetailWidth;
                }
                canvas.DrawRect(new SKRect(x, y, (int)(workpiece.Length * detailWidthCoeff), y + detailHeight), blackPaint);
                canvas.DrawRect(new SKRect(x + 1, y + 1, (int)(workpiece.Length * detailWidthCoeff) - 1, y + detailHeight - 1), grayPaint);
                canvas.DrawText((workpiece.Length - workpiece.Details.Sum(d => d)).ToString(), x + 2, y + detailHeight / 2 + 10, textPaint);
                x = 0;
                y += detailHeight;
            }
            var image = SKImage.FromBitmap(bitmap);
            return image.Encode().ToArray();

        }
        public async Task<List<byte[]>> Draw2DCuttingAsync(Cutting2DResult result)
        {
            var images = new List<byte[]>();
            var detailColorsDict = new Dictionary<int, SKColor>();

            var width = result.Workpieces[0].Width;
            var height = result.Workpieces[0].Height;

            var bitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(bitmap);
            canvas.Clear();
            canvas.Translate(0, height);
            canvas.Scale(1, -1);

            var detailPaint = new SKPaint();
            var blackPaint = new SKPaint();
            var textPaint = new SKPaint();
            blackPaint.Color = SKColors.Black;
            textPaint.Color = SKColors.Black;
            textPaint.TextSize = 14;
            textPaint.IsAntialias = true;

            foreach (var workpiece in result.Workpieces)
            {
                canvas.Clear(SKColors.White);
                foreach (var detail in workpiece.Details)
                {
                    var key = detail.Width * detail.Height;
                    var rnd = new Random();
                    if (!detailColorsDict.ContainsKey(key))
                        detailColorsDict[key] = Colors[rnd.Next(0, Colors.Count)];
                    detailPaint.Color = detailColorsDict[key];


                    var rect = new SKRect(detail.X, detail.Y, detail.X + detail.Width, detail.Y + detail.Height);
                    canvas.DrawRect(rect, blackPaint);
                    canvas.DrawRect(new SKRect(rect.Left + 1, rect.Top + 1, rect.Right - 1, rect.Bottom - 1), detailPaint);


                    textPaint.TextSize = 36;

                    if (detail.Width >= 40 && detail.Height >= 20)
                    {
                        var text = $"{detail.Width}x{detail.Height}";

                        if (detail.Width < 60)
                        {
                            canvas.Save();
                            canvas.RotateDegrees(-90, rect.MidX, rect.MidY);
                            canvas.DrawText(text, rect.MidX - rect.Width / 2, rect.MidY, textPaint);
                            canvas.Restore();
                        }
                        else
                        {
                            canvas.DrawText(text, detail.X + 2, detail.Y + textPaint.TextSize, textPaint);
                        }
                    }
                }
                var rotatedBitmap = RotateBitmap90(bitmap);
                var flippedBitmap = FlipBitmapVertically(rotatedBitmap);
                images.Add(SKImage.FromBitmap(flippedBitmap).Encode().ToArray());
            }

            return images;
        }

        public async Task<byte[]> DrawDXFAsync(List<Figure> figures)
        {
            var maxX = figures.Max(f => float.Parse(f.Coordinates.Split(';')[0], new CultureInfo("ru-RU")));
            var maxY = figures.Max(f => float.Parse(f.Coordinates.Split(';')[1], new CultureInfo("ru-RU")));
            var minX = figures.Min(f => float.Parse(f.Coordinates.Split(';')[0], new CultureInfo("ru-RU")));
            var minY = figures.Min(f => float.Parse(f.Coordinates.Split(';')[1], new CultureInfo("ru-RU")));
            var width = (int)((Math.Abs(minX) + maxX) * 1.2);
            var height = (int)((Math.Abs(minY) + maxY) * 1.2);
            var mainCenterX = (int)((minX + maxX) / 2);
            var mainCenterY = (int)((minY + maxY) / 2);

            var bitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(bitmap);
            
            canvas.Clear(SKColors.Black);
            canvas.Translate(0, height);
            canvas.Scale(1, -1);
            foreach (var figure in figures)
                DrawFigure(figure, canvas, width, height, mainCenterX, mainCenterY);

            var rotatedBitmap = RotateImage(bitmap);
            var image = SKImage.FromBitmap(rotatedBitmap);
            return image.Encode().ToArray();
        }

        public async Task<List<byte[]>> DrawDXFCuttingAsync(Cutting2DResult result)
        {

            var images = new List<byte[]>();
            var detailColorsDict = new Dictionary<int, SKColor>();

            var width = result.Workpieces[0].Width;
            var height = result.Workpieces[0].Height;

            var bitmap = new SKBitmap(width, height);
            var canvas = new SKCanvas(bitmap);
            canvas.Clear();

            foreach (var workpiece in result.Workpieces)
            {
                canvas.Clear(SKColors.Black);
                foreach (var detail in workpiece.Details)
                {
                    if (detail.Rotated) RotateFigures(detail.Figures);
                    var center = GetDetailCenter(detail.Figures, detail.X, detail.Y);
                    foreach (var figure in detail.Figures)
                        DrawFigure(figure, canvas, detail.Width, detail.Height, (int)center.X, (int)center.Y);
                }
                var rotatedBitmap = RotateBitmap90(bitmap);
                var flippedBitmap = FlipBitmapVertically(rotatedBitmap);
                images.Add(SKImage.FromBitmap(flippedBitmap).Encode().ToArray());
            }
            return images;
        }

        private void DrawFigure(Figure figure, SKCanvas canvas, int width, int height, int detailCenterX, int detailCenterY, bool rotated = false)
        {
            var blackPaint = new SKPaint();
            blackPaint.Color = SKColors.Black;

            var whitePaint = new SKPaint();
            whitePaint.Color = SKColors.White;
            whitePaint.Style = SKPaintStyle.Stroke;
            var coordinates = figure.Coordinates.Split(';')
                                                .Select(f => float.Parse(f, new CultureInfo("ru-RU")))
                                                .ToList();
            //line
            if (figure.TypeId == 1)
            {
                var start = NormalizeCoordinates(width, height, detailCenterX, detailCenterY, coordinates[0], coordinates[1], rotated);
                var end = NormalizeCoordinates(width, height, detailCenterX, detailCenterY, coordinates[2], coordinates[3], rotated);
                canvas.DrawLine(new SKPoint(start.X, start.Y), new SKPoint(end.X, end.Y), whitePaint);
            }
            //circle
            if (figure.TypeId == 2)
            {
                var center = NormalizeCoordinates(width, height, detailCenterX, detailCenterY, coordinates[0], coordinates[1], rotated);
                var radius = coordinates[2];
                canvas.DrawCircle(center.X, center.Y, radius, whitePaint);
                canvas.DrawCircle(center.X, center.Y, radius - 1, blackPaint);
            }
            //arc
            if (figure.TypeId == 3)
            {
                var center = NormalizeCoordinates(width, height, detailCenterX, detailCenterY, coordinates[0], coordinates[1], rotated);
                var radius = coordinates[2];
                var startAngle = coordinates[3];
                var endAngle = coordinates[4];
                if (rotated)
                {
                    startAngle += 90;
                    endAngle += 90;
                }
                canvas.DrawArc(new SKRect(center.X - radius,
                    center.Y - radius,
                    center.X + radius,
                    center.Y + radius), startAngle, Math.Abs(endAngle + 360 - startAngle) % 360, false, whitePaint);
            }
            //Spline
            if (figure.TypeId == 4)
            {
                var coorditanesSpline = figure.Coordinates.Split('/');
                for (var i = 0; i < coorditanesSpline.Length - 2; i++)
                {
                    var start = NormalizeCoordinates(width, height, detailCenterX, detailCenterY, float.Parse(coorditanesSpline[i].Split(';')[0]), float.Parse(coorditanesSpline[i].Split(';')[1]), rotated);
                    var finish = NormalizeCoordinates(width, height, detailCenterX, detailCenterY, float.Parse(coorditanesSpline[i + 1].Split(';')[0]), float.Parse(coorditanesSpline[i + 1].Split(';')[1]), rotated);
                    canvas.DrawLine(new SKPoint(start.X, start.Y), new SKPoint(finish.X, finish.Y), whitePaint);
                }
            }
        }

        private Point NormalizeCoordinates(int width, int height, int centerX, int centerY, float x, float y, bool rotated = false)
        {
            if (rotated) (x, y) = (-y, x);
            return new Point((width / 2 - centerX + x), (height / 2 - centerY + y));
        }
        
        private SKBitmap RotateImage(SKBitmap bitmap)
        {
            var rotatedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
            {
                canvas.RotateDegrees(180, bitmap.Width / 2, bitmap.Height / 2);
                canvas.Scale(-1, 1, bitmap.Width / 2.0f, 0);
                canvas.DrawBitmap(bitmap, 0, 0);
            }
            return rotatedBitmap;
        }

        private void RotateFigures(List<Figure> figures)
        {
            for(var i = 0; i < figures.Count; i++)
            {
                var coordinates = figures[i].Coordinates
                                            .Split(';')
                                            .Select(c => float.Parse(c, new CultureInfo("ru-RU")))
                                            .ToList();
                (coordinates[0], coordinates[1]) = (-coordinates[1], coordinates[0]);
                if (figures[i].TypeId == 1) (coordinates[2], coordinates[3]) = (-coordinates[3], coordinates[2]);
                if (figures[i].TypeId == 3) (coordinates[3], coordinates[4]) = (coordinates[3]+90, coordinates[4]+90);
                figures[i].Coordinates = String.Join(';', coordinates).Replace(".", ","); // Culture fix
            }
        }

        private Point GetDetailCenter(List<Figure> figures, float detailX = 0, float detailY = 0)
        {
            var maxX = figures.Max(f => float.Parse(f.Coordinates.Split(';')[0], new CultureInfo("ru-RU")));
            var maxY = figures.Max(f => float.Parse(f.Coordinates.Split(';')[1], new CultureInfo("ru-RU")));
            var minX = figures.Min(f => float.Parse(f.Coordinates.Split(';')[0], new CultureInfo("ru-RU")));
            var minY = figures.Min(f => float.Parse(f.Coordinates.Split(';')[1], new CultureInfo("ru-RU")));

            var detailCenterX = (int)(((minX + maxX) / 2) - detailX);
            var detailCenterY = (int)(((minY + maxY) / 2) - detailY);

            return new Point(detailCenterX, detailCenterY);
        }
        private SKBitmap FlipBitmapVertically(SKBitmap srcBitmap)
        {
            var flippedBitmap = new SKBitmap(srcBitmap.Width, srcBitmap.Height);
            using (var surface = new SKCanvas(flippedBitmap))
            {
                surface.Translate(0, srcBitmap.Height);
                surface.Scale(1, -1);
                surface.DrawBitmap(srcBitmap, 0, 0);
            }
            return flippedBitmap;
        }
        private SKBitmap RotateBitmap90(SKBitmap srcBitmap)
        {
            var rotatedBitmap = new SKBitmap(srcBitmap.Height, srcBitmap.Width);
            using (var surface = new SKCanvas(rotatedBitmap))
            {
                surface.Translate(0, rotatedBitmap.Height);
                surface.RotateDegrees(-90);
                surface.DrawBitmap(srcBitmap, 0, 0);
            }
            return rotatedBitmap;
        }
    }
}
