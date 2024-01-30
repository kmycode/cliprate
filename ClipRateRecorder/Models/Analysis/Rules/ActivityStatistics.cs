using ClipRateRecorder.Models.Db.Entities;
using ClipRateRecorder.Models.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Analysis.Rules
{
  class ActivityStatistics
  {
    public static ActivityStatistics Empty { get; } = new();

    public double TotalDuration { get; }

    public double MostIneffective { get; }

    public double Ineffective { get; }

    public double Normal { get; }

    public double Effective { get; }

    public double MostEffective { get; }

    public double Score { get; }

    public IEnumerable<double> Values => new double[] { this.MostEffective, this.Effective, this.Normal, this.Ineffective, this.MostIneffective, };

    public ActivityEvaluation Evaluation { get; }

    private ActivityStatistics() { }

    public ActivityStatistics(IEnumerable<WindowActivity> activities)
    {
      double GetValue(IEnumerable<WindowActivity> targets)
      {
        if (!targets.Any())
        {
          return 0;
        }

        return targets.Sum(a => a.ProperDuration.TotalSeconds);
      }

      this.MostIneffective = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.MostIneffective));
      this.Ineffective = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.Ineffective));
      this.Normal = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.Normal));
      this.Effective = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.Effective));
      this.MostEffective = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.MostEffective));
      this.TotalDuration = this.MostIneffective + this.Ineffective + this.Normal + this.Effective + this.MostEffective;
      this.Score = this.CalcScore();
      this.Evaluation = this.CalcEvaluation();
    }

    public ActivityStatistics(IEnumerable<ActivityStatistics> statistics)
    {
      if (!statistics.Any())
      {
        return;
      }

      this.TotalDuration = statistics.Sum(s => s.TotalDuration);
      this.MostIneffective = statistics.Sum(s => s.MostIneffective);
      this.Ineffective = statistics.Sum(s => s.Ineffective);
      this.Normal = statistics.Sum(s => s.Normal);
      this.Effective = statistics.Sum(s => s.Effective);
      this.MostEffective = statistics.Sum(s => s.MostEffective);
      this.Score = this.CalcScore();
      this.Evaluation = this.CalcEvaluation();
    }

    private double CalcScore()
    {
      var baseValue = this.TotalDuration - this.Normal + this.MostEffective + this.MostIneffective;
      if (baseValue == 0)
      {
        return 0;
      }

      var effectiveValue = this.MostEffective * 2 + this.Effective - this.Ineffective - this.MostIneffective * 2;
      var value = effectiveValue / baseValue * 100 / 2 + 50;

      return Math.Max(value, 0);
    }

    private ActivityEvaluation CalcEvaluation()
    {
      var data = new List<(double, ActivityEvaluation)>
      {
        (this.MostIneffective, ActivityEvaluation.MostIneffective),
        (this.Ineffective, ActivityEvaluation.Ineffective),
        (this.Normal, ActivityEvaluation.Normal),
        (this.Effective, ActivityEvaluation.Effective),
        (this.MostEffective, ActivityEvaluation.MostEffective),
      };
      return data.OrderByDescending(d => d.Item1).First().Item2;
    }
  }
}
