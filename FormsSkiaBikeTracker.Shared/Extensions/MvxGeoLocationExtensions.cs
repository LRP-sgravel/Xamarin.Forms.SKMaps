// **********************************************************************
// 
//   MvxGeoLocationExtensions.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using MvvmCross.Plugin.Location;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Extensions
{
    public static class MvxGeoLocationExtensions
    {
        public static Position ToPosition(this MvxGeoLocation self)
        {
            return self.Coordinates.ToPosition();
        }

        public static Position ToPosition(this MvxCoordinates self)
        {
            return new Position(self.Latitude, self.Longitude);
        }
    }
}
