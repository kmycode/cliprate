using ClipRateRecorder.Models.Analysis.Groups;
using ClipRateRecorder.Models.Db.Entities;
using ClipRateRecorder.Models.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Test
{
  [TestClass]
  public class ActivityGroupTest
  {
    private DisposableCollection disposables = new();

    [TestInitialize]
    public void Initialize()
    {
      // this.disposables.Add(new UseDatabase());
    }

    [TestCleanup]
    public void CleanUp()
    {
      this.disposables.Dispose();
    }

    [TestMethod]
    public void CreateSimpleGroupTest()
    {
      var activities = new WindowActivityData[]
      {
        new()
        {
          Title = "Calc",
          ExePath = "c:\\calc.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 0, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 10, 0),
        },
      };
      var group = ExePathActivityGroupCollection.FromActivities(activities.Select(WindowActivity.FromData));

      var exePath = group.FirstOrDefault();
      var title = exePath?.WindowTitleGroups.FirstOrDefault();

      Assert.AreEqual(group.Count, 1);
      Assert.IsNotNull(exePath);
      Assert.AreEqual(exePath.WindowTitleGroups.Count, 1);
      Assert.IsNotNull(title);
      Assert.AreEqual(title.Activities.Count, 1);

      Assert.AreEqual(group.TotalDuration, 10 * 60);
      Assert.AreEqual(exePath.TotalDuration, 10 * 60);
      Assert.AreEqual(exePath.WindowTitleGroups.TotalDuration, 10 * 60);
      Assert.AreEqual(title.TotalDuration, 10 * 60);

      Assert.AreEqual(exePath.ExePath, "c:\\calc.exe");
      Assert.AreEqual(title.Title, "Calc");
    }

    [TestMethod]
    public void CreateSingleExePathMultipleTitlesGroupTest()
    {
      var activities = new WindowActivityData[]
      {
        new()
        {
          Title = "Calc",
          ExePath = "c:\\calc.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 0, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 5, 0),
        },
        new()
        {
          Title = "Editing - Calc",
          ExePath = "c:\\calc.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 5, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 9, 0),
        },
      };
      var group = ExePathActivityGroupCollection.FromActivities(activities.Select(WindowActivity.FromData));

      var exePath = group.FirstOrDefault();
      var title = exePath?.WindowTitleGroups.ElementAtOrDefault(0);
      var title2 = exePath?.WindowTitleGroups.ElementAtOrDefault(1);

      Assert.AreEqual(group.Count, 1);
      Assert.IsNotNull(exePath);
      Assert.AreEqual(exePath.WindowTitleGroups.Count, 2);
      Assert.IsNotNull(title);
      Assert.AreEqual(title.Activities.Count, 1);
      Assert.IsNotNull(title2);
      Assert.AreEqual(title2.Activities.Count, 1);

      Assert.AreEqual(group.TotalDuration, 9 * 60);
      Assert.AreEqual(exePath.TotalDuration, 9 * 60);
      Assert.AreEqual(exePath.WindowTitleGroups.TotalDuration, 9 * 60);
      Assert.AreEqual(title.TotalDuration, 5 * 60);
      Assert.AreEqual(title2.TotalDuration, 4 * 60);

      Assert.AreEqual(exePath.ExePath, "c:\\calc.exe");
      Assert.AreEqual(title.Title, "Calc");
      Assert.AreEqual(title2.Title, "Editing - Calc");
    }

    [TestMethod]
    public void CreateMultipleExePathesGroupTest()
    {
      var activities = new WindowActivityData[]
      {
        new()
        {
          Title = "Calc",
          ExePath = "c:\\calc.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 0, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 5, 0),
        },
        new()
        {
          Title = "Notepad",
          ExePath = "c:\\notepad.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 5, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 9, 0),
        },
      };
      var group = ExePathActivityGroupCollection.FromActivities(activities.Select(WindowActivity.FromData));

      var exePath = group.ElementAtOrDefault(0);
      var exePath2 = group.ElementAtOrDefault(1);
      var title = exePath?.WindowTitleGroups.FirstOrDefault();
      var title2 = exePath2?.WindowTitleGroups.FirstOrDefault();

      Assert.AreEqual(group.Count, 2);
      Assert.IsNotNull(exePath);
      Assert.AreEqual(exePath.WindowTitleGroups.Count, 1);
      Assert.IsNotNull(exePath2);
      Assert.AreEqual(exePath2.WindowTitleGroups.Count, 1);
      Assert.IsNotNull(title);
      Assert.AreEqual(title.Activities.Count, 1);
      Assert.IsNotNull(title2);
      Assert.AreEqual(title2.Activities.Count, 1);

      Assert.AreEqual(group.TotalDuration, 9 * 60);
      Assert.AreEqual(exePath.TotalDuration, 5 * 60);
      Assert.AreEqual(exePath.WindowTitleGroups.TotalDuration, 5 * 60);
      Assert.AreEqual(title.TotalDuration, 5 * 60);
      Assert.AreEqual(exePath2.TotalDuration, 4 * 60);
      Assert.AreEqual(exePath2.WindowTitleGroups.TotalDuration, 4 * 60);
      Assert.AreEqual(title2.TotalDuration, 4 * 60);

      Assert.AreEqual(exePath.ExePath, "c:\\calc.exe");
      Assert.AreEqual(exePath2.ExePath, "c:\\notepad.exe");
      Assert.AreEqual(title.Title, "Calc");
      Assert.AreEqual(title2.Title, "Notepad");
    }

    [TestMethod]
    public void CreateMultipleExePathesMultipleTitlesGroupTest()
    {
      var activities = new WindowActivityData[]
      {
        new()
        {
          Title = "Calc",
          ExePath = "c:\\calc.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 0, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 5, 0),
        },
        new()
        {
          Title = "Notepad",
          ExePath = "c:\\notepad.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 5, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 9, 0),
        },
        new()
        {
          Title = "Editing - Calc",
          ExePath = "c:\\calc.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 9, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 11, 0),
        },
      };
      var group = ExePathActivityGroupCollection.FromActivities(activities.Select(WindowActivity.FromData));

      var exePath = group.ElementAtOrDefault(0);
      var exePath2 = group.ElementAtOrDefault(1);
      var title = exePath?.WindowTitleGroups.ElementAtOrDefault(0);
      var title2 = exePath2?.WindowTitleGroups.FirstOrDefault();
      var title3 = exePath?.WindowTitleGroups.ElementAtOrDefault(1);

      Assert.AreEqual(group.Count, 2);
      Assert.IsNotNull(exePath);
      Assert.AreEqual(exePath.WindowTitleGroups.Count, 2);
      Assert.IsNotNull(exePath2);
      Assert.AreEqual(exePath2.WindowTitleGroups.Count, 1);
      Assert.IsNotNull(title);
      Assert.AreEqual(title.Activities.Count, 1);
      Assert.IsNotNull(title2);
      Assert.AreEqual(title2.Activities.Count, 1);
      Assert.IsNotNull(title3);
      Assert.AreEqual(title3.Activities.Count, 1);

      Assert.AreEqual(group.TotalDuration, 11 * 60);
      Assert.AreEqual(exePath.TotalDuration, 7 * 60);
      Assert.AreEqual(exePath.WindowTitleGroups.TotalDuration, 7 * 60);
      Assert.AreEqual(title.TotalDuration, 5 * 60);
      Assert.AreEqual(title3.TotalDuration, 2 * 60);
      Assert.AreEqual(exePath2.TotalDuration, 4 * 60);
      Assert.AreEqual(exePath2.WindowTitleGroups.TotalDuration, 4 * 60);
      Assert.AreEqual(title2.TotalDuration, 4 * 60);

      Assert.AreEqual(exePath.ExePath, "c:\\calc.exe");
      Assert.AreEqual(exePath2.ExePath, "c:\\notepad.exe");
      Assert.AreEqual(title.Title, "Calc");
      Assert.AreEqual(title2.Title, "Notepad");
      Assert.AreEqual(title3.Title, "Editing - Calc");
    }

    [TestMethod]
    public void CreateSameExePathAndTitleMultipleActivitiesGroupTest()
    {
      var activities = new WindowActivityData[]
      {
        new()
        {
          Title = "Calc",
          ExePath = "c:\\calc.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 0, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 10, 0),
        },
        new()
        {
          Title = "Calc",
          ExePath = "c:\\calc.exe",
          StartTime = new DateTime(2024, 1, 3, 5, 10, 0),
          EndTime = new DateTime(2024, 1, 3, 5, 20, 0),
        },
      };
      var group = ExePathActivityGroupCollection.FromActivities(activities.Select(WindowActivity.FromData));

      var exePath = group.FirstOrDefault();
      var title = exePath?.WindowTitleGroups.FirstOrDefault();

      Assert.AreEqual(group.Count, 1);
      Assert.IsNotNull(exePath);
      Assert.AreEqual(exePath.WindowTitleGroups.Count, 1);
      Assert.IsNotNull(title);
      Assert.AreEqual(title.Activities.Count, 2);

      Assert.AreEqual(group.TotalDuration, 20 * 60);
      Assert.AreEqual(exePath.TotalDuration, 20 * 60);
      Assert.AreEqual(exePath.WindowTitleGroups.TotalDuration, 20 * 60);
      Assert.AreEqual(title.TotalDuration, 20 * 60);

      Assert.AreEqual(exePath.ExePath, "c:\\calc.exe");
      Assert.AreEqual(title.Title, "Calc");
    }
  }
}
