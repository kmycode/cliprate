using ClipRateRecorder.Models.Db.Entities;
using ClipRateRecorder.Models.Goals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ClipRateRecorder.Converters
{
  public class ForegroundBrushConverter : IValueConverter
  {
    private static readonly Brush InachievedBrush = new SolidColorBrush(Color.FromRgb(255, 45, 132));
    private static readonly Brush AchievedBrush = new SolidColorBrush(Color.FromRgb(48, 255, 115));

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is MilestoneStatus status)
      {
        return status switch
        {
          MilestoneStatus.Processing => InachievedBrush,
          MilestoneStatus.Achieved => AchievedBrush,
          _ => null,
        };
      }

      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
