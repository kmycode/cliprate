﻿using ClipRateRecorder.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClipRateRecorder
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      this.Closed += (sender, e) =>
      {
        if (this.DataContext is ICloseEventReceiver receiver)
        {
          receiver.OnWindowClose();
        }
      };

      /*
      double[] dataX = { 1, 2, 3, 4, 5 };
      double[] dataY = { 1, 4, 9, 16, 25 };
      TestPlot.Plot.Add.Scatter(dataX, dataY);
      TestPlot.Refresh();
      */

      var plt = TestPlot.Plot;

      // create sample data
      double[] values = { 26, 20, 23, 7, 16 };

      // add a bar graph to the plot
      plt.Add.Bar(1, 26);
      plt.Add.Bar(2, 20);
      plt.Add.Bar(3, 23);
      plt.Add.Bar(4, 7);
      plt.Add.Bar(5, 16);

      plt.Axes.Bottom.FrameLineStyle.Color = ScottPlot.Colors.White;
      plt.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Colors.White;
      plt.Axes.Left.FrameLineStyle.Color = ScottPlot.Colors.White;
      plt.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Colors.White;

      plt.DataBackground = ScottPlot.Colors.Black;
      plt.FigureBackground = ScottPlot.Colors.Black;

      TestPlot.Refresh();
    }
  }
}
