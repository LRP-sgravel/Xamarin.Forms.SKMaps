// **********************************************************************
// 
//   RectangleExtension.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using MapKit;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.iOS.Helpers
{
    public static class RectangleExtension
    {
        public static MKMapRect ToMapRect(this Rectangle self)
        {
            Point gpsTopLeft = new Point(self.Left, self.Top);
            Point gpsBottomRight = new Point(self.Right, self.Bottom);
            MKMapPoint mapTopLeft = MKMapPoint.FromCoordinate(gpsTopLeft.ToLocationCoordinate());
            MKMapPoint mapBottomRight = MKMapPoint.FromCoordinate(gpsBottomRight.ToLocationCoordinate());
            MKMapSize mapSize = new MKMapSize(mapTopLeft.X - mapBottomRight.X,
                                              mapTopLeft.Y - mapBottomRight.Y);

            return new MKMapRect(mapTopLeft, mapSize);
        }
    }
}
