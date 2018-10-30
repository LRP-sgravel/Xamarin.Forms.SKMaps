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
using Xamarin.Forms.SKMaps.Models;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.SKMaps.Extensions
{
    public static class SKMapExtensions
    {
        // Skia uses float instead of double.  Therefore the map size does not fit in the integer portion of the
        //  float and this obviously leads to issues
        public const int MercatorMapSize = 268435456;
        public const int MercatorCenterOffset = MercatorMapSize >> 1;
        private const double MercatorRadius = MercatorCenterOffset / Math.PI;

        public static Rectangle ToMercator(this MapSpan gpsSpan)
        {
            return new SKMapSpan(gpsSpan).ToMercator();
        }

        public static Rectangle ToMercator(this SKMapSpan gpsSpan)
        {
            SKMapPosition topLeftGps = gpsSpan.TopLeft;
            SKMapPosition bottomRightGps = gpsSpan.BottomRight;
            Point canvasTopLeft = topLeftGps.ToMercator();
            Point canvasBottomRight = bottomRightGps.ToMercator();

            return new Rectangle(canvasTopLeft.X,
                                 canvasTopLeft.Y,
                                 canvasBottomRight.X - canvasTopLeft.X,
                                 canvasBottomRight.Y - canvasTopLeft.Y);
        }

        public static Point ToMercator(this Position gpsCoords)
        {
            return new SKMapPosition(gpsCoords).ToMercator();
        }

        public static Point ToMercator(this SKMapPosition gpsCoords)
        {
            double x = (gpsCoords.Longitude + MapSpanExtensions.MaxLongitude) / MapSpanExtensions.WorldLongitude * MercatorMapSize;
            double y = MercatorCenterOffset -
                            MercatorRadius *
                            Math.Log((1 + Math.Sin(gpsCoords.Latitude * Math.PI / 180.0)) /
                                    (1 - Math.Sin(gpsCoords.Latitude * Math.PI / 180.0))) /
                            2.0;

            return new Point(x, y);
        }

        // Inverse Mercator
        public static SKMapSpan ToGps(this Rectangle mercatorRect)
        {
            SKMapPosition canvasTopLeft = new Point(mercatorRect.Left, mercatorRect.Top).ToSKMapPosition();
            SKMapPosition canvasBottomRight = new Point(mercatorRect.Right, mercatorRect.Bottom).ToSKMapPosition();
            double centerLatitude = (canvasTopLeft.Latitude + canvasBottomRight.Latitude) * 0.5;
            double centerLongitude = (canvasTopLeft.Longitude + canvasBottomRight.Longitude) * 0.5;
            double spanLatitude = (canvasTopLeft.Latitude - canvasBottomRight.Latitude) * 0.5;
            double spanLongitude = (canvasBottomRight.Longitude - canvasTopLeft.Longitude) * 0.5;

            return new SKMapSpan(new Position(centerLatitude, centerLongitude),
                                 spanLatitude,
                                 spanLongitude);
        }

        public static Position ToGps(this Point mercatorCoords)
        {
            SKMapPosition mapPosition = mercatorCoords.ToSKMapPosition();

            return new Position(mapPosition.Latitude, mapPosition.Longitude);
        }

        internal static SKMapPosition ToSKMapPosition(this Point mercatorCoords)
        {
            double latitude = (Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp((mercatorCoords.Y - MercatorCenterOffset) / MercatorRadius))) * 180.0 / Math.PI;
            double longitude = (mercatorCoords.X / MercatorMapSize) * MapSpanExtensions.WorldLongitude - MapSpanExtensions.MaxLongitude;

            return new SKMapPosition(latitude, longitude);
        }
    }
}
