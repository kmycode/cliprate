using ClipRateRecorder.Models.Db.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ClipRateRecorder.Converters
{
  internal class ActivityEvaluation2BooleanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is ActivityEvaluation ev && parameter is string pm)
      {
        return ev switch
        {
          ActivityEvaluation.MostIneffective => pm == nameof(ActivityEvaluation.MostIneffective),
          ActivityEvaluation.Ineffective => pm == nameof(ActivityEvaluation.Ineffective),
          ActivityEvaluation.Normal => pm == nameof(ActivityEvaluation.Normal),
          ActivityEvaluation.Effective => pm == nameof(ActivityEvaluation.Effective),
          ActivityEvaluation.MostEffective => pm == nameof(ActivityEvaluation.MostEffective),
          _ => false,
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
