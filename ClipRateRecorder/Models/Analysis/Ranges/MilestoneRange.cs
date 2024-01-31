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

    private MilestoneRange(IEnumerable<MilestoneData> dataSet, DateTime start, DateTime end)
    {
      this.Milestones = new ObservableCollection<Milestone>(dataSet.OrderBy(a => a.StartTime).Select(Milestone.FromData));
    }

    public void UpdateStatuses(ExePathActivityGroupCollection exeGroups)
    {
      foreach (var milestone in this.Milestones)
      {
        milestone.UpdateStatus(exeGroups.Activities);
      }
    }

    internal void OnActivityStatisticsUpdated(object? sender, StatisticsChangedEventArgs e)
    {
      var exeGroups = e.ExeGroups;
      this.UpdateStatuses(exeGroups);
    }

    public async Task AddMilestoneAsync(DateTime day)
    {
      var milestone = new Milestone(day);

      this.Milestones.Add(milestone);

      using var db = new MainContext();
      await milestone.SaveDataAsync(db);
    }

    public async Task RemoveMilestoneAsync(Milestone milestone)
    {
      using var db = new MainContext();
      await milestone.RemoveDataAndSaveAsync(db);
    }

    private static async Task<MilestoneRange> CreateInstanceAsync(MainContext db, IQueryable<MilestoneData> dataSet, DateTime start, DateTime end)
    {
      var list = await dataSet.ToListAsync();
      var range = new MilestoneRange(list, start, end);
      await db.SaveChangesAsync();
      return range;
    }

    private static async Task<MilestoneRange> CreateInstanceAsync(DateTime start, DateTime end)
    {
      using var db = new MainContext();
      return await CreateInstanceAsync(db, db.Milestones!.Where(a => a.StartTime >= start && a.StartTime < end), start, end);
    }

    public static async Task<MilestoneRange> RangeOfDayAsync(DateTime day)
    {
      var d = day.Date;
      return await CreateInstanceAsync(d, d.AddDays(1));
    }
  }
}
