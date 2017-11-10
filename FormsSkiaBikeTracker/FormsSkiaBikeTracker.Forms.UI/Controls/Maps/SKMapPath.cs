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

using SkiaSharp;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Controls.Maps
{
    public class SKMapPath
    {
        public SKPath MapCanvasPath { get; }

        private SKMapCanvas _MapCanvas { get; }

        internal SKMapPath(SKMapCanvas mapCanvas)
        {
            MapCanvasPath = new SKPath();
            _MapCanvas = mapCanvas;
        }

        public void Close()
        {
            MapCanvasPath.Close();
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
    }
}
