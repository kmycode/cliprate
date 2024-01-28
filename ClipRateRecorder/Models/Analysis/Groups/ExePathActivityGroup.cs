using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Db.Entities;
using ClipRateRecorder.Models.Watching;
using ClipRateRecorder.Models.Window;
using ClipRateRecorder.Utils;
using Microsoft.EntityFrameworkCore;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Analysis.Groups
{
  class ExePathActivityGroupCollection : ObservableCollection<ExePathActivityGroup>
  {
    public double TotalDuration => this.Any() ? this.Sum(a => a.TotalDuration) : 0;

    private IEnumerable<WindowActivity> Activities => this.SelectMany(a => a.WindowTitleGroups.SelectMany(b => b.Activities));

    private IEnumerable<ActivityStatistics> AllStatistics => this.Select(a => a.Statistics);

    public ActivityStatistics Statistics { get; private set; } = ActivityStatistics.Empty;

    public event EventHandler? StatisticsChanged;

    public ActivityEvaluator? Evaluator
    {
      get => this._evaluator;
      set
      {
        if (this._evaluator != value)
        {
          this._evaluator = value;
          this.Evaluate();
          this.InitializeEvalucator();
        }
      }
    }
    private ActivityEvaluator? _evaluator;

    public void OnWindowActivityTicked(object? sender, WatchingTickEventArgs e)
    {
      var activity = e.CurrentActivity;

      ThreadUtil.RunGuiThread(() => this.OnWindowActivityTicked(activity));
    }

    private void OnWindowActivityTicked(WindowActivity activity)
    {
      this.Evaluator?.Evaluate(activity);

      var exePathes = this.Where(a => a.ExePath == activity.ExePath);
      if (!this.FireIfActivityUpdated(activity))
      {
        if (!exePathes.Any())
        {
          var group = ExePathActivityGroup.FromActivity(activity);
          this.Add(group);
          if (this.Evaluator != null)
          {
            group.Rule = this.Evaluator.RequestRule(activity.ExePath);
          }
        }
        else
        {
          var titles = exePathes.SelectMany(e => e.WindowTitleGroups).Where(a => a.Title == activity.Title);
          if (!titles.Any())
          {
            var group = new WindowTitleActivityGroup(activity.Title);
            group.AddActivity(activity);
            exePathes.Last().WindowTitleGroups.Add(group);
            if (this.Evaluator != null)
            {
              group.Rule = this.Evaluator.RequestRule(activity.ExePath, activity.Title);
            }
          }
          else
          {
            titles.Last().Activities.Add(activity);
          }
        }

        this.FireIfActivityUpdated(activity);
      }

      this.Reorder();
      this.UpdateStatistics();
    }

    public bool FireIfActivityUpdated(WindowActivity activity)
    {
      if (this.Count(a => a.FireIfActivityUpdated(activity)) == 0)
      {
        return false;
      }

      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(TotalDuration)));
      return true;
    }

    private void InitializeEvalucator()
    {
      if (this.Evaluator == null)
      {
        return;
      }

      foreach (var exePath in this)
      {
        exePath.Rule = this.Evaluator.RequestRule(exePath.ExePath);

        foreach (var title in exePath.WindowTitleGroups)
        {
          title.Rule = this.Evaluator.RequestRule(exePath.ExePath, title.Title);
        }
      }
    }

    public void Evaluate()
    {
      this.Evaluator?.Evaluate(this.Activities);
    }

    private void UpdateStatistics()
    {
      foreach (var item in this)
      {
        item.UpdateStatistics();
      }

      this.Statistics = new(this.AllStatistics);
      this.StatisticsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Reorder()
    {
      var copy = this.ToList();
      var ordered = copy.OrderByDescending(c => c.TotalDuration).ToList();

      for (var i = 0; i < copy.Count; i++)
      {
        var item = this[i];
        var currentIndex = i;
        var actualIndex = ordered.IndexOf(item);

        if (currentIndex >= 0 && actualIndex >= 0 && currentIndex != actualIndex)
        {
          this.MoveItem(currentIndex, actualIndex);
        }
      }
    }

    public async Task SetDefaultEvaluationAsync(string exePath, ActivityEvaluation evaluation)
    {
      foreach (var target in this.Where(a => a.ExePath == exePath))
      {
        await target.SetDefaultEvaluationAsync(evaluation);
      }
    }

    public static ExePathActivityGroupCollection FromActivities(IEnumerable<WindowActivity> activities, ActivityEvaluator evaluator)
    {
      var exePathGroups = FromActivities(activities);

      exePathGroups.Evaluator = evaluator;
      exePathGroups.UpdateStatistics();

      return exePathGroups;
    }

    public static ExePathActivityGroupCollection FromActivities(IEnumerable<WindowActivity> activities)
    {
      var exePathGroups = new ExePathActivityGroupCollection();

      foreach (var exePathActivityGroup in activities.GroupBy(a => a.ExePath))
      {
        var exePathGroup = new ExePathActivityGroup(exePathActivityGroup.Key);

        foreach (var titleActivityGroup in exePathActivityGroup.GroupBy(a => a.Title))
        {
          var titleGroup = new WindowTitleActivityGroup(titleActivityGroup.Key);
          titleGroup.Activities.AddRange(titleActivityGroup);
          exePathGroup.WindowTitleGroups.Add(titleGroup);
        }

        exePathGroups.Add(exePathGroup);
      }

      exePathGroups.Reorder();

      return exePathGroups;
    }
  }

  class ExePathActivityGroup : INotifyPropertyChanged
  {
    public WindowTitleActivityGroupCollection WindowTitleGroups { get; } = [];

    public string ExePath { get; }

    public string ExeFileName { get; }

    public double TotalDuration => this.WindowTitleGroups.TotalDuration;

    private IEnumerable<WindowActivity> Activities => this.WindowTitleGroups.SelectMany(a => a.Activities);

    private IEnumerable<ActivityStatistics> AllStatistics => this.WindowTitleGroups.Select(a => a.Statistics);

    public ActivityStatistics Statistics { get; private set; } = ActivityStatistics.Empty;

    public ActivityEvaluationRule? Rule
    {
      get => this._rule;
      set
      {
        if (this._rule == value) return;
        this._rule = value;
        this.OnPropertyChanged();
      }
    }
    private ActivityEvaluationRule? _rule;

    public ExePathActivityGroup(string exePath)
    {
      this.ExePath = exePath;
      this.ExeFileName = Path.GetFileName(this.ExePath);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateStatistics()
    {
      this.WindowTitleGroups.UpdateStatistics();

      this.Statistics = new(this.AllStatistics);

      this.OnPropertyChanged(nameof(this.Statistics));
    }

    public bool FireIfActivityUpdated(WindowActivity activity)
    {
      if (this.WindowTitleGroups.Count(a => a.FireIfActivityUpdated(activity)) == 0)
      {
        return false;
      }

      this.UpdateStatistics();

      this.OnPropertyChanged(nameof(TotalDuration));
      return true;
    }

    public static ExePathActivityGroup FromActivity(WindowActivity activity)
    {
      var exePathGroup = new ExePathActivityGroup(activity.ExePath);
      var titleGroup = new WindowTitleActivityGroup(activity.Title);

      titleGroup.Activities.Add(activity);
      exePathGroup.WindowTitleGroups.Add(titleGroup);

      return exePathGroup;
    }

    private void UpdateEvalucations()
    {
      foreach (var activity in this.Activities.Where(a => a.Rule == this.Rule))
      {
        activity.Rule = this.Rule;
      }
    }

    public async Task SetDefaultEvaluationAsync(ActivityEvaluation evaluation)
    {
      if (this.Rule == null)
      {
        return;
      }

      if (evaluation != this.Rule.Evaluation)
      {
        this.Rule.Evaluation = evaluation;

        using var db = new MainContext();
        await this.Rule.SaveDataAsync(db);

        this.UpdateEvalucations();
      }
    }
  }
}
