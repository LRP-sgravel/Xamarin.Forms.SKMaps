// **********************************************************************
// 
//   PostionExtensions.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System;
using Xamarin.Forms.Maps.Overlays.Skia;

namespace Xamarin.Forms.Maps.Overlays.Extensions
{
    public static class PositionExtensions
    {
        // Law of cosines, good enoguh for short distances
        public static Distance FastDistanceTo(this Position self, Position other)
        {
            double latitudeMiddle = (self.Latitude + other.Latitude) * 0.5;
            double latitudeDiff = Math.Abs(self.Latitude - other.Latitude).ToRadians();
            double longitudeDiff = Math.Abs(self.Longitude - other.Longitude).ToRadians();
            double temp = longitudeDiff * Math.Cos(latitudeMiddle.ToRadians());
            
            return Distance.FromMeters(Math.Sqrt(temp * temp + latitudeDiff * latitudeDiff) * 6371000);
        }
    }
}
