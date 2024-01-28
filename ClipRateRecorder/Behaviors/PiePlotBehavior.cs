using Microsoft.Xaml.Behaviors;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClipRateRecorder.Behaviors
{
  internal class PiePlotBehavior : Behavior<ScottPlot.WPF.WpfPlot>
  {
    public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
        "Values", typeof(IEnumerable<double>),
        typeof(PiePlotBehavior),
        new PropertyMetadata(null, (sender, e) =>
        {
          if (sender is PiePlotBehavior b)
          {
            b.OnValuesUpdated();
          }
        }));

    public IEnumerable<double> Values
    {
      get => (IEnumerable<double>)GetValue(ValuesProperty);
      set => SetValue(ValuesProperty, value);
    }

    private void OnValuesUpdated()
    {
      this.AssociatedObject.Plot.Clear();
      this.AssociatedObject.Plot.DataBackground = ScottPlot.Colors.Black;
      this.AssociatedObject.Plot.FigureBackground = ScottPlot.Colors.Black;

      var values = this.Values;
      if (values == null)
      {
        return;
      }

      var colors = new[] { Colors.DarkBlue, Colors.DeepSkyBlue, Colors.Gray, Colors.Red, Colors.DarkRed, };

      var pie = this.AssociatedObject.Plot.Add.Pie(values.Select((v, i) => new PieSlice(v, colors.ElementAtOrDefault(i))).ToList());
      this.AssociatedObject.Plot.Axes.Left.IsVisible = false;
      this.AssociatedObject.Plot.Axes.Bottom.IsVisible = false;
      this.AssociatedObject.Plot.Axes.Top.IsVisible = false;
      this.AssociatedObject.Plot.Axes.Right.IsVisible = false;

      this.AssociatedObject.Refresh();
    }
  }
}
