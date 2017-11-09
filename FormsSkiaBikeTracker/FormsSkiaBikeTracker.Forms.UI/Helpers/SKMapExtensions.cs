// **********************************************************************
// 
//   SKMapExtensions.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using System;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Helpers
{
    public static class SKMapExtensions
    {
        // Skia uses float instead of double.  Therefore the map size does not fit in the integer portion of the
        //  float and this obviously leads to issues
        public const double MercatorCenterOffset = 134217728;
        private const double MercatorRadius = MercatorCenterOffset / Math.PI;

        public static Rectangle ToMercator(this MapSpan gpsRect)
        {
            Point gpsTopLeft = gpsRect.TopLeft().ToMercator();
            Point gpsBottomRight = gpsRect.BottomRight().ToMercator();

            return new Rectangle(gpsTopLeft.X,
                                 gpsTopLeft.Y,
                                 gpsBottomRight.X - gpsTopLeft.X,
                                 gpsBottomRight.Y - gpsTopLeft.Y);
        }

        public static Point ToMercator(this Position gpsCoords)
        {
            double x = MercatorCenterOffset +
                            MercatorRadius * gpsCoords.Longitude * Math.PI / 180.0;
            double y = MercatorCenterOffset -
                            MercatorRadius *
                            Math.Log((1 + Math.Sin(gpsCoords.Latitude * Math.PI / 180.0)) /
                                    (1 - Math.Sin(gpsCoords.Latitude * Math.PI / 180.0))) /
                            2.0;

            return new Point(x, y);
        }

        // Inverse Mercator
        public static MapSpan ToGps(this Rectangle mercatorRect)
        {
            Position mercatorTopLeft = mercatorRect.Location.ToGps();
            Position mercatorBottomRight = (mercatorRect.Location + mercatorRect.Size).ToGps();

            return new MapSpan(new Position((mercatorTopLeft.Latitude + mercatorBottomRight.Latitude) * 0.5,
                                            (mercatorTopLeft.Longitude + mercatorBottomRight.Longitude) * 0.5),
                               Math.Abs(mercatorTopLeft.Latitude - mercatorBottomRight.Latitude) * 0.5,
                               Math.Abs(mercatorTopLeft.Longitude - mercatorBottomRight.Longitude) * 0.5);
        }

        public static Position ToGps(this Point mercatorCoords)
        {
            double latitude = (Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp((mercatorCoords.Y - MercatorCenterOffset) / MercatorRadius))) * 180.0 / Math.PI;
            double longitude = ((mercatorCoords.X - MercatorCenterOffset) / MercatorRadius) * 180.0 / Math.PI;

            return new Position(latitude, longitude);
        }
    }
}
