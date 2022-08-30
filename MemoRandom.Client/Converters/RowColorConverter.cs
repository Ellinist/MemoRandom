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
                if (currentHuman.FullYearsLived > 40) return Colors.Brown.ToString();
                else return Colors.Green.ToString();
            }

            return Colors.Goldenrod.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
