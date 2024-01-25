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

    public string ExePathes { get; set; } = string.Empty;

    public string Titles { get; set; } = string.Empty;

    public PropertiesMatchRule MatchRule { get; set; }

    public ActivityEvaluation Evaluation { get; set; }
  }

  public enum PropertiesMatchRule : int
  {
    Or = 0,
    And = 1,
  }

  public enum ActivityEvaluation : int
  {
    MostIneffective = 0,
    Ineffective = 1,
    Normal = 2,
    Effective = 3,
    MostEffective = 4,
  }
}
