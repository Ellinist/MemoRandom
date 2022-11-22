using MemoRandom.Client.Common.Implementations;
using MemoRandom.Models.Models;
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
    public class RowColorConverter : IValueConverter
    {
        /// <summary>
        /// Прмяая конвертация
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentHuman = value as Human;

            if(currentHuman != null)
            {
                var o = CommonDataController.AgeCategories.FirstOrDefault(x => x.StartAge <= currentHuman.FullYearsLived &&
                                                                          x.StopAge + 1 > currentHuman.FullYearsLived);

                if (o == null) return Colors.White.ToString(); // Если цвет не задан, то белый
                return o.CategoryColor.ToString();
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
