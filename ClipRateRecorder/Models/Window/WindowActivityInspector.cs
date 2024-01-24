using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ClipRateRecorder.Models.Window
{
  static class WindowActivityInspector
  {
    public static WindowActivity GetCurrentActivity()
      => WindowActivity.CreateRecord(
        GetActiveWindowProcess()?.Id ?? default,
        GetActiveWindowTitle() ?? string.Empty,
        GetActiveWindowExePath() ?? string.Empty);

    private static string? GetActiveWindowTitle()
    {
      var sb = new StringBuilder(65535);//65535に特に意味はない
      var result = GetWindowText(GetForegroundWindow(), sb, 65535);
      if (result == 0)
      {
        return null;
      }

      return sb.ToString();
    }

    private static string? GetActiveWindowExePath()
    {
      var process = GetActiveWindowProcess();
      if (process == null)
      {
        return null;
      }

      return process.MainModule?.FileName;
    }

    private static Process? GetActiveWindowProcess()
    {
      var activeWindowHandle = GetForegroundWindow();
      var result = GetWindowThreadProcessId(activeWindowHandle, out int processId);
      if (result == 0)
      {
        return null;
      }

      return Process.GetProcessById(processId);
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
  }
}
