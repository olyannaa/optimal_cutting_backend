
using Org.BouncyCastle.Crypto.Prng;
using vega.Controllers.DTO;
using vega.Migrations.DAL;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class Cutting2DService : ICutting2DService
    {
        //Отступы от краёв заготовки
        public int Indent = 10;
        public async Task<Cutting2DResult> CalculateCuttingAsync(List<Detail2D> details, Workpiece workpiece, float thickness)
        {
            if (details.Max(d => d.Width) > workpiece.Width || details.Max(d => d.Height) > Math.Max(workpiece.Height, workpiece.Width)) throw new Exception("detail > workpiece");
            details = details.OrderByDescending(d => d.Height * d.Width).ToList();
            if (workpiece.Height > workpiece.Width)
                (workpiece.Width, workpiece.Height) = (workpiece.Height, workpiece.Width);
            var workpieces = new List<Workpiece2D>();
            while(details.Count > 0)
                workpieces.Add(CalculateCuttingForWorkpiece(details, workpiece, thickness));
            
            var result = new Cutting2DResult() {Workpieces = workpieces, TotalPercentUsage = Math.Round(workpieces.Sum(w=>w.ProcentUsage) / workpieces.Count,2)};

            return result;
        }
        public Workpiece2D CalculateCuttingForWorkpiece(List<Detail2D> details, Workpiece workpiece, float thickness)
        {
            var result = new List<Detail2D>();
            workpiece.Width -= 2 * Indent;
            workpiece.Height -= 2 * Indent;
            var arr = new byte[workpiece.Width][];
            arr = arr.Select(x => new byte[workpiece.Height]).ToArray();
            int currX = 0, currY = 0;
            var minSize = Math.Min(details.Min(d => d.Height), details.Min(d => d.Width));
            var detailsSizes = 0;
            while(details.Count > 0)
            {
                var detailNumber = 0;
                if (currY + minSize >= workpiece.Height) break;
                //details = details.Select(d => { d.Rotated = false; return d; }).ToList();
                while (detailNumber < details.Count)
                {
                    var detail = details[detailNumber];
                    currX = CanAddToRow(arr, currY);
                    if (currX == -1) break;
                    if (currX + minSize >= workpiece.Width && details.Count > 1) break;

                    if (CanAddDetail(arr, detail, currX, currY))
                    {
                        var leftDetail = DetailLeft(result, currX, currY, detail.Height);
                        var topDetail = DetailTop(result, currX, currY, detail.Width);
                        if (leftDetail != null) 
                        {
                            detail.X = (leftDetail.X + leftDetail.Width) + thickness;
                            currX = (int)detail.X;
                        }  
                        else detail.X = currX;

                        if (topDetail != null) detail.Y = (topDetail.Y + topDetail.Height) + thickness;
                        else detail.Y = currY;

                        if (CanAddDetail(arr, detail, currX, currY))
                        {
                            AddDetail(arr, detail, currX, currY);
                            detailsSizes += detail.Width * detail.Height;
                            result.Add(detail);
                            details.RemoveAt(detailNumber);
                            if (details.Count > 0)
                                minSize = Math.Min(details.Min(d => d.Height), details.Min(d => d.Width));
                            detailNumber--;
                        }
                    }
                    else
                    {
                        details[detailNumber] = RotateDetail(detail);
                        detail.Rotated = !detail.Rotated;
                        if (detail.Rotated == true)
                            detailNumber--;
                    }
                    detailNumber++;
                }
                currX = CanAddToRow(arr, currY);
                if (currX != -1) arr[currX][currY] = 1;
                else currY++;
            }

            result = result.Select(x =>
            {
                x.X += Indent;
                x.Y += Indent;
                return x;
            }).ToList();
            var procentUsage = Math.Round((double)detailsSizes / (workpiece.Height * workpiece.Width), 2);
            workpiece.Width += 2 * Indent;
            workpiece.Height += 2 * Indent;
            var resultWorkpiece = new Workpiece2D { Details = result, Width = workpiece.Width, Height = workpiece.Height };
            resultWorkpiece.ProcentUsage = procentUsage;
            return resultWorkpiece;
        }

        public Detail2D DetailLeft(List<Detail2D> details, int x, int y, int height)
        {
            return details.Where(detail => detail.Y + detail.Height > y && detail.Y < y + height && detail.X + detail.Width <= x)
                           .OrderBy(detail => x - detail.X - detail.Width)
                           .FirstOrDefault();
        }

        public Detail2D DetailTop(List<Detail2D> details, int x, int y, int width)
        {
            return details.Where(detail => detail.X + detail.Width > x && detail.X < x + width )
                          .OrderBy(detail => y - detail.Y - detail.Height)
                          .FirstOrDefault();
        }

        public Detail2D RotateDetail(Detail2D detail)
        {
            var width = detail.Width;
            detail.Width = detail.Height;
            detail.Height = width;
            return detail;
        }

        public bool CanAddDetail(byte[][] arr, Detail2D detail, int x, int y)
        {
            if (x < 0 || y < 0) return false;
            if (x + detail.Width >= arr.Length || detail.X + detail.Width >= arr.Length) return false;
            if (y + detail.Height >= arr[0].Length || detail.Y + detail.Height >= arr[0].Length) return false;
            for (int i = 0; i < detail.Width; i++)
                if (arr[x + i][y] == 1) return false;
            return true;
        }

        public int CanAddToRow(byte[][] arr, int y)
        {
            for (int i = 0; i < arr.Length; i++)
                if (arr[i][y] == 0) return i;
            return -1;
        }
        public void AddDetail(byte[][] arr, Detail2D detail, int x, int y)
        {
            for (int i = 0; i < detail.Width; i++)
                for (int j = 0; j < detail.Height; j++)
                    arr[x + i][y + j]++;
        }
    }
}
