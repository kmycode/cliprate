using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Db;
using ClipRateRecorder.Models.Db.Entities;
using ClipRateRecorder.Models.Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Goals
{
  internal class Milestone : INotifyPropertyChanged
  {
    private MilestoneData? Data { get; set; }

    private IEnumerable<WindowActivity>? lastActivities;
    private bool isInitializing;
    
    public MilestoneType Type
    {
      get => this._type;
      set
      {
        if (this._type == value) return;
        this._type = value;
        this.OnPropertyChanged();
        this.UpdateStatusWithLatestData();
        this.SaveData();
      }
    }
    private MilestoneType _type;

    public MilestoneTarget Target
    {
      get => this._target;
      set
      {
        if (this._target == value) return;
        this._target = value;
        this.OnPropertyChanged();
        this.UpdateStatusWithLatestData();
        this.SaveData();
      }
    }
    private MilestoneTarget _target;

    public double Value
    {
      get => this._value;
      set
      {
        if (this._value == value) return;
        this._value = value;
        this.OnPropertyChanged();
        this.UpdateStatusWithLatestData();
        this.SaveData();
      }
    }
    private double _value;

    public double CurrentValue
    {
      get => this._currentValue;
      set
      {
        if (this._currentValue == value) return;
        this._currentValue = value;
        this.OnPropertyChanged();
      }
    }
    private double _currentValue;

    public DateTime StartTime
    {
      get => this._startTime;
      private set
      {
        if (this._startTime == value) return;
        this._startTime = value;
        this.OnPropertyChanged();
        this.SaveData();
      }
    }
    private DateTime _startTime;

    public DateTime EndTime
    {
      get => this._endTime;
      private set
      {
        if (this._endTime == value) return;
        this._endTime = value;
        this.OnPropertyChanged();
        this.SaveData();
      }
    }
    private DateTime _endTime;

    public int StartHours
    {
      get => this._startHours;
      set
      {
        if (this._startHours == value) return;
        this._startHours = value;
        this.OnPropertyChanged();
        this.UpdateStartTime();
      }
    }
    private int _startHours;

    public int StartMinutes
    {
      get => this._startMinutes;
      set
      {
        if (this._startMinutes == value) return;
        this._startMinutes = value;
        this.OnPropertyChanged();
        this.UpdateStartTime();
      }
    }
    private int _startMinutes;

    public int EndHours
    {
      get => this._endHours;
      set
      {
        if (this._endHours == value) return;
        this._endHours = value;
        this.OnPropertyChanged();
        this.UpdateEndTime();
      }
    }
    private int _endHours;

    public int EndMinutes
    {
      get => this._endMinutes;
      set
      {
        if (this._endMinutes == value) return;
        this._endMinutes = value;
        this.OnPropertyChanged();
        this.UpdateEndTime();
      }
    }
    private int _endMinutes;

    public MilestoneStatus Status
    {
      get => this._status;
      set
      {
        if (this._status == value) return;
        this._status = value;
        this.OnPropertyChanged();
        this.SaveData();
      }
    }
    private MilestoneStatus _status;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task SaveDataAsync(MainContext db)
    {
      if (this.Data == null)
      {
        this.Data = new();
        await db.Milestones!.AddAsync(this.Data);
      }
      else
      {
        db.Attach(this.Data);
      }

      this.SetData();

      await db.SaveChangesAsync();
    }

    private void SaveData()
    {
      if (this.isInitializing)
      {
        return;
      }

      Task.Run(async () =>
      {
        using var db = new MainContext();
        await this.SaveDataAsync(db);
      });
    }

    private void UpdateStartTime()
    {
      this.StartTime = this.StartTime.Date.Add(new TimeSpan(this.StartHours, this.StartMinutes, 0));
      this.UpdateStatusWithLatestData();
    }

    private void UpdateEndTime()
    {
      this.EndTime = this.EndTime.Date.Add(new TimeSpan(this.EndHours, this.EndMinutes, 0));
      this.UpdateStatusWithLatestData();
    }

    public Milestone(DateTime day)
    {
      this.isInitializing = true;
      this._startTime = day.Date;
      this._endTime = day.Date.AddDays(1).AddMinutes(-1);
      this._startHours = this.StartTime.Hour;
      this._startMinutes = this.StartTime.Minute;
      this._endHours = this.EndTime.Hour;
      this._endMinutes = this.EndTime.Minute;
      this.isInitializing = false;
    }

    public Milestone(DateOnly day) : this(day.ToDateTime(TimeOnly.MinValue))
    {
    }

    public Milestone(DateTime start, DateTime end)
    {
      this.isInitializing = true;
      this._startTime = start;
      this._endTime = end;
      this._startHours = this.StartTime.Hour;
      this._startMinutes = this.StartTime.Minute;
      this._endHours = this.EndTime.Hour;
      this._endMinutes = this.EndTime.Minute;
      this.isInitializing = false;
    }

    private Milestone(MilestoneData data)
    {
      this.isInitializing = true;
      this.Data = data;
      this.ReadData();
      this.isInitializing = false;
    }

    public async Task RemoveDataAndSaveAsync(MainContext db)
    {
      if (this.Data == null || this.Data.Id == default)
      {
        return;
      }

      db.Milestones!.Remove(this.Data);
      await db.SaveChangesAsync();

      this.Data.Id = default;
    }

    private void ReadData()
    {
      if (this.Data == null)
      {
        return;
      }

      this.StartTime = this.Data.StartTime;
      this.EndTime = this.Data.EndTime;
      this.StartHours = this.Data.StartTime.Hour;
      this.StartMinutes = this.Data.StartTime.Minute;
      this.EndHours = this.Data.EndTime.Hour;
      this.EndMinutes = this.Data.EndTime.Minute;

      this.Target = this.Data.Target;
      this.Type = this.Data.Type;
      this.Value = this.Data.Value;
    }

    private void SetData()
    {
      if (this.Data == null)
      {
        return;
      }

      this.Data.StartTime = this.StartTime;
      this.Data.EndTime = this.EndTime;
      this.Data.Target = this.Target;
      this.Data.Type = this.Type;
      this.Data.Value = this.Value;
    }

    private void UpdateStatusWithLatestData()
    {
      if (this.lastActivities == null)
      {
        return;
      }

      this.UpdateStatus(this.lastActivities);
    }

    public void UpdateStatus(IEnumerable<WindowActivity> activities)
    {
      this.lastActivities = activities;

      var statistics = new ActivityStatistics(activities.Where(a => a.StartTime >= this.StartTime && a.StartTime <= this.EndTime));
      this.UpdateStatus(statistics);
    }

    private void UpdateStatus(ActivityStatistics statistics)
    {
      bool IsAchieved()
      {
        if (this.Type == MilestoneType.More)
        {
          return this.CurrentValue >= this.Value;
        }
        else if (this.Type == MilestoneType.Less)
        {
          return this.CurrentValue <= this.Value;
        }

        return false;
      }

      this.UpdateCurrentValue(statistics);

      var isAchieved = IsAchieved();
      this.Status = isAchieved ? MilestoneStatus.Achieved : MilestoneStatus.Processing;
    }

    private void UpdateCurrentValue(ActivityStatistics statistics)
    {
      var currentValue = this.Target switch
      {
        MilestoneTarget.AllEffective => statistics.Effective + statistics.MostEffective,
        MilestoneTarget.AllIneffective => statistics.Ineffective + statistics.MostIneffective,
        MilestoneTarget.MostEffective => statistics.MostEffective,
        MilestoneTarget.Effective => statistics.Effective,
        MilestoneTarget.Normal => statistics.Normal,
        MilestoneTarget.Ineffective => statistics.Ineffective,
        MilestoneTarget.MostIneffective => statistics.MostIneffective,
        MilestoneTarget.Score => statistics.Score,
        _ => 0.0,
      };

      this.CurrentValue = currentValue;
    }

    public static Milestone FromData(MilestoneData data)
    {
      return new(data);
    }
  }

  public enum MilestoneStatus
  {
    Processing,
    Achieved,
  }
}
