using ClipRateRecorder.Models.Db.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Utils
{
  public enum ViewPage
  {
    Daily,
    Spot,
    Milestone,
  }

  public class UtilData
  {
    public Dictionary<MilestoneTarget, string> MilestoneTargets { get; } = new()
    {
      { MilestoneTarget.AllEffective, "効率的な全ての時間" },
      { MilestoneTarget.AllIneffective, "非効率的な全ての時間" },
      { MilestoneTarget.MostEffective, "非常に効率的な時間" },
      { MilestoneTarget.Effective, "効率的な時間" },
      { MilestoneTarget.Normal, "普通の時間" },
      { MilestoneTarget.Ineffective, "非効率的な時間" },
      { MilestoneTarget.MostIneffective, "非常に非効率的な時間" },
      { MilestoneTarget.Score, "スコア" },
    };

    public Dictionary<MilestoneType, string> MilestoneTypes { get; } = new()
    {
      { MilestoneType.More, "以上" },
      { MilestoneType.Less, "以下" },
    };
  }

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
