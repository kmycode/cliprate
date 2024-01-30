using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Db.Entities
{
  public class MilestoneData
  {
    public uint Id { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public MilestoneType Type { get; set; }

    public MilestoneTarget Target { get; set; }

    public double Value { get; set; }
  }

  public enum MilestoneType : int
  {
    More = 0,
    Less = 1,
  }

  public enum MilestoneTarget : int
  {
    AllEffective = 0,
    AllIneffective = 1,
    MostEffective = 2,
    Effective = 3,
    Normal = 4,
    Ineffective = 5,
    MostIneffective = 6,
    Score = 7,
  }
}
