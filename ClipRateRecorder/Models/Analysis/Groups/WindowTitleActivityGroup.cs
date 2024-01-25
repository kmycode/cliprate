using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Analysis.Groups
{
  class WindowTitleActivityGroupCollection : ObservableCollection<WindowTitleActivityGroup>
  {
    public double TotalDuration => this.Any() ? this.Sum(a => a.TotalDuration) : 0;

    private IEnumerable<WindowActivity> Activities => this.SelectMany(a => a.Activities);

    public ActivityStatistics Statistics { get; private set; } = ActivityStatistics.Empty;

    public void UpdateStatistics()
    {
      foreach (var item in this)
      {
        item.UpdateStatistics();
      }

      this.Statistics = new ActivityStatistics(this.Activities);

      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Statistics)));
    }

    public bool FireIfActivityUpdated(WindowActivity activity)
    {
      if (this.Count(a => a.FireIfActivityUpdated(activity)) == 0)
      {
        return false;
      }
      this.UpdateStatistics();

      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(TotalDuration)));
      return true;
    }
  }

  class WindowTitleActivityGroup : INotifyPropertyChanged
  {
    public List<WindowActivity> Activities { get; } = [];

    public string Title { get; }

    public double TotalDuration => this.Activities.Any() ? this.Activities.Sum(a => a.Duration.TotalSeconds) : 0;

    public ActivityStatistics Statistics { get; private set; } = ActivityStatistics.Empty;

    public WindowTitleActivityGroup(string title)
    {
      this.Title = title;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void UpdateStatistics()
    {
      this.Statistics = new(this.Activities);

      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Statistics)));
    }

    public void AddActivity(WindowActivity activity)
    {
      this.Activities.Add(activity);
    }

    public bool FireIfActivityUpdated(WindowActivity activity)
    {
      if (!this.Activities.Contains(activity))
      {
        return false;
      }
      this.UpdateStatistics();

      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalDuration)));
      return true;
    }
  }
}
