using ClipRateRecorder.Models.Analysis.Ranges;
using ClipRateRecorder.Models.Analysis.Rules;
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

    public MainWatcherModel()
    {
      this.loop = ActivityWatcher.StartWatchLoop();

      Task.Run(async () =>
      {
        var range = await ActivityRange.RangeOfDayAsync(DateTime.Now);
        ThreadUtil.RunGuiThread(() => this.Range = range);
        this.loop.Ticked += range.ActivityGroups.OnWindowActivityTicked;
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
  }
}
