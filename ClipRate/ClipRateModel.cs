using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ClipRate
{
  public class ClipRateModel : INotifyPropertyChanged
  {
    private int _inactiveCount;
    private int _activeCount;
    private int _timeCorrection;

    public double Rate
    {
      get => this._rate;
      set
      {
        if (this._rate != value)
        {
          this._rate = value;
          this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rate)));
        }
      }
    }
    private double _rate;

    public int Minutes
    {
      get => this._minutes;
      set
      {
        if (this._minutes != value)
        {
          this._minutes = value;
          this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Minutes)));
        }
      }
    }
    private int _minutes;

    public int Seconds
    {
      get => this._seconds;
      set
      {
        if (this._seconds != value)
        {
          this._seconds = value;
          this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Seconds)));
        }
      }
    }
    private int _seconds;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ClipRateModel()
    {
      this.BeginCheckLoop();
    }

    private static bool IsPauseWindow(string title)
    {
      return title == "ClipRate" || (title.EndsWith("pixiv - Google Chrome") && !title.Contains("ウマ娘"));
    }

    public void BeginCheckLoop()
    {
      Task.Run(async () =>
      {
        var current = DateTime.Now;
        var startTime = current;

        while (true)
        {
          try
          {
            var title = GetActiveWindowTitle();

            if (IsPauseWindow(title))
            {
              var pauseStartTime = DateTime.Now;
              while (IsPauseWindow(title))
              {
                await Task.Delay(100);
                title = GetActiveWindowTitle();
              }
              this._timeCorrection += (int)(DateTime.Now - pauseStartTime).TotalSeconds;
            }

            if (title == "CLIP STUDIO PAINT" || title == "Eagle" || title.EndsWith("- OneNote") || title.EndsWith("DesignDoll"))
            {
              this._activeCount++;
            }
            else
            {
              this._inactiveCount++;
            }

            {
              var rate = (double)this._activeCount / (this._activeCount + this._inactiveCount);
              if (this._activeCount + this._inactiveCount == 0)
              {
                rate = 0;
              }
              this.Rate = rate;
            }
            {
              var seconds = (int)((current - startTime).TotalSeconds) - this._timeCorrection;
              this.Minutes = seconds / 60;
              this.Seconds = seconds % 60;
            }

            while (DateTime.Now < current.AddSeconds(1))
            {
              await Task.Delay(50);
            }
            if (DateTime.Now - current > TimeSpan.FromSeconds(5))
            {
              current = DateTime.Now;
            }
            else
            {
              current = current.AddSeconds(1);
            }
          }
          catch (Exception ex)
          {

          }
        }
      });
    }

    private static string GetActiveWindowTitle()
    {
      var sb = new StringBuilder(65535);//65535に特に意味はない
      GetWindowText(GetForegroundWindow(), sb, 65535);
      return sb.ToString();
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  }
}
