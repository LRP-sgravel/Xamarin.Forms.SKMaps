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

using FormsSkiaBikeTracker.Forms.UI.Helpers;
using MapKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.iOS.Helpers
{
    public static class MapSpanExtension
    {
        public static MKMapRect ToMapRect(this MapSpan self)
        {
            MKMapPoint mapTopLeft = MKMapPoint.FromCoordinate(self.TopLeft().ToLocationCoordinate());
            MKMapPoint mapBottomRight = MKMapPoint.FromCoordinate(self.BottomRight().ToLocationCoordinate());
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

            return new MapSpan(gpsCenter,
                               (gpsTopLeft.Latitude - gpsBottomRight.Latitude) * 0.5,
                               (gpsBottomRight.Longitude - gpsTopLeft.Longitude) * 0.5);
        }

        public static Rectangle ToRectangle(this MKMapRect self)
        {
            return new Rectangle(self.MinX, self.MinY, self.Width, self.Height);
        }
    }
}
