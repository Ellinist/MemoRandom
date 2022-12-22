using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Models;

namespace MemoRandom.Client.Converters
{
    public class DeathReasonNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            var id = (value as Human).DeathReasonId;
            var reason = CommonDataController.PlainReasonsList.FirstOrDefault(x => x.ReasonId == id);
            var result = reason.ReasonName;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
