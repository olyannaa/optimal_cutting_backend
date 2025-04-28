
using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.IO;
using SkiaSharp;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class DXFService : IDXFService
    {
        private readonly HttpClient _httpClient;

        public DXFService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<byte[]>> Create2DDXFAsync(Cutting2DResult result)
        {
            var answer = new List<byte[]>();
            foreach (var item in result.Workpieces)
            {
                var dxf = new DxfDocument();
                foreach(var detail in item.Details)
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

        public async Task<List<byte[]>> CreateDXFAsync(Cutting2DResult result)
        {
            var answer = new List<byte[]>();
            foreach (var item in result.Workpieces)
            {
                var dxf = new DxfDocument();
                foreach (var detail in item.Details)
                {
                    foreach (var figure in detail.Figures)
                    {
                        var coorditanes = figure.Coordinates.Split(';').Select(f => float.Parse(f, new CultureInfo("ru-RU"))).ToList();
                        var sizes = new Vector2(detail.Width / 2, -detail.Height / 2);
                        var center = GetDetailCenter(detail.Figures);
                        if (figure.TypeId == 1)
                        {
                            var start = new Vector2(coorditanes[0], coorditanes[1]);
                            var finish = new Vector2(coorditanes[2], coorditanes[3]);
                            if (detail.Rotated)
                            {
                                (start.X, start.Y) = (-start.Y, start.X);
                                (finish.X, finish.Y) = (-finish.Y, finish.X);
                                (center.X, center.Y) = (-center.Y, center.X);
                            }
                            start += new Vector2(detail.X, -detail.Y) + sizes - center;
                            finish += new Vector2(detail.X, -detail.Y) + sizes - center;

                            dxf.Entities.Add(new Line(start, finish));
                        }
                        if (figure.TypeId == 2)
                        {
                            var start = new Vector2(coorditanes[0], coorditanes[1]);
                            var radius = coorditanes[2];
                            if (detail.Rotated)
                            {
                                (start.X, start.Y) = (-start.Y, start.X);
                                (center.X, center.Y) = (-center.Y, center.X);
                            }
                            start += new Vector2(detail.X, -detail.Y) + sizes - center;
                            dxf.Entities.Add(new Circle(start, radius));
                        }
                        if (figure.TypeId == 3)
                        {
                            var start = new Vector2(coorditanes[0], coorditanes[1]);
                            var radius = coorditanes[2];
                            var startAngle = coorditanes[3];
                            var endAngle = coorditanes[4];
                            if (detail.Rotated)
                            {
                                (start.X, start.Y) = (-start.Y, start.X);
                                (center.X, center.Y) = (-center.Y, center.X);
                                (startAngle, endAngle) = (startAngle + 90, endAngle + 90);
                            }
                            start += new Vector2(detail.X, -detail.Y) + sizes - center;
                            dxf.Entities.Add(new Arc(start, radius, startAngle, endAngle));
                        }

                    }
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
            try
            {

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    throw new ArgumentException("File bytes cannot be empty");
                }

                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(fileContent, "file", "drawing.dxf");

                var response = await _httpClient.PostAsync("parse-dxf", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DXF API error: {response.StatusCode}, {errorContent}");
                    throw new Exception($"DXF parsing failed: {errorContent}");
                }

                var responseStream = await response.Content.ReadAsStreamAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var result = await JsonSerializer.DeserializeAsync<List<FigureDTO>>(responseStream, options);

                if (result == null || result.Count == 0)
                {
                    throw new Exception("No valid figures found in DXF file");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Error in GetDXFAsync");
                throw new Exception("Failed to parse DXF", ex);
            }
        }


        private Vector2 GetDetailCenter(List<Figure> figures, float detailX = 0, float detailY = 0)
        {
            var maxX = figures.Max(f => float.Parse(f.Coordinates.Split(';')[0], new CultureInfo("ru-RU")));
            var maxY = figures.Max(f => float.Parse(f.Coordinates.Split(';')[1], new CultureInfo("ru-RU")));
            var minX = figures.Min(f => float.Parse(f.Coordinates.Split(';')[0], new CultureInfo("ru-RU")));
            var minY = figures.Min(f => float.Parse(f.Coordinates.Split(';')[1], new CultureInfo("ru-RU")));

            var detailCenterX = (((minX + maxX) / 2) + detailX);
            var detailCenterY = (((minY + maxY) / 2) + detailY);

            return new Vector2(detailCenterX, detailCenterY);
        }
    }
}
