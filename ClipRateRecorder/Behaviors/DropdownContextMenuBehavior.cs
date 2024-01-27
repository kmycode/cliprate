using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace ClipRateRecorder.Behaviors
{
  internal class DropdownContextMenuBehavior : Behavior<ToggleButton>
  {
    protected override void OnAttached()
    {
      base.OnAttached();

      var button = this.AssociatedObject;
      var menu = button.ContextMenu;

      if (menu == null)
      {
        return;
      }

      menu.PlacementTarget = button;

      button.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsOpen")
      {
        Source = menu,
        Mode = BindingMode.TwoWay,
      });
    }

    protected override void OnDetaching()
    {
      var button = this.AssociatedObject;
      var menu = button.ContextMenu;

      if (menu != null)
      {
        menu.PlacementTarget = null;
        button.SetBinding(ToggleButton.IsCheckedProperty, new Binding());
      }

      base.OnDetaching();
    }
  }
}
