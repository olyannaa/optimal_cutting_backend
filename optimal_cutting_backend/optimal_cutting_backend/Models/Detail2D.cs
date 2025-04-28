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
            foreach (var figure in Figures)
            {
                figure.Coordinates = figure.Coordinates.Replace('.', ',');
                var coorditanes = figure.Coordinates.Split(';');
                if (figure.TypeId == 1) //line
                {
                    var figureMaxX = Math.Max(
                        float.Parse(coorditanes[0], new CultureInfo("ru-RU")),
                        float.Parse(coorditanes[2], new CultureInfo("ru-RU")));
                    var figureMinX = Math.Min(
                        float.Parse(coorditanes[0], new CultureInfo("ru-RU")),
                        float.Parse(coorditanes[2], new CultureInfo("ru-RU")));
                    var figureMaxY = Math.Max(
                        float.Parse(coorditanes[1], new CultureInfo("ru-RU")),
                        float.Parse(coorditanes[3], new CultureInfo("ru-RU")));
                    var figureMinY = Math.Min(
                        float.Parse(coorditanes[1], new CultureInfo("ru-RU")),
                        float.Parse(coorditanes[3], new CultureInfo("ru-RU")));
                    maxX = Math.Max(maxX, figureMaxX);
                    minX = Math.Min(minX, figureMinX);
                    maxY = Math.Max(maxY, figureMaxY);
                    minY = Math.Min(minY, figureMinY);
                }
                else if (figure.TypeId == 2 || figure.TypeId == 3) //circle and arc
                {
                    var centerX = float.Parse(coorditanes[0], new CultureInfo("ru-RU"));
                    var centerY = float.Parse(coorditanes[1], new CultureInfo("ru-RU"));
                    var radius = float.Parse(coorditanes[2], new CultureInfo("ru-RU"));
                    maxX = Math.Max(centerX + radius, maxX);
                    minX = Math.Min(centerX - radius, minX);
                    maxY = Math.Max(centerY + radius, maxY);
                    minY = Math.Min(centerY - radius, minY);
                }
                else if (figure.TypeId == 4) //spline
                {
                    coorditanes = figure.Coordinates.Split('/');
                    for (var i = 0; i < coorditanes.Length - 1; i++)
                    {
                        var point = coorditanes[i].Split(';');
                        var pointX = float.Parse(point[0], new CultureInfo("ru-RU"));
                        var pointY = float.Parse(point[1], new CultureInfo("ru-RU"));
                        maxX = Math.Max(maxX, pointX);
                        minX = Math.Min(minX, pointX);
                        maxY = Math.Max(maxY, pointY);
                        minY = Math.Min(minY, pointY);
                    }
                }
            }

            this.Width = (int)(maxX - minX);
            this.Height = (int)(maxY - minY);
        }
        
        public void Rotate() => (Width, Height) = (Height, Width);
    }
}
