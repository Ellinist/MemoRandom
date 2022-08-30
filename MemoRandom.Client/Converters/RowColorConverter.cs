using MemoRandom.Models.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MemoRandom.Client.Converters
{
    public class RowColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentHuman = value as Human;

            if(currentHuman != null)
            {
                var o = Categories.AgeCategories.FirstOrDefault(x => x.StartAge <= currentHuman.FullYearsLived &&
                                                                     x.StopAge + 1 > currentHuman.FullYearsLived);

                if (o == null) return Colors.White.ToString();
                return o.CategoryColor.ToString();
                //if (currentHuman.FullYearsLived > 40) return Colors.Brown.ToString();
                //else return Colors.Green.ToString();
            }

            return Colors.White.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
