using ClipRateRecorder.Models.Analysis.Ranges;
using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Db.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Test
{
  [TestClass]
  public class ActivityRangeTest
  {
    private readonly DisposableCollection disposables = new();

    [TestInitialize]
    public void Initialize()
    {
      this.disposables.Add(new UseDatabase());
    }

    [TestCleanup]
    public void CleanUp()
    {
      this.disposables.Dispose();
    }

    [TestMethod]
    public void RangeOfDayTest()
    {
      using var db = new MainContext();
      db.WindowActivities!.Add(new WindowActivityData
      {
        Title = "Notepad",
        ExePath = "C:\\notepad.exe",
        StartTime = new DateTime(2024, 1, 15, 1, 0, 0),
        EndTime = new DateTime(2024, 1, 15, 2, 0, 0),
      });
      db.SaveChanges();

      var range = ActivityRange.RangeOfDay(new DateTime(2024, 1, 15)).Result;

      Assert.AreEqual(range.ActivityGroups.Count, 1);
      Assert.AreEqual(range.ActivityGroups.TotalDuration, 3600);
    }

    [TestMethod]
    public void RangeOfDayOutOfActivityTest()
    {
      using var db = new MainContext();
      db.WindowActivities!.Add(new WindowActivityData
      {
        Title = "Notepad",
        ExePath = "C:\\notepad.exe",
        StartTime = new DateTime(2024, 1, 15, 1, 0, 0),
        EndTime = new DateTime(2024, 1, 15, 2, 0, 0),
      });
      db.SaveChanges();

      var range = ActivityRange.RangeOfDay(new DateTime(2024, 1, 16)).Result;

      Assert.AreEqual(range.ActivityGroups.Count, 0);
      Assert.AreEqual(range.ActivityGroups.TotalDuration, 0);
    }
  }
}
