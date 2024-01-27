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

      activity.Rule = this.RequestRule(activity.ExePath, activity.Title);
    }

    public ActivityEvaluationRule RequestRule(string exePath, string? title = null)
    {
      var exePathRules = this.Rules.Where(r => r.ExePath == exePath).ToList();

      ActivityEvaluationRule? rule = null;
      if (!string.IsNullOrEmpty(title))
      {
        rule = exePathRules.FirstOrDefault(r => r.WindowTitle == title);
      }
      if (rule == null)
      {
        rule = exePathRules.FirstOrDefault(r => string.IsNullOrEmpty(r.WindowTitle));
      }

      if (rule == null)
      {
        var data = new ActivityEvaluationRuleData
        {
          ExePath = exePath,
          Title = exePathRules.Count == 0 ? string.Empty : title ?? string.Empty,
        };

        rule = new(data);
        this.Rules.Add(rule);
      }

      return rule;
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

  class ActivityEvaluationRule : INotifyPropertyChanged
  {
    private ActivityEvaluationRuleData? Data { get; set; }

    public string WindowTitle { get; set; } = string.Empty;

    public string ExePath { get; set; } = string.Empty;

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
      this.WindowTitle = this.Data.Title;
      this.ExePath = this.Data.ExePath;
      this.Evaluation = this.Data.Evaluation;
    }

    private void SetData()
    {
      if (this.Data == null)
      {
        return;
      }
      this.Data.Title = this.WindowTitle;
      this.Data.ExePath = this.ExePath;
      this.Data.Evaluation = this.Evaluation;
    }

    public async Task SaveDataAsync(MainContext db)
    {
      var isRemove = false;

      if (this.Data == null || this.Data.Id == default)
      {
        if (this.Evaluation != ActivityEvaluation.Normal)
        {
          this.Data ??= new();
          this.SetData();
          await db.ActivityEvaluationRules!.AddAsync(this.Data);
        }
        else
        {
          return;
        }
      }
      else
      {
        db.ActivityEvaluationRules!.Attach(this.Data);

        if (this.Evaluation != ActivityEvaluation.Normal)
        {
          this.SetData();
          await db.SaveChangesAsync();
        }
        else
        {
          db.ActivityEvaluationRules!.Remove(this.Data);
          isRemove = true;
        }
      }

      await db.SaveChangesAsync();
      
      if (isRemove)
      {
        this.Data.Id = default;
      }
    }

    public bool Match(WindowActivity activity)
    {
      return this.ExePath == activity.ExePath && (string.IsNullOrEmpty(this.WindowTitle) || this.WindowTitle == activity.Title);
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
