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

using MapKit;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.iOS.Helpers
{
    public static class MapSpanExtension
    {
        public static MKMapRect ToMapRect(this MapSpan self)
        {
            Position gpsTopLeft = new Position(self.Center.Latitude + self.LatitudeDegrees, self.Center.Longitude - self.LongitudeDegrees);
            Position gpsBottomRight = new Position(self.Center.Latitude - self.LatitudeDegrees, self.Center.Longitude + self.LongitudeDegrees);
            MKMapPoint mapTopLeft = MKMapPoint.FromCoordinate(gpsTopLeft.ToLocationCoordinate());
            MKMapPoint mapBottomRight = MKMapPoint.FromCoordinate(gpsBottomRight.ToLocationCoordinate());
            MKMapSize mapSize = new MKMapSize(mapBottomRight.X - mapTopLeft.X,
                                              mapBottomRight.Y - mapTopLeft.Y);

            return new MKMapRect(mapTopLeft, mapSize);
        }

        public static MapSpan ToMapSpan(this MKMapRect self)
        {
            MKMapPoint mapTopLeft = new MKMapPoint(self.MinX, self.MinY);
            MKMapPoint mapBottomRight = new MKMapPoint(self.MaxX, self.MaxY);
            Position gpsTopLeft = MKMapPoint.ToCoordinate(mapTopLeft)
                                            .ToPosition();
            Position gpsBottomRight = MKMapPoint.ToCoordinate(mapBottomRight)
                                                .ToPosition();
            Position gpsCenter = new Position((gpsTopLeft.Latitude + gpsBottomRight.Latitude) * 0.5,
                                              (gpsTopLeft.Longitude + gpsBottomRight.Longitude) * 0.5);

            return new MapSpan(gpsCenter, gpsTopLeft.Latitude - gpsBottomRight.Latitude, gpsBottomRight.Longitude - gpsTopLeft.Longitude);
        }
    }
}
