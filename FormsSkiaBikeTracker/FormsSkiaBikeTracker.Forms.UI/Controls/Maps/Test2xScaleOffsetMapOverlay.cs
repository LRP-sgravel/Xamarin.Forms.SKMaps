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

using FormsSkiaBikeTracker.Forms.UI.Pages;
using FormsSkiaBikeTracker.Shared.Helpers;
using FormsSkiaBikeTracker.Shared.Models.Maps;
using LRPLib.Views.XForms.Extensions;
using MvvmCross.Platform.Platform;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Controls.Maps
{
    public class Test2xScaleOffsetMapOverlay : DrawableMapOverlay
    {
        private MapSpan baseBounds;

        public Test2xScaleOffsetMapOverlay()
        {
            GpsBounds = new MapSpan(new Position(MapSpanExtensions.MaxLatitude - 1, -179), 1, 1);
        }

        public override void DrawOnMap(SKMapCanvas canvas, SKMapSpan canvasMapRect, double pixelScale)
        {
            SKPaint paint = new SKPaint();

            paint.IsAntialias = true;
            paint.StrokeWidth = 1;
            paint.Color = Color.Fuchsia.ToSKColor();

            MvxTrace.Trace($"Drawing line with coords \n" +
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
