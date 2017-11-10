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
using FormsSkiaBikeTracker.Forms.UI.Helpers;
using MathNet.Numerics.LinearAlgebra;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Controls.Maps
{
    public class SKMapCanvas : IDisposable
    {
        private SKCanvas _Canvas { get; }
        private Rectangle _MercatorRenderArea { get; }
        private double _ScaleFactor { get; }
        private Matrix<double> _MercatorMatrix { get; set; }
        private Stack<Matrix<double>> _MatrixStack { get; set; }

        public SKMapCanvas(SKBitmap bitmap, Rectangle mercatorRenderArea, double scaleFactor)
        {
            _Canvas = new SKCanvas(bitmap);
            _MercatorRenderArea = mercatorRenderArea;
            _ScaleFactor = scaleFactor;
            _MercatorMatrix = CreateTileBaseMatrix();
            _MatrixStack = new Stack<Matrix<double>>();
        }

        public void Dispose()
        {
            _Canvas?.Dispose();
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
                _MercatorMatrix = CreateTileBaseMatrix();
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
                _MercatorMatrix = CreateTileBaseMatrix();
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

        public void RotateDegrees(float degrees)
        {
            _MercatorMatrix *= Matrix<double>.Build.RotationDegrees(degrees);
        }

        public void RotateDegrees(float degrees, float offsetX, float offsetY)
        {
            RotateDegrees(degrees, new Position(offsetX, offsetY));
        }

        public void RotateDegrees(float degrees, Position gpsOffset)
        {
            Point mercatorReference = gpsOffset.ToMercator();

            _MercatorMatrix *= Matrix<double>.Build.RotationDegreesAround(degrees, mercatorReference.X, mercatorReference.Y);
        }

        public void RotateRadians(float radians)
        {
            _MercatorMatrix *= Matrix<double>.Build.RotationRadians(radians);
        }

        public void RotateRadians(float radians, float offsetX, float offsetY)
        {
            RotateRadians(radians, new Position(offsetX, offsetY));
        }

        public void RotateRadians(float radians, Position gpsOffset)
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

        public void DrawOval(float latitude, float longitude, float latitudeDegrees, float longitudeDegrees, SKPaint paint)
        {
            DrawOval(new MapSpan(new Position(latitude, longitude), latitudeDegrees, longitudeDegrees), paint);
        }

        public void DrawOval(MapSpan rect, SKPaint paint)
        {
            SKRect canvasRect = ConvertRectangleToLocal(rect);

            _Canvas.DrawOval(canvasRect, paint);
        }

        public void DrawLine(float latitude, float longitude, float latitudeDegrees, float longitudeDegrees, SKPaint paint)
        {
            DrawLine(new Position(latitude, longitude), new Position(latitudeDegrees, longitudeDegrees), paint);
        }

        public void DrawLine(Position start, Position end, SKPaint paint)
        {
            SKPoint canvasStart = ConvertPositionToLocal(start);
            SKPoint canvasEnd = ConvertPositionToLocal(end);

            _Canvas.DrawLine(canvasStart.X, canvasStart.Y, canvasEnd.X, canvasEnd.Y, paint);
        }

        public SKMapPath GetMapPath()
        {
            return new SKMapPath(this);
        }

        public void DrawPath(SKMapPath path, SKPaint paint)
        {
            _Canvas.DrawPath(path.MapCanvasPath, paint);
        }


        public void DrawPicture(SKPicture picture, Position gpsPosition, SKSize pixelSize, SKPaint paint = null)
        {
            Matrix<double> matrix = GetPictureDrawMatrix(picture, gpsPosition, pixelSize);
            SKMatrix canvasMatrix = matrix.ToSKMatrix();

            _Canvas.DrawPicture(picture, ref canvasMatrix, paint);
        }

        private Matrix<double> GetPictureDrawMatrix(SKPicture picture, Position gpsPosition, SKSize pixelSize)
        {
            Matrix<double> matrix = Matrix<double>.Build.DenseIdentity(3, 3);
            SKPoint canvasPoint = ConvertPositionToLocal(gpsPosition);
            SKRect sourceRect = picture.CullRect;
            float xScale = pixelSize.Width / sourceRect.Width;
            float yScale = pixelSize.Height / sourceRect.Height;

            matrix *= Matrix<double>.Build.Translation(canvasPoint.X, canvasPoint.Y);
            matrix *= Matrix<double>.Build.Scale(xScale, -yScale);
            matrix *= Matrix<double>.Build.Translation(sourceRect.Width * -0.5, sourceRect.Height * -0.5);

            return matrix;
        }

        internal SKPoint ConvertPositionToLocal(Position position)
        {
            Point mercatorStart = position.ToMercator();

            return ApplyMatrix(mercatorStart);
        }

        internal SKRect ConvertRectangleToLocal(MapSpan rect)
        {
            Rectangle mercatorRect = rect.ToMercator();

            return ApplyMatrix(mercatorRect);
        }

        private Matrix<double> CreateTileBaseMatrix()
        {
            Matrix<double> result = Matrix<double>.Build.DenseIdentity(3, 3);

            result *= Matrix<double>.Build.Scale(_ScaleFactor, -_ScaleFactor);
            result *= Matrix<double>.Build.Translation(-_MercatorRenderArea.Left, -_MercatorRenderArea.Bottom);

            return result;
        }

        private SKRect ApplyMatrix(Rectangle mercatorRect)
        {
            Matrix<double> source = mercatorRect.ToMatrix();
            Matrix<double> rect = _MercatorMatrix * source;

            return new SKRect((float)rect[0, 0], (float)rect[0, 1], (float)rect[1, 0], (float)rect[1, 1]);
        }

        private SKPoint ApplyMatrix(Point mercatorPoint)
        {
            Vector<double> point = mercatorPoint.ToVector();
            Vector<double> result = _MercatorMatrix.Multiply(point);

            return new SKPoint((float)result[0], (float)result[1]);
        }
    }
}
