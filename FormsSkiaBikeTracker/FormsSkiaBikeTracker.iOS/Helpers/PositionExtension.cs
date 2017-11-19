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

using CoreLocation;
using FormsSkiaBikeTracker.Forms.UI.Pages;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.iOS.Helpers
{
    public static class PositionExtension
    {
        public static CLLocationCoordinate2D ToLocationCoordinate(this SKMapPosition self)
        {
            Position position = new Position(self.Latitude,
                                             (self.Longitude + Forms.UI.Helpers.MapSpanExtension.MaxLongitude) %
                                             Forms.UI.Helpers.MapSpanExtension.WorldLongitude +
                                             Forms.UI.Helpers.MapSpanExtension.MinLongitude);

            return position.ToLocationCoordinate();
        }
        
        public static CLLocationCoordinate2D ToLocationCoordinate(this Position self)
        {
            return new CLLocationCoordinate2D(self.Latitude, self.Longitude);
        }

        public static Position ToPosition(this CLLocationCoordinate2D self)
        {
            return new Position(self.Latitude, self.Longitude);
        }
    }
}
