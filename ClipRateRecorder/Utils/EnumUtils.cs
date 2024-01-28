using ClipRateRecorder.Models.Db.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Utils
{
  internal static class EnumUtils
  {
    public static ActivityEvaluation StringToActivityEvaluation(string ev)
    {
      var evaluation = ev switch
      {
        nameof(ActivityEvaluation.MostIneffective) => ActivityEvaluation.MostIneffective,
        nameof(ActivityEvaluation.Ineffective) => ActivityEvaluation.Ineffective,
        nameof(ActivityEvaluation.Effective) => ActivityEvaluation.Effective,
        nameof(ActivityEvaluation.MostEffective) => ActivityEvaluation.MostEffective,
        _ => ActivityEvaluation.Normal,
      };

      return evaluation;
    }
  }
}
