// **********************************************************************
// 
//   ImageMapOverlay.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using LRPLib.Views.XForms.Extensions;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Controls.Maps
{
    public class ImageMapOverlay : DrawableMapOverlay
    {
        public ImageMapOverlay()
        {
            GpsBounds = new MapSpan(new Position(-0.5, 0.5), 0.5, 0.5);
        }

        public override void DrawOnMap(SKCanvas canvas)
        {
            SKPaint paint = new SKPaint();

            paint.IsAntialias = true;
            paint.StrokeWidth = 10;
            paint.Color = Color.Aqua.ToSKColor();

            canvas.DrawOval((float)GpsBounds.Center.Longitude,
                            (float)GpsBounds.Center.Latitude,
                            (float)GpsBounds.LongitudeDegrees,
                            (float)GpsBounds.LatitudeDegrees,
                            paint);
        }
    }
}
