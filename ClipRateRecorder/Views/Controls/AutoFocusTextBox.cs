using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClipRateRecorder.Views.Controls
{
  public class AutoFocusTextBox : TextBox
  {
    protected override void OnGotFocus(RoutedEventArgs e)
    {
      base.OnGotFocus(e);

      this.SelectAll();
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnMouseLeftButtonDown(e);

      if (this.IsFocused)
      {
        return;
      }

      this.SelectAll();
    }
  }
}
