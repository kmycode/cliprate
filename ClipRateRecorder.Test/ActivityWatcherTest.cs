using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Watching;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Test
{
  [TestClass]
  public class ActivityWatcherTest
  {
    private DisposableCollection disposables = new();

    [TestInitialize]
    public void Initialize()
    {
      this.disposables.Add(new UseDatabase());
      this.disposables.Add(new StartActivityWatcher());
    }

    [TestCleanup]
    public void CleanUp()
    {
      this.disposables.Dispose();
    }


    [TestMethod]
    public void WatchingTest()
    {
      var beforeLaunch = DateTime.Now;
      this.disposables.Add(new LaunchPaintProcess());
      Task.Delay(1000).Wait();
      this.disposables.Remove<StartActivityWatcher>();
      Task.Delay(1000).Wait();

      using var db = new MainContext();
      var activity = db.WindowActivities!.OrderByDescending(w => w.EndTime).First();
      Assert.IsNotNull(activity);
      Assert.IsTrue(activity.Title?.Contains("ペイント"));
      Assert.IsTrue(activity.ExePath?.EndsWith("mspaint.exe"));
      Assert.IsTrue(activity.StartTime >= beforeLaunch);
      Assert.IsTrue(activity.EndTime <= DateTime.Now);
      Assert.AreNotEqual(activity.EndTime, default);
      Assert.IsTrue(activity.DurationSeconds > 0);
    }

    [TestMethod]
    public void WatchingTestWhenWindowChange()
    {
      this.disposables.Add(new LaunchPaintProcess());
      Task.Delay(1000).Wait();
      this.disposables.Add(new LaunchCalcProcess());
      Task.Delay(1000).Wait();
      this.disposables.Remove<StartActivityWatcher>();
      Task.Delay(1000).Wait();

      using var db = new MainContext();
      var activities = db.WindowActivities!.ToArray();
      var paint = activities.FirstOrDefault(a => a.Title?.Contains("ペイント") ?? false);
      var calc = activities.FirstOrDefault(a => a.Title?.Contains("電卓") ?? false);

      Assert.IsNotNull(paint);
      Assert.IsNotNull(calc);
      Assert.AreNotEqual(paint.ExePath, calc.ExePath);
      Assert.IsTrue(paint.StartTime < calc.StartTime);
      Assert.IsTrue(paint.EndTime < calc.EndTime);
      Assert.IsTrue(paint.EndTime <= calc.StartTime.AddSeconds(0.1));
      Assert.AreNotEqual(calc.EndTime, default);
      Assert.IsTrue(paint.DurationSeconds > 0);
      Assert.IsTrue(calc.DurationSeconds > 0);
    }
  }
}
