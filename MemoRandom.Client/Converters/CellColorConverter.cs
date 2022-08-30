using MemoRandom.Models.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MemoRandom.Client.Converters
{
    public class CellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Category val = value as Category;
            if (val == null) return "White";
            
            return val.CategoryColor.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
