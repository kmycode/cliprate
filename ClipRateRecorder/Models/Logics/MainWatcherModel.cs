using ClipRateRecorder.Models.Analysis.Ranges;
using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Db;
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
    private IWatcherLoop? loop;

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

    public MainWatcherModel()
    {
      Task.Run(async () => await this.ChangeDayAsync(DateTime.Now, withSpot: true));
    }

    private async Task ChangeDayAsync(DateTime day, bool withSpot = false)
    {
      if (this.loop != null)
      {
        this.loop.Ticked -= this.Range!.ActivityGroups.OnWindowActivityTicked;

        if (withSpot)
        {
          this.loop.Ticked -= this.SpotRange!.ActivityGroups.OnWindowActivityTicked;
        }
      }

      this.loop = ActivityWatcher.StartWatchLoop();

      using var db = new MainContext();
      var evalucator = await ActivityEvaluator.CreateFromDatabaseAsync(db);

      var range = await ActivityRange.RangeOfDayAsync(day, evalucator);
      var spotRange = await ActivityRange.RangeOfEmptyAsync(evalucator);

      ThreadUtil.RunGuiThread(() =>
      {
        this.Range = range;
        if (withSpot)
        {
          this.SpotRange = spotRange;
        }
        this.CurrentDay = day;
      });

      if (DateOnly.FromDateTime(day) == DateOnly.FromDateTime(DateTime.Now))
      {
        this.loop.Ticked += range.ActivityGroups.OnWindowActivityTicked;
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
  }
}
