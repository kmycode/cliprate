using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Watching
{
  interface IWatcherLoop : IAsyncDisposable
  {
    event EventHandler<WatchingTickEventArgs> Ticked;
  }

  static class ActivityWatcher
  {
    public static IWatcherLoop? Default { get; private set; }

    public static IWatcherLoop StartWatchLoop(CancellationToken token = default)
    {
      var loop = new LoopClass(token);

      if (Default == null)
      {
        Default = loop;
      }

      return loop;
    }

    private class LoopClass : IWatcherLoop
    {
      private WindowActivity? Current { get; set; }

      private CancellationToken Token { get; }

      public bool IsFinished => this.Token.IsCancellationRequested;

      public event EventHandler<WatchingTickEventArgs>? Ticked;

      public LoopClass(CancellationToken token)
      {
        this.Token = token;
        Task.Run(this.Loop);
      }

      public async ValueTask DisposeAsync()
      {
        await this.RecordActivityAsync();
        throw new TaskCanceledException();
      }

      private async Task Loop()
      {
        while (true)
        {
          if (this.Token.IsCancellationRequested)
          {
            await this.DisposeAsync();
          }

          try
          {
            await this.CheckOrUpdateLatestActivityAsync();
            await Task.Delay(700);
          }
          catch
          {
            // TODO: Log error
          }
        }
      }

      private async Task CheckOrUpdateLatestActivityAsync()
      {
        var latest = WindowActivityInspector.GetCurrentActivity();

        if (this.Current != null)
        {
          if (!this.Current.IsSameWindow(latest))
          {
            await this.UpdateActivityAsync(latest);
          }
        }
        else
        {
          await this.UpdateActivityAsync(latest);
        }

        if (this.Current != null)
        {
          this.Ticked?.Invoke(this, new WatchingTickEventArgs(this.Current));
        }
      }

      private async Task UpdateActivityAsync(WindowActivity activity)
      {
        if (this.Current != null)
        {
          await this.RecordActivityAsync(this.Current);
        }

        this.Current = activity;
      }

      private async Task RecordActivityAsync(WindowActivity activity)
      {
        if (!activity.IsValid)
        {
          activity.Stop();
        }

        var data = activity.GenerateData();

        using (var db = new MainContext())
        {
          await db.WindowActivities!.AddAsync(data);
          await db.SaveChangesAsync();
        }
      }

      private async Task RecordActivityAsync()
      {
        if (this.Current != null)
        {
          await this.RecordActivityAsync(this.Current);
        }
      }
    }
  }

  internal class WatchingTickEventArgs : EventArgs
  {
    public WindowActivity CurrentActivity { get; }

    public WatchingTickEventArgs(WindowActivity currentActivity)
    {
      this.CurrentActivity = currentActivity;
    }
  }
}
