using vega.Models;

namespace vega.Services
{
    public class Cutting1DService : ICutting1DService
    {
        public Cutting1DResult CalculateCutting(List<int> details, List<int> workpieces)
        {
            if (details.Max(x => x) > workpieces.Max(y => y))
                throw new Exception("detail length > workpiece length");
            details = details.OrderBy(x => -x).ToList();
            var cuts = new List<List<int>>();
            for(var i = 0; i < workpieces.Count; i++)
            {
                var cut = new List<int>();
                var summ = 0;
                if (details.Count == 0) break;
                var j = 0;
                while (j < details.Count)
                {
                    if (summ + details[j] <= workpieces[i])
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

            var result = new Cutting1DResult();
            for (var i = 0; i < workpieces.Count; i++)
            {
                var workpiece = new Workpiece()
                {
                    Length = workpieces[i],
                    Details = cuts[i],
                    PercentUsage = Math.Round((double)cuts[i].Sum(c => c) / workpieces[i], 2)
                };
                result.Workpieces.Add(workpiece);
            }
            result.TotalPercentUsage = Math.Round((double)result.Workpieces.Sum(w => w.PercentUsage)
                                            / (result.Workpieces.Count), 2);

            return result;

        }
    }
}
