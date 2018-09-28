// **********************************************************************
// 
//   TestMapOverlay.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using MvvmCross.Logging;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Overlays;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Maps.Overlays.Skia;

namespace FormsSkiaBikeTracker.Forms.Controls.Maps
{
    public class Test2xScaleOffsetMapOverlay : SKMapOverlay
    {
        public Test2xScaleOffsetMapOverlay()
        {
            GpsBounds = new MapSpan(new Position(MapSpanExtensions.MaxLatitude - 1, -179), 1, 1);
        }

        public override void DrawOnMap(SKMapCanvas canvas, SKMapSpan canvasMapRect, double zoomScale)
        {
            SKPaint paint = new SKPaint();

            paint.IsAntialias = true;
            paint.StrokeWidth = 1;
            paint.Color = Color.Fuchsia.ToSKColor();

            MvxLog.Instance.Log(MvxLogLevel.Trace,
                                () => $"Drawing line with coords \n" +
                                      $"[{GpsBounds.Center.Longitude - GpsBounds.LongitudeDegrees - canvasMapRect.LongitudeDegrees}, " +
                                      $"{GpsBounds.Center.Latitude - GpsBounds.LatitudeDegrees - canvasMapRect.LatitudeDegrees}; \n" +
                                      $" {GpsBounds.Center.Longitude + GpsBounds.LongitudeDegrees}, " +
                                      $"{GpsBounds.Center.Latitude + GpsBounds.LatitudeDegrees}]");

            canvas.DrawLine(GpsBounds.Center.Latitude + GpsBounds.LatitudeDegrees - canvasMapRect.LatitudeDegrees,
                            GpsBounds.Center.Longitude + GpsBounds.LongitudeDegrees - canvasMapRect.LongitudeDegrees,
                            GpsBounds.Center.Latitude + GpsBounds.LatitudeDegrees,
                            GpsBounds.Center.Longitude + GpsBounds.LongitudeDegrees,
                            paint);
        }
    }
}
