// **********************************************************************
// 
//   MapSpanExtension.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using Android.Gms.Maps.Model;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;

namespace Xamarin.Forms.Maps.Overlays.Platforms.Droid.Extensions
{
    internal static class MapSpanExtension
    {
        public static LatLngBounds ToLatLng(this MapSpan self)
        {
            return new SKMapSpan(self).ToLatLng();
        }

        public static LatLngBounds ToLatLng(this SKMapSpan self)
        {
            return new LatLngBounds(self.BottomLeft.ToLatLng(), self.TopRight.ToLatLng());
        }

        public static SKMapSpan ToMapSpan(this LatLngBounds self)
        {
            double latSpan = self.Northeast.Latitude - self.Southwest.Latitude;
            double longSpan = self.Northeast.Longitude - self.Southwest.Longitude;

            return new SKMapSpan(self.Center.ToPosition(), latSpan * 0.5, longSpan * 0.5);
        }

        public static Rectangle ToRectangle(this LatLngBounds self)
        {
            return self.ToMapSpan().ToMercator();
        }

        public static LatLngBounds ToLatLng(this Rectangle self)
        {
            SKMapSpan mapSpan = self.ToGps();

            return new LatLngBounds(mapSpan.BottomLeft.ToLatLng(), mapSpan.TopRight.ToLatLng());
        }
    }
}
