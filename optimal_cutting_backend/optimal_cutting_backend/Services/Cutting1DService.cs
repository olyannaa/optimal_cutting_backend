using vega.Models;
using vega.Services.Interfaces;

namespace vega.Services
{
    public class Cutting1DService : ICutting1DService
    {
        public async Task<Cutting1DResult> CalculateCuttingAsync(List<int> details, int workpiece)
        {
            if (details.Max(x => x) > workpiece)
                throw new Exception("detail length > workpiece length");

            details = details.OrderBy(x => -x).ToList();
            var cuts = new List<List<int>>();
            while(true)
            {
                var cut = new List<int>();
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
                        j--;
                    }
                    j++;
                }
                cuts.Add(cut);
            }

            return CreateResultModel(cuts, workpiece);
        }
        private Cutting1DResult CreateResultModel(List<List<int>> cuts, int workpieceLength)
        {
            var result = new Cutting1DResult();
            for (var i = 0; i < cuts.Count; i++)
            {
                var workpiece = new Workpiece();
                workpiece.Length = workpieceLength;
                workpiece.Details = cuts[i];
                workpiece.PercentUsage = Math.Round((double)cuts[i].Sum(c => c) / workpieceLength, 2);
                result.Workpieces.Add(workpiece);
            }
            result.TotalPercentUsage = Math.Round((double)result.Workpieces.Sum(w => w.PercentUsage)
                                            / (result.Workpieces.Count), 2);

            return result;
        }
    }
}
