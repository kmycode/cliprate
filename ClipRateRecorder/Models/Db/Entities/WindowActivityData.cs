using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Db.Entities
{
  [Index(nameof(Title))]
  [Index(nameof(ExePath))]
  [Index(nameof(StartTime))]
  public class WindowActivityData
  {
    public uint Id { get; set; }

    public string? Title { get; set; }

    public string? ExePath { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public float DurationSeconds { get; set; }
  }
}
