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
using Xamarin.Forms.SKMaps.Models;

namespace Xamarin.Forms.SKMaps.Extensions
{
    public static class MapSpanExtensions
    {
        public const double WorldLatitude = 170.1022575596;
        public const int WorldLongitude = 360;

        public const double MaxLatitude = WorldLatitude * 0.5;
        public const double MinLatitude = -MaxLongitude;
        public const int MaxLongitude = WorldLongitude >> 1;
        public const int MinLongitude = -MaxLongitude;

        public static readonly MapSpan WorldSpan = new MapSpan(new Position(0, 0), MaxLatitude, MaxLongitude);

        public static Position TopLeft(this MapSpan self)
        {
            return new SKMapSpan(self).TopLeft.ToPosition();
        }

        public static Position TopRight(this MapSpan self)
        {
            return new SKMapSpan(self).TopRight.ToPosition();
        }

        public static Position BottomLeft(this MapSpan self)
        {
            return new SKMapSpan(self).BottomLeft.ToPosition();
        }

        public static Position BottomRight(this MapSpan self)
        {
            return new SKMapSpan(self).BottomRight.ToPosition();
        }

        public static MapSpan WrapIfRequired(this MapSpan self)
        {
            return new SKMapSpan(self).WrapIfRequired().ToMapSpan();
        }

        public static SKMapSpan WrapIfRequired(this SKMapSpan self)
        {
            SKMapSpan result = self;

            if (self.Crosses180thMeridian())
            {
                // If we cross the 180th meridian, our map span is the full world width
                result = new SKMapSpan(new Position(result.Center.Latitude, 0), result.LatitudeDegrees, MaxLongitude);
            }

            return result;
        }

        public static bool Crosses180thMeridian(this MapSpan self)
        {
            return new SKMapSpan(self).Crosses180thMeridian();
        }

        public static bool Crosses180thMeridianRight(this MapSpan self)
        {
            return new SKMapSpan(self).Crosses180thMeridianRight();
        }

        public static bool Crosses180thMeridianLeft(this MapSpan self)
        {
            return new SKMapSpan(self).Crosses180thMeridianLeft();
        }

        public static bool Crosses180thMeridian(this SKMapSpan self)
        {
            return self.Crosses180thMeridianRight() ||
                   self.Crosses180thMeridianLeft();
        }

        public static bool Crosses180thMeridianRight(this SKMapSpan self)
        {
            return self.Center.Longitude + self.LongitudeDegrees > MaxLongitude;
        }

        public static bool Crosses180thMeridianLeft(this SKMapSpan self)
        {
            return self.Center.Longitude - self.LongitudeDegrees < MinLongitude;
        }
    }
}
