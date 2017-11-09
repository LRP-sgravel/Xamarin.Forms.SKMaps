// **********************************************************************
// 
//   MapSpanExtensions.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Helpers
{
    public static class MapSpanExtension
    {
        public static Position TopLeft(this MapSpan self)
        {
            return new Position(self.Center.Latitude + self.LatitudeDegrees, self.Center.Longitude - self.LongitudeDegrees);
        }

        public static Position BottomRight(this MapSpan self)
        {
            return new Position(self.Center.Latitude - self.LatitudeDegrees, self.Center.Longitude + self.LongitudeDegrees);
        }
    }
}
