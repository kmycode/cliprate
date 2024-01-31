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
  public class LabelConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is MilestoneType type)
      {
        return type switch
        {
          MilestoneType.Less => "以下",
          MilestoneType.More => "以上",
          _ => "ERROR",
        };
      }
      if (value is MilestoneTarget target)
      {
        return target switch
        {
          MilestoneTarget.AllEffective => "効率的な全ての時間",
          MilestoneTarget.AllIneffective => "非効率的な全ての時間",
          MilestoneTarget.MostEffective => "最も効率的な時間",
          MilestoneTarget.Effective => "効率的な時間",
          MilestoneTarget.Normal => "普通の時間",
          MilestoneTarget.Ineffective => "非効率的な時間",
          MilestoneTarget.MostIneffective => "最も非効率的な時間",
          MilestoneTarget.Score => "スコア",
          _ => "ERROR",
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
