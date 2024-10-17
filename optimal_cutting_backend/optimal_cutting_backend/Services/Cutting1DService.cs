using System.Linq;
using vega.Models;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class Cutting1DService : ICutting1DService
    {
        public async Task<Cutting1DResult> CalculateCuttingAsync(List<int> details, List<int> workpieces)
        {
            workpieces = workpieces.OrderBy(w => w).ToList();
            if (details.Max() > workpieces.Max())
                throw new Exception("detail length > workpiece length");

            details = details.OrderByDescending(x => x).ToList();
            var cuts = new List<(int, List<int>)>();
            while(true)
            {
                var cut = new List<int>();
                var workpieceIndex = 0;
                var workpiece = workpieces[workpieceIndex];
                var lastWorkpiece = workpiece;
                var summ = 0;
                if (details.Count == 0) break;
                var j = 0;
                while (j < details.Count)
                {
                    if (summ + details[j] <= workpiece)
                    {
                        cut.Add(details[j]);
                        summ += details[j];
                        details.RemoveAt(j);
                        lastWorkpiece = workpiece;
                        j--;
                    }
                    else
                    {
                        if (workpieceIndex < workpieces.Count - 1)
                        {
                            workpieceIndex++;
                            workpiece = workpieces[workpieceIndex];
                            j--;
                        }
                        else break;
                    }
                    j++;
                }
                cuts.Add((lastWorkpiece, cut));
            }

            return CreateResultModel(cuts);
        }
        private Cutting1DResult CreateResultModel(List<(int, List<int>)> cuts)
        {
            var result = new Cutting1DResult();
            for (var i = 0; i < cuts.Count; i++)
            {
                var workpiece = new Workpiece();
                workpiece.Length = cuts[i].Item1;
                workpiece.Details = cuts[i].Item2;
                workpiece.PercentUsage = Math.Round((double)cuts[i].Item2.Sum(c => c) / cuts[i].Item1, 2);
                result.Workpieces.Add(workpiece);
            }
            result.TotalPercentUsage = Math.Round((double)result.Workpieces.Sum(w => w.PercentUsage)
                                            / (result.Workpieces.Count), 2);

            return result;
        }
    }
}
