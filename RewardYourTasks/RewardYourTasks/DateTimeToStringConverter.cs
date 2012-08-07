using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Globalization;

namespace RewardYourTasks
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dt = (DateTime)value;
            string modifiedDateTimeFormat;

            if(dt.AddSeconds(-5).TimeOfDay.TotalMinutes != 0)
                modifiedDateTimeFormat = CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/yyyy", "");
            else
                modifiedDateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/yyyy", "");

            return dt.ToString(modifiedDateTimeFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
