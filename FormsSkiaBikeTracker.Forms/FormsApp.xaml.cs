using System.Collections.Generic;
using FormsSkiaBikeTracker.Converters;
using FormsSkiaBikeTracker.Forms.Controls;
using FormsSkiaBikeTracker.Forms.Converters;
using FormsSkiaBikeTracker.Services;
using MvvmCross;
using MvvmCross.Forms.Core;
using MvvmCross.Localization;
using MvvmCross.Plugin.JsonLocalization;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms
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

            TextProviderBuilder builder = Mvx.Resolve<IMvxTextProviderBuilder>() as TextProviderBuilder;

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