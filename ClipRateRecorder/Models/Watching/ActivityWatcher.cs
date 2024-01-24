using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Watching
{
  class ActivityWatcher
  {
    public void StartWatchLoop(CancellationToken token = default)
    {
      new LoopClass(token);
    }

    private class LoopClass
    {
      private WindowActivity? Current { get; set; }

      private CancellationToken Token { get; }

      public LoopClass(CancellationToken token)
      {
        this.Token = token;
        Task.Run(this.Loop);
      }

      private async Task Loop()
      {
        while (true)
        {
          if (this.Token.IsCancellationRequested)
          {
            await this.RecordActivityAsync();
            throw new TaskCanceledException();
          }

          try
          {
            await this.WatchLatestActivityAsync();
            await Task.Delay(700);
          }
          catch
          {
          }
        }
      }

      private async Task WatchLatestActivityAsync()
      {
        var latest = WindowActivityInspector.GetCurrentActivity();

        if (this.Current != null)
        {
          if (!this.Current.IsSameProcess(latest))
          {
            await this.UpdateActivityAsync(latest);
          }
        }
        else
        {
          await this.UpdateActivityAsync(latest);
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
}
