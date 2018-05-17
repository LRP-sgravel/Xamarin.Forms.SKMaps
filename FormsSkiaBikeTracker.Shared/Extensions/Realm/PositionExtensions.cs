// **********************************************************************
// 
//   PositionExtensions.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using FormsSkiaBikeTracker.Models;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Extensions.Realm
{
    public static class PositionExtensions
    {
        public static RoutePoint ToRoutePoint(this Position self)
        {
            return new RoutePoint
                   {
                       Latitude = self.Latitude,
                       Longitude = self.Longitude
                   };
        }
    }
}
