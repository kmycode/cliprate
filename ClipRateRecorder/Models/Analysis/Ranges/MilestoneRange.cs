using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Db.Entities;
using ClipRateRecorder.Models.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipRateRecorder.Models.Analysis.Groups;
using ClipRateRecorder.Models.Window;
using System.Collections.ObjectModel;
using ClipRateRecorder.Models.Goals;
using ClipRateRecorder.Models.Watching;

namespace ClipRateRecorder.Models.Analysis.Ranges
{
  internal class MilestoneRange
  {
    public ObservableCollection<Milestone> Milestones { get; }

    private MilestoneRange(IEnumerable<MilestoneData> dataSet)
    {
      this.Milestones = new ObservableCollection<Milestone>(dataSet.OrderBy(a => a.StartTime).Select(Milestone.FromData));

      this.Milestones.Add(new Milestone
      {
        EndHours = 20,
        EndMinutes = 0,
        StartHours = 0,
        StartMinutes = 0,
        Target = MilestoneTarget.AllEffective,
        Type = MilestoneType.More,
        Value = 30,
      });
    }

    internal void OnActivityStatisticsUpdated(object? sender, StatisticsChangedEventArgs e)
    {
      var exeGroups = e.ExeGroups;

      foreach (var milestone in this.Milestones)
      {
        milestone.UpdateStatus(exeGroups.Activities);
      }
    }

    private static async Task<MilestoneRange> CreateInstanceAsync(MainContext db, IQueryable<MilestoneData> dataSet)
    {
      var list = await dataSet.ToListAsync();
      var range = new MilestoneRange(list);
      await db.SaveChangesAsync();
      return range;
    }

    private static async Task<MilestoneRange> CreateInstanceAsync(DateTime start, DateTime end)
    {
      using var db = new MainContext();
      return await CreateInstanceAsync(db, db.Milestones!.Where(a => a.StartTime >= start && a.StartTime < end));
    }

    public static MilestoneRange RangeOfEmpty()
    {
      using var db = new MainContext();
      var range = new MilestoneRange(Enumerable.Empty<MilestoneData>());
      return range;
    }

    public static async Task<MilestoneRange> RangeOfDayAsync(DateTime day)
    {
      var d = day.Date;
      return await CreateInstanceAsync(d, d.AddDays(1));
    }
  }
}
