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
    private readonly IWatcherLoop loop;

    public event PropertyChangedEventHandler? PropertyChanged;

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
      this.loop = ActivityWatcher.StartWatchLoop();

      Task.Run(async () =>
      {
        using var db = new MainContext();
        var evalucator = await ActivityEvaluator.CreateFromDatabaseAsync(db);

        var range = await ActivityRange.RangeOfDayAsync(DateTime.Now, evalucator);
        ThreadUtil.RunGuiThread(() => this.Range = range);

        var spotRange = await ActivityRange.RangeOfEmptyAsync(evalucator);
        ThreadUtil.RunGuiThread(() => this.SpotRange = spotRange);

        this.loop.Ticked += range.ActivityGroups.OnWindowActivityTicked;
        this.loop.Ticked += spotRange.ActivityGroups.OnWindowActivityTicked;
      });
    }

    public void Dispose()
    {
      var task = this.loop.DisposeAsync();

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

      this.loop.Ticked -= this.SpotRange.ActivityGroups.OnWindowActivityTicked;

      Task.Run(async () =>
      {
        using var db = new MainContext();
        var evalucator = this.SpotRange.ActivityGroups.Evaluator;

        var spotRange = await ActivityRange.RangeOfEmptyAsync(evalucator);
        ThreadUtil.RunGuiThread(() => this.SpotRange = spotRange);

        this.loop.Ticked += spotRange.ActivityGroups.OnWindowActivityTicked;
      });
    }
  }
}
