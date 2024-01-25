using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Watching;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.EnterpriseData;

namespace ClipRateRecorder.Test
{
  internal class DisposableCollection : IDisposable
  {
    private Collection<IDisposable> disposables = new();

    public void Add(IDisposable disposable)
    {
      this.disposables.Add(disposable);
    }

    public void Call<T>(Action<T> action)
    {
      var targets = this.disposables.OfType<T>();
      if (!targets.Any())
      {
        throw new InvalidOperationException();
      }

      foreach (var obj in targets)
      {
        action(obj);
      }
    }

    public void Dispose()
    {
      foreach (var obj in this.disposables)
      {
        obj.Dispose();
      }
    }

    public void Remove<T>() where T : IDisposable
    {
      var targets = this.disposables.OfType<T>().ToArray();
      if (!targets.Any())
      {
        throw new InvalidOperationException();
      }

      foreach (var obj in targets)
      {
        obj.Dispose();
        this.disposables.Remove(obj);
      }
    }
  }

  internal abstract class LaunchProcess : IDisposable
  {
    private Process? testProcess;

    protected LaunchProcess(string processName)
    {
      this.testProcess = Process.Start(processName);
      this.testProcess.WaitForInputIdle();
      Task.Delay(500).Wait();
    }

    public void Dispose()
    {
      this.testProcess?.CloseMainWindow();
      this.testProcess?.Close();
      Task.Delay(500).Wait();
    }
  }

  internal class LaunchPaintProcess : LaunchProcess
  {
    public LaunchPaintProcess() : base("mspaint")
    {
    }
  }

  internal class LaunchCalcProcess : LaunchProcess
  {
    public LaunchCalcProcess() : base("calc")
    {
    }
  }

  internal class UseDatabase : IDisposable
  {
    public UseDatabase()
    {
      using var db = new MainContext();
      db.Database.Migrate();
      db.WindowActivities!.ExecuteDelete();
    }

    public void Dispose()
    {
      using var db = new MainContext();
      db.WindowActivities!.ExecuteDelete();
    }
  }

  internal class StartActivityWatcher : IDisposable
  {
    private CancellationTokenSource cancellation = new();

    public StartActivityWatcher()
    {
      ActivityWatcher.StartWatchLoop(this.cancellation.Token);
    }

    public void Dispose()
    {
      if (!this.cancellation.IsCancellationRequested)
      {
        this.cancellation.Cancel();
      }
    }
  }
}
