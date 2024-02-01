using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ClipRateRecorder.Converters
{
  public class EndTimeSpanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is TimeSpan ts)
      {
        if (ts.TotalDays >= 1)
        {
          return $"{ts.Hours + 24}:{ts:mm}";
        }
        return ts.ToString("hh\\:mm");
      }

      throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
