using System.Collections.Generic;
using FormsSkiaBikeTracker.Forms.Controls;
using LRPFramework.Mvx.Services.Localization;
using MvvmCross;
using MvvmCross.Forms.Core;
using MvvmCross.Plugin.JsonLocalization;

namespace FormsSkiaBikeTracker.Forms
{
	public partial class FormsApp : MvxFormsApplication
    {
		public FormsApp()
		{
			InitializeComponent();
		}

        protected override void OnStart()
        {
            base.OnStart();

            LRPTextProviderBuilder builder = Mvx.Resolve<IMvxTextProviderBuilder>() as LRPTextProviderBuilder;

            builder.RegisterExtraTextKeys(new Dictionary<string, string>
                                          {
                                              { nameof(QuickStatsBanner), nameof(QuickStatsBanner) }
                                          });
        }
    }
}