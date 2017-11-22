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
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Controls.Maps
{
    public class TestLineMapOverlay : DrawableMapOverlay
    {
        private SKMapSpan baseBounds;

        public TestLineMapOverlay()
        {
            baseBounds = new SKMapSpan(new Position(0, -180), 1, 1);
            GpsBounds = MapSpanExtensions.WorldSpan;
        }

        public override void DrawOnMap(SKMapCanvas canvas, SKMapSpan canvasMapRect, double pixelScale)
        {
            SKPaint paint = new SKPaint();

            paint.IsAntialias = true;
            paint.StrokeWidth = 1;
/*            paint.Color = Color.Fuchsia.ToSKColor();

            if (canvasMapRect.Center.Latitude > 0)
            {
                canvas.DrawLine(canvasMapRect.Center.Latitude - canvasMapRect.LatitudeDegrees,
                                canvasMapRect.Center.Longitude - canvasMapRect.LongitudeDegrees,
                                canvasMapRect.Center.Latitude + canvasMapRect.LatitudeDegrees,
                                canvasMapRect.Center.Longitude + canvasMapRect.LongitudeDegrees,
                                paint);
            }
            else
            {
                canvas.DrawLine(canvasMapRect.Center.Latitude + canvasMapRect.LatitudeDegrees,
                                canvasMapRect.Center.Longitude - canvasMapRect.LongitudeDegrees,
                                canvasMapRect.Center.Latitude - canvasMapRect.LatitudeDegrees,
                                canvasMapRect.Center.Longitude + canvasMapRect.LongitudeDegrees,
                                paint);
            }

            paint.StrokeWidth = 5;
            paint.Color = Color.Blue.ToSKColor();

            canvas.DrawLine(baseBounds.Center.Latitude + baseBounds.LatitudeDegrees,
                            baseBounds.Center.Longitude + baseBounds.LongitudeDegrees,
                            baseBounds.Center.Latitude - baseBounds.LatitudeDegrees,
                            baseBounds.Center.Longitude - baseBounds.LongitudeDegrees,
                            paint);

            canvas.DrawLine(baseBounds.Center.Latitude - baseBounds.LatitudeDegrees,
                            baseBounds.Center.Longitude + baseBounds.LongitudeDegrees,
                            baseBounds.Center.Latitude + baseBounds.LatitudeDegrees,
                            baseBounds.Center.Longitude - baseBounds.LongitudeDegrees,
                            paint);

            paint.Color = Color.Red.ToSKColor();

            canvas.DrawLine(baseBounds.Center.Latitude + baseBounds.LatitudeDegrees,
                            baseBounds.Center.Longitude + baseBounds.LongitudeDegrees,
                            baseBounds.Center.Latitude - baseBounds.LatitudeDegrees,
                            baseBounds.Center.Longitude - baseBounds.LongitudeDegrees,
                            paint,
                            false);

            canvas.DrawLine(baseBounds.Center.Latitude - baseBounds.LatitudeDegrees,
                            baseBounds.Center.Longitude + baseBounds.LongitudeDegrees,
                            baseBounds.Center.Latitude + baseBounds.LatitudeDegrees,
                            baseBounds.Center.Longitude - baseBounds.LongitudeDegrees,
                            paint,
                            false);
            */
            paint.StrokeWidth = 5;
            paint.Color = Color.Orange.ToSKColor();

            canvas.DrawLine(GpsBounds.Center.Latitude + GpsBounds.LatitudeDegrees,
                            GpsBounds.Center.Longitude - GpsBounds.LongitudeDegrees,
                            GpsBounds.Center.Latitude - GpsBounds.LatitudeDegrees,
                            GpsBounds.Center.Longitude + GpsBounds.LongitudeDegrees,
                            paint,
                            false);

            canvas.DrawLine(GpsBounds.Center.Latitude + GpsBounds.LatitudeDegrees,
                            GpsBounds.Center.Longitude + GpsBounds.LongitudeDegrees,
                            GpsBounds.Center.Latitude - GpsBounds.LatitudeDegrees,
                            GpsBounds.Center.Longitude - GpsBounds.LongitudeDegrees,
                            paint,
                            false);
        }
    }
}
