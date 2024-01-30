using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ClipRateRecorder.Converters
{
  internal class MultibleBooleanOrConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType == typeof(bool) || targetType == typeof(bool?))
      {
        return values.Cast<bool>().Any(b => b);
      }
      if (targetType == typeof(Visibility) || targetType == typeof(Visibility?))
      {
        return values.Cast<bool>().Any(b => b) ? Visibility.Visible : Visibility.Collapsed;
      }

      throw new NotSupportedException();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
