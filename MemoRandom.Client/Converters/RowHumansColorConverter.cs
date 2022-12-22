using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace MemoRandom.Client.Converters
{
    /// <summary>
    /// Конвертер для отображения строк соответствующим цветом
    /// </summary>
    public class RowHumansColorConverter : IValueConverter
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
            if (value is not Human currentHuman) return Colors.White.ToString();
            var o = CommonDataController.AgeCategories.FirstOrDefault(x => x.StartAge <= currentHuman.FullYearsLived &&
                                                                           x.StopAge + 1 > currentHuman.FullYearsLived);

            return o == null ? Colors.White.ToString() : o.CategoryColor;
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
