using ClipRateRecorder.Utils;
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
  public class ViewPageConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is ViewPage page)
      {
        var isBoolean = false;

        var p = parameter.ToString() ?? string.Empty;
        if (p.StartsWith('?'))
        {
          p = p.Substring(1);
          isBoolean = true;
        }

        var isSame = page switch
        {
          ViewPage.Daily => p == "Daily",
          ViewPage.Spot => p == "Spot",
          ViewPage.Milestone => p == "Milestone",
          _ => false,
        };

        if (isBoolean || targetType == typeof(bool) || targetType == typeof(bool?))
        {
          return isSame;
        }
        if (targetType == typeof(Visibility) || targetType == typeof(Visibility?))
        {
          return isSame ? Visibility.Visible : Visibility.Collapsed;
        }

        throw new NotSupportedException();
      }
      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
