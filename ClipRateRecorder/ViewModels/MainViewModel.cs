using ClipRateRecorder.Models.Watching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipRateRecorder.ViewModels
{
  class MainViewModel : ViewModelBase, ICloseEventReceiver
  {
    private readonly IWatcherLoop loop;

    public MainViewModel()
    {
      this.loop = ActivityWatcher.StartWatchLoop();
    }

    public void OnWindowClose()
    {
      var task = this.loop.DisposeAsync();

      while (!(task.IsCompleted || task.IsFaulted))
      {
        Thread.Sleep(500);
      }
    }
  }
}
