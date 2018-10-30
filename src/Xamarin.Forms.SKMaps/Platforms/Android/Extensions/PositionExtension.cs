// **********************************************************************
// 
//   PositionExtension.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using Android.Gms.Maps.Model;
using Xamarin.Forms.Maps;
using Xamarin.Forms.SKMaps.Models;

namespace Xamarin.Forms.SKMaps.Platforms.Droid.Extensions
{
    internal static class PositionExtension
    {
        public static LatLng ToLatLng(this SKMapPosition self)
        {
            return new LatLng(self.Latitude, self.Longitude);
        }
        
        public static LatLng ToLatLng(this Position self)
        {
            return new LatLng(self.Latitude, self.Longitude);
        }

        public static Position ToPosition(this LatLng self)
        {
            return new Position(self.Latitude, self.Longitude);
        }
    }
}
