using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Watching;
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
  class RawActivityGroup : INotifyPropertyChanged
  {
    public ObservableCollection<WindowActivity> Activities { get; } = [];

    public double TotalDuration => this.Activities.Any() ? this.Activities.Sum(a => a.Duration.TotalSeconds) : 0;

    public ActivityStatistics Statistics { get; private set; } = ActivityStatistics.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public RawActivityGroup() { }

    private RawActivityGroup(IEnumerable<WindowActivity> activities)
    {
      this.Activities = new(activities);
    }

    public void UpdateStatistics()
    {
      this.Statistics = new(this.Activities);

      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Statistics)));
    }

    public void OnWindowActivityTicked(object sender, WatchingTickEventArgs e)
    {
      var activity = e.CurrentActivity;

      if (!this.FireIfActivityUpdated(activity))
      {
        this.Activities.Add(activity);

        this.FireIfActivityUpdated(activity);
      }
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

    public static RawActivityGroup FromActivities(IEnumerable<WindowActivity> activities)
    {
      return new RawActivityGroup(activities);
    }
  }
}
