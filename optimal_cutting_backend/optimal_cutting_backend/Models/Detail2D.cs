using System.Globalization;
using vega.Migrations.DAL;

namespace vega.Models
{
    public class Detail2D
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public List<Figure> Figures { get; set; }
        public bool Rotated { get; set; } = false;

        public Detail2D() { }

        public Detail2D(List<Figure> figures)
        {
            Figures = figures;
            GetSize();
            if (Width > Height)
            {
                Rotate();
                Rotated = true;
            }
        }
        private void GetSize()
        {
            var maxX = float.MinValue;
            var minX = float.MaxValue;
            var maxY = float.MinValue;
            var minY = float.MaxValue;
            var culture = new CultureInfo("ru-RU");

            foreach (var figure in Figures)
            {
                figure.Coordinates = figure.Coordinates.Replace('.', ',');
                var coords = figure.Coordinates.Split(';')
                    .Select(s => float.Parse(s, culture))
                    .ToList();

                if (figure.TypeId == 1) // LINE
                {
                    maxX = Math.Max(maxX, Math.Max(coords[0], coords[2]));
                    minX = Math.Min(minX, Math.Min(coords[0], coords[2]));
                    maxY = Math.Max(maxY, Math.Max(coords[1], coords[3]));
                    minY = Math.Min(minY, Math.Min(coords[1], coords[3]));
                }
                else if (figure.TypeId == 2) // CIRCLE
                {
                    var cx = coords[0];
                    var cy = coords[1];
                    var r = coords[2];
                    maxX = Math.Max(maxX, cx + r);
                    minX = Math.Min(minX, cx - r);
                    maxY = Math.Max(maxY, cy + r);
                    minY = Math.Min(minY, cy - r);
                }
                else if (figure.TypeId == 3) // ARC
                {
                    float cx = coords[0];
                    float cy = coords[1];
                    float r = coords[2];
                    float start = coords[3];
                    float end = coords[4];

                    var arcPoints = GetArcExtremePoints(cx, cy, r, start, end);
                    foreach (var (x, y) in arcPoints)
                    {
                        maxX = Math.Max(maxX, x);
                        minX = Math.Min(minX, x);
                        maxY = Math.Max(maxY, y);
                        minY = Math.Min(minY, y);
                    }
                }
                else if (figure.TypeId == 4) // SPLINE
                {
                    var points = figure.Coordinates.Split('/');
                    foreach (var point in points)
                    {
                        var parts = point.Split(';');
                        if (parts.Length >= 2)
                        {
                            var px = float.Parse(parts[0], culture);
                            var py = float.Parse(parts[1], culture);
                            maxX = Math.Max(maxX, px);
                            minX = Math.Min(minX, px);
                            maxY = Math.Max(maxY, py);
                            minY = Math.Min(minY, py);
                        }
                    }
                }
            }

            this.Width = (int)(maxX - minX);
            this.Height = (int)(maxY - minY);
        }
        private List<(float x, float y)> GetArcExtremePoints(float cx, float cy, float r, float startAngle, float endAngle)
        {
            List<(float x, float y)> points = new();

            float Normalize(float angle) => (angle % 360 + 360) % 360;

            bool IsAngleBetween(float angle, float start, float end)
            {
                angle = Normalize(angle);
                start = Normalize(start);
                end = Normalize(end);
                if (start < end)
                    return angle >= start && angle <= end;
                return angle >= start || angle <= end;
            }

            points.Add(PolarToCartesian(cx, cy, r, startAngle));
            points.Add(PolarToCartesian(cx, cy, r, endAngle));

            foreach (var a in new[] { 0f, 90f, 180f, 270f })
            {
                if (IsAngleBetween(a, startAngle, endAngle))
                {
                    points.Add(PolarToCartesian(cx, cy, r, a));
                }
            }

            return points;
        }

        private (float x, float y) PolarToCartesian(float cx, float cy, float r, float angleDeg)
        {
            double rad = angleDeg * Math.PI / 180.0;
            return ((float)(cx + r * Math.Cos(rad)), (float)(cy + r * Math.Sin(rad)));
        }


        public void Rotate() => (Width, Height) = (Height, Width);
    }
}
