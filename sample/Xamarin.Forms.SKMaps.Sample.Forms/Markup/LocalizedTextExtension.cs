
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.SKMaps.Sample.Forms.Markup
{
    [ContentProperty("TextId")]
    public class LocalizedTextExtension : IMarkupExtension<BindingBase>
    {
        public string TextId { get; set; }
        public object Source { get; set; }
        public string Path { get; set; }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            Binding newBinding = new Binding
            {
                Path = "LanguageBinder",
                Converter = Application.Current.Resources["Language"] as IValueConverter,
                ConverterParameter = TextId,
                Mode = BindingMode.OneWay,
            };

            if (Source != null)
            {
                newBinding.Source = Source;
            }

            if (Path != null)
            {
                newBinding.Path = string.Format("{0}.{1}", Path, newBinding.Path);
            }

            return newBinding;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}
