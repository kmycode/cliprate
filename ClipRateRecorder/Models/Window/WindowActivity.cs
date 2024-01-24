using ClipRateRecorder.Models.Db.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Window
{
  class WindowActivity
  {
    public int ProcessId { get; }

    public string Title { get; } = string.Empty;

    public string ExePath { get; } = string.Empty;

    public DateTime StartTime { get; }

    public DateTime EndTime { get; private set; }

    public TimeSpan Duration => this.EndTime > this.StartTime ? this.EndTime - this.StartTime : TimeSpan.Zero;

    public bool IsValid => this.Duration != TimeSpan.Zero;

    private WindowActivity()
    {
    }

    private WindowActivity(int processId, string title, string exePath)
    {
      this.ProcessId = processId;
      this.Title = title;
      this.ExePath = exePath;
      this.StartTime = DateTime.Now;
    }

    public WindowActivityData GenerateData()
    {
      if (!this.IsValid)
      {
        throw new InvalidOperationException();
      }

      return new()
      {
        Title = this.Title,
        ExePath = this.ExePath,
        StartTime = this.StartTime,
        EndTime = this.EndTime,
        DurationSeconds = (float)this.Duration.TotalSeconds,
      };
    }

    public void Stop()
    {
      this.EndTime = DateTime.Now;
    }

    public bool IsSameProcess(WindowActivity other) => this.ProcessId == other.ProcessId;

    public static WindowActivity CreateRecord(int processId, string title, string exePath)
      => new(processId, title, exePath);
  }
}
