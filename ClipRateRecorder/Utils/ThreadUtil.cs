using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ClipRateRecorder.Utils
{
  internal static class ThreadUtil
  {
    public static Dispatcher? Dispatcher { get; set; }

    public static void RunGuiThread(Action action)
    {
      Dispatcher?.Invoke(action);
    }
  }
}
