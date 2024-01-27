using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Db.Entities
{
  public class ActivityEvaluationRuleData
  {
    public uint Id { get; set; }

    public int Order { get; set; }

    public string ExePath { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public ActivityEvaluation Evaluation { get; set; }
  }

  public enum ActivityEvaluation : int
  {
    MostIneffective = -2,
    Ineffective = -1,
    Normal = 0,
    Effective = 1,
    MostEffective = 2,
  }
}
