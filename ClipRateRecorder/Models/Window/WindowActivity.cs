using ClipRateRecorder.Models.Db.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Window
{
  class WindowActivity : INotifyPropertyChanged
  {
    private bool isFromData;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title { get; } = string.Empty;

    public string ExePath { get; } = string.Empty;

    public DateTime StartTime { get; }

    public DateTime EndTime { get; private set; }

    public TimeSpan Duration => this.EndTime > this.StartTime ? this.EndTime - this.StartTime : TimeSpan.Zero;

    public bool IsValid => this.Duration != TimeSpan.Zero;

    public ActivityEvaluation Evaluation
    {
      get => this._evaluation;
      set
      {
        if (this._evaluation == value) return;
        this._evaluation = value;
        this.OnPropertyChanged();
      }
    }
    private ActivityEvaluation _evaluation;

    private WindowActivity()
    {
    }

    private WindowActivity(string title, string exePath)
    {
      this.Title = title;
      this.ExePath = exePath.ToLower();
      this.StartTime = DateTime.Now;
    }

    private WindowActivity(WindowActivityData data)
    {
      this.Title = data.Title ?? string.Empty;
      this.ExePath = data.ExePath?.ToLower() ?? string.Empty;
      this.StartTime = data.StartTime;
      this.EndTime = data.EndTime;
      this.isFromData = true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public WindowActivityData GenerateData()
    {
      if (!this.IsValid || this.isFromData)
      {
        throw new InvalidOperationException();
      }

      return new()
      {
        Title = this.Title,
        ExePath = this.ExePath.ToLower(),
        StartTime = this.StartTime,
        EndTime = this.EndTime,
        DurationSeconds = (float)this.Duration.TotalSeconds,
      };
    }

    public static WindowActivity FromData(WindowActivityData data)
    {
      return new(data);
    }

    public void Stop()
    {
      this.EndTime = DateTime.Now;
      this.OnPropertyChanged(nameof(EndTime));
      this.OnPropertyChanged(nameof(Duration));
      this.OnPropertyChanged(nameof(IsValid));
    }

    public bool IsSameWindow(WindowActivity other) => this.ExePath == other.ExePath && this.Title == other.Title;

    public static WindowActivity CreateRecord(string title, string exePath)
      => new(title, exePath);
  }
}
