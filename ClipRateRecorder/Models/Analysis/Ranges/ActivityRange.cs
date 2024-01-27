using ClipRateRecorder.Models.Analysis.Groups;
using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Db.Entities;
using ClipRateRecorder.Models.Window;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Analysis.Ranges
{
  internal class ActivityRange : INotifyPropertyChanged, IDisposable
  {
    private ObservableCollection<WindowActivity> Activities { get; }

    public ExePathActivityGroupCollection ActivityGroups { get; }

    public ActivityStatistics? Statistics => this.ActivityGroups.Statistics;

    public event PropertyChangedEventHandler? PropertyChanged;

    private ActivityRange(IEnumerable<WindowActivityData> dataSet, ActivityEvaluator evalucator)
    {
      this.Activities = new ObservableCollection<WindowActivity>(dataSet.OrderBy(a => a.StartTime).Select(WindowActivity.FromData));
      this.ActivityGroups = ExePathActivityGroupCollection.FromActivities(this.Activities, evalucator);

      this.ActivityGroups.StatisticsChanged += this.ActivityGroups_StatisticsChanged;
    }

    public void Dispose()
    {
      this.ActivityGroups.StatisticsChanged -= this.ActivityGroups_StatisticsChanged;
    }

    private void ActivityGroups_StatisticsChanged(object? sender, EventArgs e)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Statistics)));
    }

    private static async Task<ActivityRange> CreateInstanceAsync(MainContext db, IQueryable<WindowActivityData> dataSet) 
    {
      var list = await dataSet.ToListAsync();
      var evalucator = await ActivityEvaluator.CreateFromDatabaseAsync(db);
      var range = new ActivityRange(list, evalucator);
      await db.SaveChangesAsync();
      return range;
    }

    private static async Task<ActivityRange> CreateInstanceAsync(DateTime start, DateTime end)
    {
      using var db = new MainContext();
      return await CreateInstanceAsync(db, db.WindowActivities!.Where(a => a.StartTime >= start && a.StartTime < end));
    }

    public static async Task<ActivityRange> RangeOfDayAsync(DateTime day)
    {
      var d = new DateOnly(day.Year, day.Month, day.Day).ToDateTime(TimeOnly.MinValue);
      return await CreateInstanceAsync(d, d.AddDays(1));
    }

    public static async Task<ActivityRange> RangeOfLatestAsync(int limit)
    {
      using var db = new MainContext();
      return await CreateInstanceAsync(db, db.WindowActivities!.OrderByDescending(a => a.StartTime).Take(limit));
    }
  }
}
