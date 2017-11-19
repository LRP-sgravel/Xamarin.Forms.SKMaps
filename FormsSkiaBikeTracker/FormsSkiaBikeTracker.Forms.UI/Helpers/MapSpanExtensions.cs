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

using FormsSkiaBikeTracker.Forms.UI.Pages;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Helpers
{
    public static class MapSpanExtension
    {
        public const int WorldLongitude = 360;
        public const int MaxLongitude = WorldLongitude / 2;
        public const int MinLongitude = -MaxLongitude;

        public static SKMapPosition TopLeft(this MapSpan self)
        {
            return new SKMapPosition(self.Center.Latitude + self.LatitudeDegrees, self.Center.Longitude - self.LongitudeDegrees);
        }

        public static SKMapPosition BottomRight(this MapSpan self)
        {
            return new SKMapPosition(self.Center.Latitude - self.LatitudeDegrees, self.Center.Longitude + self.LongitudeDegrees);
        }

        public static MapSpan WrapIfRequired(this MapSpan self)
        {
            MapSpan result = self;

            if (self.Crosses180thMeridian())
            {
                // If we cross the 180th meridian, our map span is the full world width
                result = new MapSpan(new Position(result.Center.Latitude, 0), result.LatitudeDegrees, MaxLongitude);
            }

            return result;
        }

        public static bool Crosses180thMeridian(this MapSpan self)
        {
            return self.Crosses180thMeridianRight() ||
                   self.Crosses180thMeridianLeft();
        }

        public static bool Crosses180thMeridianRight(this MapSpan self)
        {
            return self.Center.Longitude + self.LongitudeDegrees > MaxLongitude;
        }

        public static bool Crosses180thMeridianLeft(this MapSpan self)
        {
            return self.Center.Longitude - self.LongitudeDegrees < MinLongitude;
        }
    }
}
