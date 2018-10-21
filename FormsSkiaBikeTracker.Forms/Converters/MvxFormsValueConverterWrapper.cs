using MvvmCross.Converters;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.Converters
{
    class MvxFormsValueConverterWrapper : MvxValueConverter, IValueConverter
    {
        private IMvxValueConverter _Source { get; set; }

        public MvxFormsValueConverterWrapper(IMvxValueConverter source)
        {
            _Source = source;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _Source.Convert(value, targetType, parameter, culture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _Source.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
