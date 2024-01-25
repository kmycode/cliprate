using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Watching;
using ClipRateRecorder.Models.Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public ActivityEvaluator? Evaluator
    {
      get => this._evaluator;
      set
      {
        if (this._evaluator != value)
        {
          this._evaluator = value;
          this.Evaluate();
        }
      }
    }
    private ActivityEvaluator? _evaluator;

    public void OnWindowActivityTicked(object sender, WatchingTickEventArgs e)
    {
      var activity = e.CurrentActivity;
      this.Evaluator?.Evaluate(activity);

      var exePathes = this.Where(a => a.ExePath == activity.ExePath);
      if (!this.FireIfActivityUpdated(activity))
      {
        if (!exePathes.Any())
        {
          this.Add(ExePathActivityGroup.FromActivity(activity));
        }
        else
        {
          var titles = exePathes.SelectMany(e => e.WindowTitleGroups).Where(a => a.Title == activity.Title);
          if (!titles.Any())
          {
            var group = new WindowTitleActivityGroup(activity.Title);
            group.AddActivity(activity);
            exePathes.Last().WindowTitleGroups.Add(group);
          }
        }

        this.FireIfActivityUpdated(activity);
      }
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

    public void Evaluate()
    {
      this.Evaluator?.Evaluate(this.Activities);
    }

    public void UpdateStatistics()
    {
      foreach (var item in this)
      {
        item.UpdateStatistics();
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

      return exePathGroups;
    }
  }

  class ExePathActivityGroup : INotifyPropertyChanged
  {
    public WindowTitleActivityGroupCollection WindowTitleGroups { get; } = [];

    public string ExePath { get; }

    public double TotalDuration => this.WindowTitleGroups.TotalDuration;

    private IEnumerable<WindowActivity> Activities => this.WindowTitleGroups.SelectMany(a => a.Activities);

    public ActivityStatistics Statistics { get; private set; } = ActivityStatistics.Empty;

    public ExePathActivityGroup(string exePath)
    {
      this.ExePath = exePath;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void UpdateStatistics()
    {
      this.WindowTitleGroups.UpdateStatistics();

      this.Statistics = new(this.Activities);

      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Statistics)));
    }

    public bool FireIfActivityUpdated(WindowActivity activity)
    {
      if (this.WindowTitleGroups.Count(a => a.FireIfActivityUpdated(activity)) == 0)
      {
        return false;
      }

      this.UpdateStatistics();
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
  }
}
