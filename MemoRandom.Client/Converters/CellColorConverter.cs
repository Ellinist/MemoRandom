using MemoRandom.Models.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MemoRandom.Client.Converters
{
    public class CellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as Category;
            if (val == null) return "White";
            
            return val.CategoryColor.ToString();
            

            //return Colors.Red.ToString();
            //if (value is DataGridCell)
            //{
            //    string text = (value as DataGridCell).Content.ToString();

            //    int input = string.IsNullOrEmpty(text) ? 0 : System.Convert.ToInt32(text);


            //    if (input >= 0)
            //    {
            //        return Brushes.Green;
            //    }
            //    if (input < 0)
            //    {
            //        return Brushes.Red;
            //    }
            //}
            //return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
