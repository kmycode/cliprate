using ClipRateRecorder.Models.Analysis.Ranges;
using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Goals;
using ClipRateRecorder.Models.Watching;
using ClipRateRecorder.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Logics
{
  internal class MainWatcherModel : IDisposable, INotifyPropertyChanged
  {
    private readonly IWatcherLoop loop;

    public event PropertyChangedEventHandler? PropertyChanged;

    public DateTime CurrentDay
    {
      get => this._currentDay;
      set
      {
        if (this._currentDay != value)
        {
          this._currentDay = value;
          this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentDay)));
        }
      }
    }
    private DateTime _currentDay;

    public ActivityRange? Range
    {
      get => this._range;
      set
      {
        if (this._range != value)
        {
          this._range = value;
          this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Range)));
        }
      }
    }
    private ActivityRange? _range;

    public ActivityRange? SpotRange
    {
      get => this._spotRange;
      set
      {
        if (this._spotRange != value)
        {
          this._spotRange = value;
          this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpotRange)));
        }
      }
    }
    private ActivityRange? _spotRange;

    public MilestoneRange? MilestoneRange
    {
      get => this._milestoneRange;
      set
      {
        if (this._milestoneRange != value)
        {
          this._milestoneRange = value;
          this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MilestoneRange)));
        }
      }
    }
    private MilestoneRange? _milestoneRange;

    public MainWatcherModel()
    {
      this.loop = ActivityWatcher.StartWatchLoop();

      Task.Run(async () => await this.ChangeDayAsync(DateTime.Now, withSpot: true));
    }

    private async Task ChangeDayAsync(DateTime day, bool withSpot = false)
    {
      if (this.Range != null && this.MilestoneRange != null && this.SpotRange != null)
      {
        this.loop.Ticked -= this.Range.ActivityGroups.OnWindowActivityTicked;
        this.Range!.ActivityGroups.StatisticsUpdated -= this.MilestoneRange.OnActivityStatisticsUpdated;
        if (withSpot)
        {
          this.loop.Ticked -= this.SpotRange.ActivityGroups.OnWindowActivityTicked;
        }
      }

      this.Range?.Dispose();
      this.SpotRange?.Dispose();

      using var db = new MainContext();
      var evalucator = await ActivityEvaluator.CreateFromDatabaseAsync(db);

      var range = await ActivityRange.RangeOfDayAsync(day, evalucator);
      var spotRange = await ActivityRange.RangeOfEmptyAsync(evalucator);
      var milestone = await MilestoneRange.RangeOfDayAsync(day);
      milestone.UpdateStatuses(range.ActivityGroups);

      ThreadUtil.RunGuiThread(() =>
      {
        this.Range = range;
        this.MilestoneRange = milestone;
        if (withSpot)
        {
          this.SpotRange = spotRange;
        }
        this.CurrentDay = day;
      });

      if (DateOnly.FromDateTime(day) == DateOnly.FromDateTime(DateTime.Now))
      {
        this.loop.Ticked += range.ActivityGroups.OnWindowActivityTicked;
        range.ActivityGroups.StatisticsUpdated += milestone.OnActivityStatisticsUpdated;
        if (withSpot)
        {
          this.loop.Ticked += spotRange.ActivityGroups.OnWindowActivityTicked;
        }
      }
    }

    public async Task StepPrewDayAsync()
    {
      await this.ChangeDayAsync(this.CurrentDay.AddDays(-1));
    }

    public async Task StepNextDayAsync()
    {
      await this.ChangeDayAsync(this.CurrentDay.AddDays(1));
    }

    public void Dispose()
    {
      var task = this.loop!.DisposeAsync();

      while (!(task.IsCompleted || task.IsFaulted))
      {
        Thread.Sleep(500);
      }
    }

    internal void ResetSpot()
    {
      if (this.SpotRange == null)
      {
        return;
      }

      this.loop!.Ticked -= this.SpotRange.ActivityGroups.OnWindowActivityTicked;

      Task.Run(async () =>
      {
        using var db = new MainContext();
        var evalucator = this.SpotRange.ActivityGroups.Evaluator;

        var spotRange = await ActivityRange.RangeOfEmptyAsync(evalucator);
        ThreadUtil.RunGuiThread(() => this.SpotRange = spotRange);

        this.loop.Ticked += spotRange.ActivityGroups.OnWindowActivityTicked;
      });
    }

    internal async Task SetDefaultEvaluationAsync(string exePath, string ev)
    {
      var evaluation = EnumUtils.StringToActivityEvaluation(ev);

      await this.Range!.ActivityGroups.SetDefaultEvaluationAsync(exePath, evaluation);
      await this.SpotRange!.ActivityGroups.SetDefaultEvaluationAsync(exePath, evaluation);
    }

    public async Task AddMilestoneAsync()
    {
      if (this.MilestoneRange == null)
      {
        return;
      }

      await this.MilestoneRange.AddMilestoneAsync(this.CurrentDay);
    }

    public async Task RemoveMilestoneAsync(Milestone milestone)
    {
      if (this.MilestoneRange == null)
      {
        return;
      }

      using var db = new MainContext();
      await milestone.RemoveDataAndSaveAsync(db);
      this.MilestoneRange.Milestones.Remove(milestone);
    }
  }
}
