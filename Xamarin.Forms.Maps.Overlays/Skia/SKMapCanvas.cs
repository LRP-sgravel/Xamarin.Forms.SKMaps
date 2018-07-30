// **********************************************************************
// 
//   SKMapCanvas.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using System;
using System.Collections.Generic;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;
using MathNet.Numerics.LinearAlgebra;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Xamarin.Forms.Maps.Overlays.Skia
{
    public class SKMapCanvas : IDisposable
    {
        public const int MapTileSize = 256;
        public const int HalfMapTileSize = 256 >> 1;
        public const int MaxZoomLevel = 17;
        public static double MaxZoomScale => Math.Pow(2, -MaxZoomLevel);
        private static double PlatformPixelScale => Device.RuntimePlatform == Device.iOS ? 2 : 1;

        private SKCanvas _Canvas { get; set; }

        private float _BitmapDensity { get; }
        private Rectangle _MercatorRenderArea { get; }
        private double _ScaleFactor { get; }
        private Matrix<double> _MercatorMatrix { get; set; }
        private bool _InvertedYCoordinates { get; set; }

        private Stack<Matrix<double>> _MatrixStack { get; } = new Stack<Matrix<double>>();

        internal SKMapCanvas(SKBitmap bitmap, Rectangle mercatorRenderArea, double scaleFactor, bool invertedYCoordinates = false)
        {
            _BitmapDensity = bitmap.Height / (float)MapTileSize;
            _MercatorRenderArea = mercatorRenderArea;
            _ScaleFactor = scaleFactor;
            _MercatorMatrix = CreateDefaultTileBaseMatrix();
            _InvertedYCoordinates = invertedYCoordinates;

            _Canvas = new SKCanvas(bitmap);

            if (_InvertedYCoordinates)
            {
                _Canvas.Scale(1, -1);
                _Canvas.Translate(0, -bitmap.Height);
            }
            _Canvas.Scale(_BitmapDensity);
        }

        public void Dispose()
        {
            _Canvas?.Dispose();
            _Canvas = null;
        }

        public SKMapPath CreateEmptyMapPath()
        {
            return new SKMapPath(this);
        }

        public void Save()
        {
            _MatrixStack.Push(_MercatorMatrix);
            _MercatorMatrix = _MercatorMatrix.Clone();
        }

        public void Restore()
        {
            if (_MatrixStack.Count > 0)
            {
                _MercatorMatrix = _MatrixStack.Pop();
            }
            else
            {
                _MercatorMatrix = CreateDefaultTileBaseMatrix();
            }
        }

        public void RestoreToCount(int count)
        {
            if (_MatrixStack.Count > count)
            {
                while (_MatrixStack.Count > count)
                {
                    _MercatorMatrix = _MatrixStack.Pop();
                }
            }
            else
            {
                _MatrixStack.Clear();
                _MercatorMatrix = CreateDefaultTileBaseMatrix();
            }
        }

        public void Concat(Matrix<double> matrix)
        {
            _MercatorMatrix *= matrix;
        }

        public void SetMatrix(Matrix<double> matrix)
        {
            _MercatorMatrix = matrix;
        }

        public void RotateDegrees(double degrees)
        {
            _MercatorMatrix *= Matrix<double>.Build.RotationDegrees(degrees);
        }

        public void RotateDegrees(double degrees, double offsetX, double offsetY)
        {
            RotateDegrees(degrees, new Position(offsetX, offsetY));
        }

        public void RotateDegrees(double degrees, Position gpsOffset)
        {
            Point mercatorReference = gpsOffset.ToMercator();

            _MercatorMatrix *= Matrix<double>.Build.RotationDegreesAround(degrees, mercatorReference.X, mercatorReference.Y);
        }

        public void RotateRadians(double radians)
        {
            _MercatorMatrix *= Matrix<double>.Build.RotationRadians(radians);
        }

        public void RotateRadians(double radians, double offsetX, double offsetY)
        {
            RotateRadians(radians, new Position(offsetX, offsetY));
        }

        public void RotateRadians(double radians, Position gpsOffset)
        {
            Point mercatorReference = gpsOffset.ToMercator();

            _MercatorMatrix *= Matrix<double>.Build.RotationRadiansAround(radians, mercatorReference.X, mercatorReference.Y);
        }

        public void Scale(double scale)
        {
            _MercatorMatrix *= Matrix<double>.Build.Scale(scale);
        }

        public void Scale(double scaleX, double scaleY)
        {
            _MercatorMatrix *= Matrix<double>.Build.Scale(scaleX, scaleY);
        }

        public void Scale(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            Scale(scaleX, scaleY, new Position(offsetX, offsetY));
        }

        public void Scale(double scaleX, double scaleY, Position gpsOffset)
        {
            Point mercatorReference = gpsOffset.ToMercator();

            _MercatorMatrix *= Matrix<double>.Build.ScaleAround(scaleX, scaleY, mercatorReference.X, mercatorReference.Y);
        }

        public void Skew(double skewX, double skewY)
        {
            _MercatorMatrix *= Matrix<double>.Build.Skew(skewX, skewY);
        }

        public void Translate(double transX, double transY, double atX, double atY)
        {
            Translate(new Position(transX, transY),
                      new Position(atX, atY));
        }

        public void Translate(Position gpsOffset, Position atPosition)
        {
            Point mercatorPoint = atPosition.ToMercator();
            Point mercatorOffset = gpsOffset.ToMercator();

            _MercatorMatrix *= Matrix<double>.Build.Translation(mercatorOffset.X - mercatorOffset.X,
                                                                mercatorOffset.Y - mercatorPoint.Y);
        }

        public void Clear()
        {
            _Canvas.Clear();
        }

        public void Clear(SKColor color)
        {
            _Canvas.Clear(color);
        }

        public void DrawOval(double latitude, double longitude, double latitudeDegrees, double longitudeDegrees, SKPaint paint)
        {
            DrawOval(new MapSpan(new Position(latitude, longitude), latitudeDegrees, longitudeDegrees), paint);
        }

        public void DrawOval(MapSpan gpsSpan, SKPaint paint)
        {
            SKRect canvasRect = ConvertSpanToLocal(gpsSpan);

            _Canvas.DrawOval(canvasRect, paint);
        }

        public void DrawLine(double startLatitude, double startLongitude, double endLatitude, double endLongitude, SKPaint paint, bool shortLine = true)
        {
            DrawLine(new SKMapPosition(startLatitude, startLongitude), new SKMapPosition(endLatitude, endLongitude), paint, shortLine);
        }

        public void DrawLine(Position start, Position end, SKPaint paint, bool shortLine = true)
        {
            DrawLine(new SKMapPosition(start), new SKMapPosition(end), paint, shortLine);
        }

        internal void DrawLine(SKMapPosition start, SKMapPosition end, SKPaint paint, bool shortLine = true)
        {
            Tuple<SKMapPosition, SKMapPosition> line = new Tuple<SKMapPosition, SKMapPosition>(start, end);
            Tuple<SKPoint, SKPoint> canvasLine = ConvertLineToLocal(line, shortLine);

            _Canvas.DrawLine(canvasLine.Item1.X, canvasLine.Item1.Y, canvasLine.Item2.X, canvasLine.Item2.Y, paint);
        }

        public void DrawPath(SKMapPath path, SKPaint paint)
        {
            _Canvas.DrawPath(path.MapCanvasPath, paint);
        }

        public void DrawRect(MapSpan gpsSpan, SKPaint paint)
        {
            SKRect canvasDest = ConvertSpanToLocal(gpsSpan);

            _Canvas.DrawRect(canvasDest, paint);
        }

        public void DrawImage(SKImage image, MapSpan gpsSpan, SKPaint paint = null)
        {
            DrawImage(image, new SKRect(0, 0, image.Width, image.Height), gpsSpan, paint);
        }

        public void DrawImage(SKImage image, Position gpsPosition, SKPaint paint = null)
        {
            Size imageMapSize = PixelsToMapSizeAtScale(new Size(image.Width, image.Height), gpsPosition, _ScaleFactor);
            MapSpan imageMapSpan = new MapSpan(gpsPosition, imageMapSize.Height * 0.5, imageMapSize.Width * 0.5);

            DrawImage(image, imageMapSpan, paint);
        }

        public void DrawImage(SKImage image, SKRect source, MapSpan gpsSpan, SKPaint paint = null)
        {
            SKRect canvasDest = ConvertSpanToLocal(gpsSpan);

            _Canvas.DrawImage(image, source, canvasDest, paint);
        }

        public void DrawBitmap(SKBitmap bitmap, MapSpan gpsSpan, SKPaint paint = null)
        {
            DrawBitmap(bitmap, new SKRect(0, 0, bitmap.Width, bitmap.Height), gpsSpan, paint);
        }

        public void DrawBitmap(SKBitmap bitmap, Position gpsPosition, SKPaint paint = null)
        {
            Size imageMapSize = PixelsToMapSizeAtScale(new Size(bitmap.Width, bitmap.Height), gpsPosition, _ScaleFactor);
            MapSpan imageMapSpan = new MapSpan(gpsPosition, imageMapSize.Height * 0.5, imageMapSize.Width * 0.5);

            DrawBitmap(bitmap, imageMapSpan, paint);
        }

        public void DrawBitmap(SKBitmap bitmap, SKRect source, MapSpan gpsSpan, SKPaint paint = null)
        {
            SKRect canvasDest = ConvertSpanToLocal(gpsSpan);

            _Canvas.DrawBitmap(bitmap, source, canvasDest, paint);
        }

        public void DrawPicture(SKPicture picture, Position gpsPosition, Size destinationSize, SKPaint paint = null)
        {
            Matrix<double> matrix = GetPictureDrawMatrix(picture, gpsPosition, destinationSize);
            SKMatrix canvasMatrix = matrix.ToSKMatrix();

            _Canvas.DrawPicture(picture, ref canvasMatrix, paint);
        }
        
        public static Size PixelsToMaximumMapSizeAtScale(Size pixelsSize, double zoomScale)
        {
            SKMapSpan gpsArea = PixelsToMaximumMapSpanAtScale(pixelsSize, zoomScale);

            return new Size(gpsArea.LongitudeDegrees * 2, gpsArea.LatitudeDegrees * 2);
        }

        public static SKMapSpan PixelsToMaximumMapSpanAtScale(Size pixelsSize, double zoomScale)
        {
            return PixelsToMapSpanAtScale(pixelsSize,
                                          new Point(SKMapExtensions.MercatorCenterOffset, SKMapExtensions.MercatorCenterOffset),
                                          zoomScale);
        }

        public static Size PixelsToMapSizeAtScale(Size pixelsSize, Position mapPosition, double zoomScale)
        {
            SKMapSpan gpsArea = PixelsToMapSpanAtScale(pixelsSize, mapPosition, zoomScale);

            return new Size(gpsArea.LongitudeDegrees * 2, gpsArea.LatitudeDegrees * 2);
        }

        public static SKMapSpan PixelsToMapSpanAtScale(Size pixelsSize, Position mapPosition, double zoomScale)
        {
            Point mercatorPosition = mapPosition.ToMercator();

            return PixelsToMapSpanAtScale(pixelsSize, mercatorPosition, zoomScale);
        }

        internal static SKMapSpan PixelsToMapSpanAtScale(Size pixelSize, Point mercatorPosition, double zoomScale)
        {
            Size platformPixelsSize = pixelSize * PlatformPixelScale;
            Rectangle sizeRect = new Rectangle(Point.Zero, platformPixelsSize);
            Rectangle mercatorRectAtPosition = new Rectangle(mercatorPosition.X - (platformPixelsSize.Width / zoomScale) * 0.5,
                                                             mercatorPosition.Y - (platformPixelsSize.Height / zoomScale) * 0.5,
                                                             platformPixelsSize.Width / zoomScale,
                                                             platformPixelsSize.Height / zoomScale);
            Matrix<double> mercatorAtZoom = CreateTileBaseMatrix(mercatorRectAtPosition, zoomScale);
            Matrix<double> originSize = mercatorAtZoom.Inverse() * sizeRect.ToMatrix();

            return originSize.ToRectangle()
                             .ToGps();
        }
        
        private Matrix<double> GetPictureDrawMatrix(SKPicture picture, Position gpsPosition, Size pixelSize)
        {
            Size platformPixelsSize = pixelSize * PlatformPixelScale;
            Matrix<double> matrix = Matrix<double>.Build.DenseIdentity(3, 3);
            Point mercatorPosition = gpsPosition.ToMercator();
            SKRect sourceRect = picture.CullRect;
            SKMapSpan drawSpan = PixelsToMapSpanAtScale(platformPixelsSize, gpsPosition, _ScaleFactor);
            double xScale = platformPixelsSize.Width / sourceRect.Width;
            double yScale = platformPixelsSize.Height / sourceRect.Height;
            SKPoint canvasPoint;

            if (drawSpan.Crosses180thMeridianRight() && _MercatorRenderArea.Left < SKMapExtensions.MercatorCenterOffset)
            {
                // We cross the 180th meridian to the right and are drawing to a tile on the left side of the map, so wrap around
                mercatorPosition.X -= SKMapExtensions.MercatorMapSize;
            }

            if (drawSpan.Crosses180thMeridianLeft() && _MercatorRenderArea.Right > SKMapExtensions.MercatorCenterOffset)
            {
                // We cross the 180th meridian to the right and are drawing to a tile on the left side of the map, so wrap around
                mercatorPosition.X += SKMapExtensions.MercatorMapSize;
            }

            canvasPoint = ApplyMatrix(mercatorPosition);

            matrix *= Matrix<double>.Build.Translation(canvasPoint.X, canvasPoint.Y);
            matrix *= Matrix<double>.Build.Scale(xScale, yScale);
            matrix *= Matrix<double>.Build.Translation(sourceRect.Width * -0.5, sourceRect.Height * -0.5);

            return matrix;
        }

        internal SKPoint ConvertPositionToLocal(Position position)
        {
            Point mercatorStart = position.ToMercator();

            return ApplyMatrix(mercatorStart);
        }

        internal SKRect ConvertSpanToLocal(MapSpan gpsSpan)
        {
            Rectangle mercatorRect = gpsSpan.ToMercator();

            if (gpsSpan.Crosses180thMeridianRight() && _MercatorRenderArea.Left < SKMapExtensions.MercatorCenterOffset)
            {
                // We cross the 180th meridian to the right and are drawing to a tile on the left side of the map, so wrap around
                mercatorRect.X -= SKMapExtensions.MercatorMapSize;
            }
            else if (gpsSpan.Crosses180thMeridianLeft() && _MercatorRenderArea.Right > SKMapExtensions.MercatorCenterOffset)
            {
                // We cross the 180th meridian to the left and are drawing to a tile on the right side of the map, so wrap around
                mercatorRect.X += SKMapExtensions.MercatorMapSize;
            }

            return ApplyMatrix(mercatorRect);
        }

        internal Tuple<SKPoint, SKPoint> ConvertLineToLocal(Tuple<SKMapPosition, SKMapPosition> gpsLine, bool toShortLine = true)
        {
            Point mercatorStart = gpsLine.Item1.ToMercator();
            Point mercatorEnd = gpsLine.Item2.ToMercator();
            bool isShortLine = Math.Abs(mercatorEnd.X - mercatorStart.X) < SKMapExtensions.MercatorCenterOffset;

            if (toShortLine != isShortLine)
            {
                if (mercatorEnd.X < 0)
                {
                    mercatorEnd.X += SKMapExtensions.MercatorMapSize;
                }
                else
                {
                    mercatorStart.X += SKMapExtensions.MercatorMapSize;
                }
            }

            if (mercatorStart.X > SKMapExtensions.MercatorMapSize && _MercatorRenderArea.Left < SKMapExtensions.MercatorCenterOffset)
            {
                // We cross the 180th meridian to the right and are drawing to a tile on the left side of the map, so wrap around
                mercatorStart.X -= SKMapExtensions.MercatorMapSize;
                mercatorEnd.X -= SKMapExtensions.MercatorMapSize;
            }
            else if (mercatorEnd.X < 0 && _MercatorRenderArea.Right > SKMapExtensions.MercatorCenterOffset)
            {
                // We cross the 180th meridian to the left and are drawing to a tile on the right side of the map, so wrap around
                mercatorStart.X += SKMapExtensions.MercatorMapSize;
                mercatorEnd.X += SKMapExtensions.MercatorMapSize;
            }

            return new Tuple<SKPoint, SKPoint>(ApplyMatrix(mercatorStart), ApplyMatrix(mercatorEnd));
        }

        internal Position ConvertLocalToPosition(SKPoint canvasPoint)
        {
            Point mercatorPoint = ApplyInverseMatrix(canvasPoint);

            return mercatorPoint.ToGps();
        }

        internal SKMapSpan ConvertLocalToSpan(SKRect canvasBounds)
        {
            Rectangle mercatorRect = ApplyInverseMatrix(canvasBounds);

            return mercatorRect.ToGps();
        }

        private Matrix<double> CreateDefaultTileBaseMatrix()
        {
            return CreateTileBaseMatrix(_MercatorRenderArea, _ScaleFactor);
        }

        private static Matrix<double> CreateTileBaseMatrix(Rectangle mercatorArea, double scale)
        {
            Matrix<double> result = Matrix<double>.Build.DenseIdentity(3, 3);

            result *= Matrix<double>.Build.Scale(scale);
            result *= Matrix<double>.Build.Translation(-mercatorArea.Left, -mercatorArea.Top);

            return result;
        }

        private SKRect ApplyMatrix(Rectangle mercatorRect)
        {
            Matrix<double> source = mercatorRect.ToMatrix();
            Matrix<double> rect = _MercatorMatrix * source;

            return rect.ToSKRect();
        }

        private SKPoint ApplyMatrix(Point mercatorPoint)
        {
            Vector<double> point = mercatorPoint.ToVector();
            Vector<double> result = _MercatorMatrix.Multiply(point);

            return result.ToSKPoint();
        }

        private Rectangle ApplyInverseMatrix(SKRect canvasRect)
        {
            return ApplyInverseMatrix(canvasRect.ToFormsRect());
        }

        private Rectangle ApplyInverseMatrix(Rectangle canvasRect)
        {
            Matrix<double> source = canvasRect.ToMatrix();
            Matrix<double> rect = _MercatorMatrix.Inverse() * source;

            return rect.ToRectangle();
        }

        private Point ApplyInverseMatrix(SKPoint canvasPoint)
        {
            Vector<double> source = canvasPoint.ToFormsPoint().ToVector();
            Vector<double> point = _MercatorMatrix.Inverse() * source;

            return point.ToPoint();
        }
    }
}
