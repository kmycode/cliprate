using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Db.Entities;
using ClipRateRecorder.Models.Window;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Analysis.Rules
{
  class ActivityEvaluator
  {
    public ObservableCollection<ActivityEvaluationRule> Rules { get; } = [];

    public void Evaluate(IEnumerable<WindowActivity> activities)
    {
      if (!this.Rules.Any())
      {
        return;
      }

      foreach (var activity in activities)
      {
        this.Evaluate(activity);
      }
    }

    public void Evaluate(WindowActivity activity)
    {
      if (!this.Rules.Any())
      {
        return;
      }

      var matchRules = this.Rules.Where(r => r.Match(activity)).ToArray();
      if (!matchRules.Any())
      {
        activity.Evaluation = ActivityEvaluation.Normal;
      }
      else
      {
        activity.Evaluation = matchRules.OrderBy(r => r.Order).First().Evaluation;
      }
    }

    public static async Task<ActivityEvaluator> CreateFromDatabaseAsync(MainContext db)
    {
      var instance = new ActivityEvaluator();

      var rules = await db.ActivityEvaluationRules!.OrderBy(r => r.Order).ToArrayAsync();
      foreach (var rule in rules)
      {
        instance.Rules.Add(new ActivityEvaluationRule(rule));
      }

      return instance;
    }
  }

  class ActivityStatistics
  {
    public static ActivityStatistics Empty { get; } = new();

    public double TotalDuration { get; }

    public double MostIneffective { get; }

    public double Ineffective { get; }

    public double Normal { get; }

    public double Effective { get; }

    public double MostEffective { get; }

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

        return targets.Sum(a => a.Duration.TotalSeconds);
      }

      this.MostIneffective = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.MostIneffective));
      this.Ineffective = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.Ineffective));
      this.Normal = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.Normal));
      this.Effective = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.Effective));
      this.MostEffective = GetValue(activities.Where(a => a.Evaluation == ActivityEvaluation.MostEffective));
      this.TotalDuration = this.MostIneffective + this.Ineffective + this.Normal + this.Effective + this.MostEffective;

      var data = new List<(double, ActivityEvaluation)>
      {
        (this.MostIneffective, ActivityEvaluation.MostIneffective),
        (this.Ineffective, ActivityEvaluation.Ineffective),
        (this.Normal, ActivityEvaluation.Normal),
        (this.Effective, ActivityEvaluation.Effective),
        (this.MostEffective, ActivityEvaluation.MostEffective),
      };
      this.Evaluation = data.OrderByDescending(d => d.Item1).First().Item2;
    }
  }

  class ActivityEvaluationRule : INotifyPropertyChanged
  {
    private ActivityEvaluationRuleData? Data { get; set; }

    public ObservableCollection<string> WindowTitles { get; } = [];

    public ObservableCollection<string> ExePathes { get; } = [];

    public int Order
    {
      get => this._order;
      set
      {
        if (this._order != value) { return; }
        this._order = value;
        this.OnPropertyChanged();
      }
    }
    private int _order;

    public PropertiesMatchRule MatchRule
    {
      get => this._matchRule;
      set
      {
        if (this._matchRule == value) return;
        this._matchRule = value;
        this.OnPropertyChanged();
      }
    }
    private PropertiesMatchRule _matchRule;

    public ActivityEvaluation Evaluation
    {
      get => this._evaluation;
      set
      {
        if (this._evaluation == value) return;
        this._evaluation = value;
        this.OnPropertyChanged();
      }
    }
    private ActivityEvaluation _evaluation;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ActivityEvaluationRule()
    {
    }

    public ActivityEvaluationRule(ActivityEvaluationRuleData data)
    {
      this.Data = data;
      this.ReadData();
    }

    private void ReadData()
    {
      if (this.Data == null)
      {
        return;
      }
      foreach (var item in this.Data.Titles.Split("\n")) this.WindowTitles.Add(item);
      foreach (var item in this.Data.ExePathes.Split("\n")) this.ExePathes.Add(item);
      this.MatchRule = this.Data.MatchRule;
      this.Evaluation = this.Data.Evaluation;
    }

    private void SetData()
    {
      if (this.Data == null)
      {
        return;
      }
      this.Data.Titles = string.Join("\n", this.WindowTitles);
      this.Data.ExePathes = string.Join("\n", this.ExePathes);
      this.Data.MatchRule = this.MatchRule;
      this.Data.Evaluation = this.Evaluation;
    }

    public async Task InputDataAsync(MainContext db)
    {
      if (this.Data == null)
      {
        this.Data = new();
        this.SetData();
        await db.ActivityEvaluationRules!.AddAsync(this.Data);
      }
      else
      {
        this.SetData();
      }
    }

    public bool Match(WindowActivity activity)
    {
      var exePathes = this.ExePathes.Any(activity.ExePath.Contains);
      var titles = this.WindowTitles.Any(activity.Title.Contains);

      var isApply = this.MatchRule switch
      {
        PropertiesMatchRule.Or => exePathes || titles,
        PropertiesMatchRule.And => exePathes && titles,
        _ => false,
      };

      return isApply;
    }

    public ActivityEvaluation Evaluate(WindowActivity activity)
    {

      return this.Match(activity) ? this.Evaluation : ActivityEvaluation.Normal;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
