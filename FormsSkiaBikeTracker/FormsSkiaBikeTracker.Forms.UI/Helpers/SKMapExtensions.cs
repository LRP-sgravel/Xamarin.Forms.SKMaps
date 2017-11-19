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
using FormsSkiaBikeTracker.Forms.UI.Pages;
using MathNet.Numerics.LinearAlgebra;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Helpers
{
    public static class SKMapExtensions
    {
        // Skia uses float instead of double.  Therefore the map size does not fit in the integer portion of the
        //  float and this obviously leads to issues
        public const double MercatorMapSize = 268435456;
        public const double MercatorCenterOffset = MercatorMapSize * 0.5;
        private const double MercatorRadius = MercatorCenterOffset / Math.PI;

        public static Rectangle ToMercator(this MapSpan gpsSpan)
        {
            SKMapPosition topLeftGps = gpsSpan.TopLeft();
            SKMapPosition bottomRightGps = gpsSpan.BottomRight();
            Point canvasTopLeft = topLeftGps.ToMercator();
            Point canvasBottomRight = bottomRightGps.ToMercator();

            return new Rectangle(canvasTopLeft.X,
                                 canvasTopLeft.Y,
                                 canvasBottomRight.X - canvasTopLeft.X,
                                 canvasBottomRight.Y - canvasTopLeft.Y);
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

        public static Point ToMercator(this SKMapPosition gpsCoords)
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
            SKMapPosition canvasTopLeft = new Point(mercatorRect.Left, mercatorRect.Top).ToSKMapPosition();
            SKMapPosition canvasBottomRight = new Point(mercatorRect.Right, mercatorRect.Bottom).ToSKMapPosition();

            return new MapSpan(new Position((canvasTopLeft.Latitude + canvasBottomRight.Latitude) * 0.5,
                                            (canvasTopLeft.Longitude + canvasBottomRight.Longitude) * 0.5),
                               Math.Abs(canvasTopLeft.Latitude - canvasBottomRight.Latitude) * 0.5,
                               Math.Abs(canvasTopLeft.Longitude - canvasBottomRight.Longitude) * 0.5);
        }

        public static Position ToGps(this Point mercatorCoords)
        {
            double latitude = (Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp((mercatorCoords.Y - MercatorCenterOffset) / MercatorRadius))) * 180.0 / Math.PI;
            double longitude = ((mercatorCoords.X - MercatorCenterOffset) / MercatorRadius) * 180.0 / Math.PI;

            return new Position(latitude, longitude);
        }

        internal static SKMapPosition ToSKMapPosition(this Point mercatorCoords)
        {
            double latitude = (Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp((mercatorCoords.Y - MercatorCenterOffset) / MercatorRadius))) * 180.0 / Math.PI;
            double longitude = ((mercatorCoords.X - MercatorCenterOffset) / MercatorRadius) * 180.0 / Math.PI;

            return new SKMapPosition(latitude, longitude);
        }

        public static SKMatrix ToSKMatrix(this Matrix<double> doubleMatrix)
        {
            return new SKMatrix
                   {
                       ScaleX = (float)doubleMatrix[0, 0],
                       SkewX = (float)doubleMatrix[0, 1],
                       TransX = (float)doubleMatrix[0, 2],
                       SkewY = (float)doubleMatrix[1, 0],
                       ScaleY = (float)doubleMatrix[1, 1],
                       TransY = (float)doubleMatrix[1, 2],
                       Persp0 = (float)doubleMatrix[2, 0],
                       Persp1 = (float)doubleMatrix[2, 1],
                       Persp2 = (float)doubleMatrix[2, 2],
                   };
        }
    }
}
