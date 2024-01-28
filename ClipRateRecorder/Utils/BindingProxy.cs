using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClipRateRecorder.Utils
{
  // https://itecnote.com/tecnote/wpf-menuitem-command-binding-to-elementname-results-to-system-windows-data-error-4-cannot-find-source-for-binding-with-reference/
  public class BindingProxy : Freezable
  {
    protected override Freezable CreateInstanceCore()
    {
      return new BindingProxy();
    }

    public object Data
    {
      get { return GetValue(DataProperty); }
      set { SetValue(DataProperty, value); }
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
  }
}
