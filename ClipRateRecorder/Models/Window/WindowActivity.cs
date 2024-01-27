using ClipRateRecorder.Models.Analysis.Rules;
using ClipRateRecorder.Models.Db.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

    public string ExeFileName { get; } = string.Empty;

    public DateTime StartTime { get; }

    public DateTime EndTime { get; private set; }

    public TimeSpan Duration { get; private set; }

    public TimeSpan ProperDuration => this.EndTime > this.StartTime ? this.Duration : DateTime.Now - this.StartTime;

    public bool IsValid => this.Duration != TimeSpan.Zero;

    public ActivityEvaluation Evaluation
    {
      get => this._evaluation;
      private set
      {
        if (this._evaluation == value) return;
        this._evaluation = value;
        this.OnPropertyChanged();
      }
    }
    private ActivityEvaluation _evaluation;

    public ActivityEvaluationRule? Rule
    {
      get => this._rule;
      set
      {
        if (value == null)
        {
          this.Evaluation = ActivityEvaluation.Normal;
        }
        else
        {
          this.Evaluation = value.Evaluate(this);
        }

        if (this._rule == value) return;

        this._rule = value;
        this.OnPropertyChanged();
      }
    }
    private ActivityEvaluationRule? _rule;

    private WindowActivity()
    {
    }

    private WindowActivity(string title, string exePath)
    {
      this.Title = title;
      this.ExePath = exePath.ToLower();
      this.ExeFileName = Path.GetFileName(this.ExePath);
      this.StartTime = DateTime.Now;
    }

    private WindowActivity(WindowActivityData data)
    {
      this.Title = data.Title ?? string.Empty;
      this.ExePath = data.ExePath?.ToLower() ?? string.Empty;
      this.ExeFileName = Path.GetFileName(this.ExePath);
      this.StartTime = data.StartTime;
      this.EndTime = data.EndTime;
      if (this.EndTime > this.StartTime)
      {
        this.Duration = this.EndTime - this.StartTime;
      }
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
      this.Duration = this.EndTime > this.StartTime ? this.EndTime - this.StartTime : TimeSpan.Zero;
      this.OnPropertyChanged(nameof(EndTime));
      this.OnPropertyChanged(nameof(Duration));
      this.OnPropertyChanged(nameof(IsValid));
    }

    public bool IsSameWindow(WindowActivity other) => this.ExePath == other.ExePath && this.Title == other.Title;

    public static WindowActivity CreateRecord(string title, string exePath)
      => new(title, exePath);
  }
}
