using System.Collections.Generic;
using Xamarin.Forms.SKMaps.Sample.Converters;
using Xamarin.Forms.SKMaps.Sample.Forms.Controls;
using Xamarin.Forms.SKMaps.Sample.Forms.Converters;
using Xamarin.Forms.SKMaps.Sample.Services;
using MvvmCross;
using MvvmCross.Forms.Core;
using MvvmCross.Localization;
using MvvmCross.Plugin.JsonLocalization;

namespace Xamarin.Forms.SKMaps.Sample.Forms
{
	public partial class FormsApp : MvxFormsApplication
    {
		public FormsApp()
		{
			InitializeComponent();
            SetupResources();
        }

        protected override void OnStart()
        {
            base.OnStart();

            TextProviderBuilder builder = Mvx.IoCProvider.Resolve<IMvxTextProviderBuilder>() as TextProviderBuilder;

            builder.RegisterExtraTextKeys(new Dictionary<string, string>
                                          {
                                              { nameof(QuickStatsBanner), nameof(QuickStatsBanner) }
                                          });
        }

        private void SetupResources()
        {
            if (Resources == null)
            {
                Resources = new ResourceDictionary();
            }

            Resources.Add("Language", new MvxFormsValueConverterWrapper(new MvxLanguageConverter()));
            Resources.Add("IsNull", new MvxFormsValueConverterWrapper(new IsNullConverter()));
            Resources.Add("IsNotNull", new MvxFormsValueConverterWrapper(new IsNotNullConverter()));
        }
    }
}