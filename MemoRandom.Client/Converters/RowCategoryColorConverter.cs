using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace MemoRandom.Client.Converters
{
    public class RowCategoryColorConverter : IValueConverter
    {
        /// <summary>
        /// Прямая конвертация
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentCategory = value as Category;

            if (currentCategory != null)
            {
                var o = CommonDataController.AgeCategories.FirstOrDefault(x => x.CategoryColor == currentCategory.CategoryColor);

                if (o == null) return Colors.White.ToString(); // Если цвет не задан, то белый
                return o.CategoryColor;
            }

            return Colors.White.ToString();
        }

        /// <summary>
        /// Обратная конвертация
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
