using ClipRateRecorder.Models.Analysis.Groups;
using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Db.Entities;
using ClipRateRecorder.Models.Window;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Analysis.Ranges
{
  internal class ActivityRange
  {
    private ObservableCollection<WindowActivity> Activities { get; }

    public ExePathActivityGroupCollection ActivityGroups { get; }

    private ActivityRange(IEnumerable<WindowActivityData> dataSet)
    {
      this.Activities = new ObservableCollection<WindowActivity>(dataSet.OrderBy(a => a.StartTime).Select(WindowActivity.FromData));
      this.ActivityGroups = ExePathActivityGroupCollection.FromActivities(this.Activities);
    }

    private static async Task<ActivityRange> CreateInstanceAsync(IQueryable<WindowActivityData> dataSet) 
    {
      var list = await dataSet.ToListAsync();
      return new ActivityRange(list);
    }

    private static async Task<ActivityRange> CreateInstanceAsync(DateTime start, DateTime end)
    {
      using var db = new MainContext();
      return await CreateInstanceAsync(db.WindowActivities!.Where(a => a.StartTime >= start && a.StartTime < end));
    }

    public static async Task<ActivityRange> RangeOfDay(DateTime day)
    {
      var d = new DateOnly(day.Year, day.Month, day.Day).ToDateTime(TimeOnly.MinValue);
      return await CreateInstanceAsync(d, d.AddDays(1));
    }

    public static async Task<ActivityRange> RangeOfLatest(int limit)
    {
      using var db = new MainContext();
      return await CreateInstanceAsync(db.WindowActivities!.OrderByDescending(a => a.StartTime).Take(limit));
    }
  }
}
