﻿using vega.Models;
using vega.Services.Interfaces;

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

            return CreateResultModel(cuts, workpieces);
        }
        private Cutting1DResult CreateResultModel(List<List<int>> cuts, List<int> workpieces)
        {
            var result = new Cutting1DResult();
            for (var i = 0; i < workpieces.Count; i++)
            {
                var workpiece = new Workpiece();
                workpiece.Length = workpieces[i];
                if (i >= cuts.Count)
                {
                    
                    workpiece.Details = new List<int> { 0 };
                    workpiece.PercentUsage = 0;
                }
                else
                {
                    workpiece.Details = cuts[i];
                    workpiece.PercentUsage = Math.Round((double)cuts[i].Sum(c => c) / workpieces[i], 2);
                }
                result.Workpieces.Add(workpiece);
            }
            result.TotalPercentUsage = Math.Round((double)result.Workpieces.Sum(w => w.PercentUsage)
                                            / (result.Workpieces.Count), 2);

            return result;
        }
    }
}
