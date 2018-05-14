// **********************************************************************
// 
//   SKMapPath.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using Xamarin.Forms.Maps.Overlays.Models;
using SkiaSharp;

namespace Xamarin.Forms.Maps.Overlays.Skia
{
    public class SKMapPath
    {
        public SKPath MapCanvasPath { get; }

        private SKMapCanvas _MapCanvas { get; }

        public bool IsConcave => MapCanvasPath.IsConcave;

        public bool IsConvex => MapCanvasPath.IsConvex;

        public bool IsEmpty => MapCanvasPath.IsEmpty;

        public SKPathSegmentMask SegmentMasks => MapCanvasPath.SegmentMasks;

        public SKPathConvexity Convexity
        {
            get => MapCanvasPath.Convexity;
            set => MapCanvasPath.Convexity = value;
        }

        public SKPathFillType FillType
        {
            get => MapCanvasPath.FillType;
            set => MapCanvasPath.FillType = value;
        }

        public SKMapSpan Bounds
        {
            get
            {
                SKRect canvasBounds = MapCanvasPath.Bounds;
                SKMapSpan gpsSpan = _MapCanvas.ConvertLocalToSpan(canvasBounds);

                return gpsSpan;
            }
        }

        public SKMapSpan TightBounds
        {
            get
            {
                SKRect canvasTightBounds = MapCanvasPath.TightBounds;
                SKMapSpan gpsSpan = _MapCanvas.ConvertLocalToSpan(canvasTightBounds);

                return gpsSpan;
            }
        }

        internal SKMapPath(SKMapCanvas mapCanvas)
        {
            MapCanvasPath = new SKPath();
            _MapCanvas = mapCanvas;
        }

        public void Close()
        {
            MapCanvasPath.Close();
        }

        public void AddRect(MapSpan gpsSpan, SKPathDirection direction = SKPathDirection.Clockwise)
        {
            SKRect canvasRect = _MapCanvas.ConvertSpanToLocal(gpsSpan);

            MapCanvasPath.AddRect(canvasRect, direction);
        }

        public void AddRoundedRect(MapSpan gpsSpan, float radiusX, float radiusY, SKPathDirection direction = SKPathDirection.Clockwise)
        {
            SKRect canvasRect = _MapCanvas.ConvertSpanToLocal(gpsSpan);

            MapCanvasPath.AddRoundedRect(canvasRect, radiusX, radiusY, direction);
        }

        public void ArcTo(double gps1X, double gps1Y, double gps2X, double gps2Y, float radius)
        {
            ArcTo(new Position(gps1X, gps1Y), new Position(gps2X, gps2Y), radius);
        }

        public void ArcTo(Position gpsDestination, SKPoint radiiPixels, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep)
        {
            SKPoint canvasDestination = _MapCanvas.ConvertPositionToLocal(gpsDestination);

            MapCanvasPath.ArcTo(radiiPixels, xAxisRotate, largeArc, sweep, canvasDestination);
        }

        public void ArcTo(Position gpsDestination, Position gpsCorner, float radiusPixels)
        {
            SKPoint canvasCorner = _MapCanvas.ConvertPositionToLocal(gpsCorner);
            SKPoint canvasDestination = _MapCanvas.ConvertPositionToLocal(gpsDestination);

            MapCanvasPath.ArcTo(canvasCorner, canvasDestination, radiusPixels);
        }

        public void ArcTo(MapSpan gpsArea, float startAngle, float sweepAngle, bool forceMoveTo)
        {
            SKRect canvasOval = _MapCanvas.ConvertSpanToLocal(gpsArea);

            MapCanvasPath.ArcTo(canvasOval, startAngle, sweepAngle, forceMoveTo);
        }

        public void ArcTo(Position gpsDestination, float radiusXPixels, float radiusYPixels, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep)
        {
            ArcTo(gpsDestination, new SKPoint(radiusXPixels, radiusYPixels), xAxisRotate, largeArc, sweep);
        }

        public void ConicTo(Position gpsPosition1, Position gpsPosition2, float weight)
        {
            SKPoint canvasPoint1 = _MapCanvas.ConvertPositionToLocal(gpsPosition1);
            SKPoint canvasPoint2 = _MapCanvas.ConvertPositionToLocal(gpsPosition2);

            MapCanvasPath.ArcTo(canvasPoint1, canvasPoint2, weight);
        }

        public void ConicTo(double gpsX1, double gpsY1, double gpsX2, double gpsY2, float weight)
        {
            ConicTo(new Position(gpsX1, gpsY1), new Position(gpsX2, gpsY2), weight);
        }

        public void CubicTo(Position gpsPosition1, Position gpsPosition2, Position gpsPosition3)
        {
            SKPoint canvasPoint1 = _MapCanvas.ConvertPositionToLocal(gpsPosition1);
            SKPoint canvasPoint2 = _MapCanvas.ConvertPositionToLocal(gpsPosition2);
            SKPoint canvasPoint3 = _MapCanvas.ConvertPositionToLocal(gpsPosition3);

            MapCanvasPath.CubicTo(canvasPoint1, canvasPoint2, canvasPoint3);
        }

        public void CubicTo(double gpsX1, double gpsY1, double gpsX2, double gpsY2, double gpsX3, double gpsY3)
        {
            CubicTo(new Position(gpsX1, gpsY1), new Position(gpsX2, gpsY2), new Position(gpsX3, gpsY3));
        }

        public void LineTo(Position gpsPoint)
        {
            SKPoint canvasPoint = _MapCanvas.ConvertPositionToLocal(gpsPoint);

            MapCanvasPath.LineTo(canvasPoint);
        }

        public void LineTo(double gpsX, double gpsY)
        {
            LineTo(new Position(gpsX, gpsY));
        }

        public void MoveTo(Position gpsPoint)
        {
            SKPoint canvasPoint = _MapCanvas.ConvertPositionToLocal(gpsPoint);

            MapCanvasPath.MoveTo(canvasPoint);
        }

        public void MoveTo(double gpsX, double gpsY)
        {
            MoveTo(new Position(gpsX, gpsY));
        }

        public void QuadTo(Position gpsPosition1, Position gpsPosition2)
        {
            SKPoint canvasPoint1 = _MapCanvas.ConvertPositionToLocal(gpsPosition1);
            SKPoint canvasPoint2 = _MapCanvas.ConvertPositionToLocal(gpsPosition2);

            MapCanvasPath.QuadTo(canvasPoint1, canvasPoint2);
        }

        public void QuadTo(double gpsX1, double gpsY1, double gpsX2, double gpsY2)
        {
            QuadTo(new Position(gpsX1, gpsY1), new Position(gpsX2, gpsY2));
        }
    }
}
