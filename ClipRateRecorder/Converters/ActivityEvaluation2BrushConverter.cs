using ClipRateRecorder.Models.Db.Entities;
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
  internal class ActivityEvaluation2BrushConverter : IValueConverter
  {
    private static readonly Brush MostIneffectiveBrush = new SolidColorBrush(Color.FromRgb(180, 24, 24));
    private static readonly Brush IneffectiveBrush = new SolidColorBrush(Color.FromRgb(255, 0, 80));
    private static readonly Brush NormalBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
    private static readonly Brush EffectiveBrush = new SolidColorBrush(Color.FromRgb(0, 120, 255));
    private static readonly Brush MostEffectiveBrush = new SolidColorBrush(Color.FromRgb(24, 24, 180));

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is ActivityEvaluation ev)
      {
        return ev switch
        {
          ActivityEvaluation.MostIneffective => MostIneffectiveBrush,
          ActivityEvaluation.Ineffective => IneffectiveBrush,
          ActivityEvaluation.Normal => NormalBrush,
          ActivityEvaluation.Effective => EffectiveBrush,
          ActivityEvaluation.MostEffective => MostEffectiveBrush,
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
