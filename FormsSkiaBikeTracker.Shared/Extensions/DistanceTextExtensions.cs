// **********************************************************************
// 
//   DistanceTextExtensions.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using MvvmCross;
using MvvmCross.Localization;
using Xamarin.Forms.Maps.Overlays.Models;

namespace FormsSkiaBikeTracker.Extensions
{
    public static class DistanceTextExtensions
    {
        public static string ToShortText(this DistanceUnit self)
        {
            IMvxTextProvider textProvider = Mvx.Resolve<IMvxTextProvider>();
            string textId = string.Empty;

            switch (self)
            {
                case DistanceUnit.Kilometer:
                    textId = "KilometersShortDistance";
                    break;
                case DistanceUnit.Meters:
                    textId = "MetersShortDistance";
                    break;
                case DistanceUnit.Miles:
                    textId = "MilesShortDistance";
                    break;
            }

            return textProvider.GetText(Constants.GeneralNamespace, Constants.TextTypeKey, textId);
        }

        public static string ToShortSpeedText(this DistanceUnit self)
        {
            IMvxTextProvider textProvider = Mvx.Resolve<IMvxTextProvider>();
            string textId = string.Empty;

            switch (self)
            {
                case DistanceUnit.Kilometer:
                    textId = "KilometersShortSpeed";
                    break;
                case DistanceUnit.Meters:
                    textId = "MetersShortSpeed";
                    break;
                case DistanceUnit.Miles:
                    textId = "MilesShortSpeed";
                    break;
            }

            return textProvider.GetText(Constants.GeneralNamespace, Constants.TextTypeKey, textId);
        }
    }
}
