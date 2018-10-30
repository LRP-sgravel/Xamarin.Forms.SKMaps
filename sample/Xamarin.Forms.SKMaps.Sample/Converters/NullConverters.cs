using System;
using System.Globalization;
using MvvmCross.Converters;

namespace Xamarin.Forms.SKMaps.Sample.Converters
{
    public class IsNullConverter : IMvxValueConverter
    {
        protected bool IsNull(object value) => value == null;

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IsNull(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNotNullConverter : IsNullConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !IsNull(value);
        }
    }
}
