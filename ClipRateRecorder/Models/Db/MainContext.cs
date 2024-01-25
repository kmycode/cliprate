using ClipRateRecorder.Models.Db.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Db
{
  public class MainContext : DbContext
  {
    private string fileName = ".\\clipraterecorder.db";

    public DbSet<WindowActivityData>? WindowActivities { get; set; }

    public DbSet<ActivityEvaluationRuleData>? ActivityEvaluationRules { get; set; }

    public MainContext() : this(".\\clipraterecorder.db")
    {
    }

    public MainContext(string fileName) : base()
    {
      this.fileName = fileName;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={this.fileName}");

    public static void Initialize()
    {
      new MainContext().Database.Migrate();
    }
  }
}
