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

using System;
using System.IO;
using System.Reflection;
using LRPLib.Mvx.Views.XForms.Views;
using LRPLib.Services.Resources;
using LRPLib.Views.XForms.Extensions;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Controls.Maps
{
    public class ImageMapOverlay : DrawableMapOverlay
    {
        public ImageMapOverlay()
        {
            GpsBounds = new MapSpan(new Position(0, 0), 1, 1);
        }

        public override void DrawOnMap(SKMapCanvas canvas, MapSpan canvasMapRect)
        {
            SKPaint paint = new SKPaint();

            paint.IsAntialias = true;
            paint.StrokeWidth = 1;
            paint.Color = Color.Fuchsia.ToSKColor();

            if (canvasMapRect.Center.Latitude > 0)
            {
                canvas.DrawLine(new Position(canvasMapRect.Center.Latitude + canvasMapRect.LatitudeDegrees, canvasMapRect.Center.Longitude),
                                new Position(canvasMapRect.Center.Latitude - canvasMapRect.LatitudeDegrees,
                                             canvasMapRect.Center.Longitude + canvasMapRect.LongitudeDegrees),
                                paint);
                canvas.DrawLine(new Position(canvasMapRect.Center.Latitude - canvasMapRect.LatitudeDegrees,
                                             canvasMapRect.Center.Longitude + canvasMapRect.LongitudeDegrees),
                                new Position(canvasMapRect.Center.Latitude - canvasMapRect.LatitudeDegrees,
                                             canvasMapRect.Center.Longitude - canvasMapRect.LongitudeDegrees),
                                paint);
                canvas.DrawLine(new Position(canvasMapRect.Center.Latitude - canvasMapRect.LatitudeDegrees,
                                             canvasMapRect.Center.Longitude - canvasMapRect.LongitudeDegrees),
                                new Position(canvasMapRect.Center.Latitude + canvasMapRect.LatitudeDegrees, canvasMapRect.Center.Longitude),
                                paint);
            }
            else
            {
                canvas.DrawLine(new Position(canvasMapRect.Center.Latitude - canvasMapRect.LatitudeDegrees, canvasMapRect.Center.Longitude),
                                new Position(canvasMapRect.Center.Latitude + canvasMapRect.LatitudeDegrees,
                                             canvasMapRect.Center.Longitude + canvasMapRect.LongitudeDegrees),
                                paint);
                canvas.DrawLine(new Position(canvasMapRect.Center.Latitude + canvasMapRect.LatitudeDegrees,
                                             canvasMapRect.Center.Longitude + canvasMapRect.LongitudeDegrees),
                                new Position(canvasMapRect.Center.Latitude + canvasMapRect.LatitudeDegrees,
                                             canvasMapRect.Center.Longitude - canvasMapRect.LongitudeDegrees),
                                paint);
                canvas.DrawLine(new Position(canvasMapRect.Center.Latitude + canvasMapRect.LatitudeDegrees,
                                             canvasMapRect.Center.Longitude - canvasMapRect.LongitudeDegrees),
                                new Position(canvasMapRect.Center.Latitude - canvasMapRect.LatitudeDegrees, canvasMapRect.Center.Longitude),
                                paint);
            }

            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 10;
            paint.StrokeCap = SKStrokeCap.Round;
            paint.StrokeJoin = SKStrokeJoin.Round;
            paint.Color = Color.Red.ToSKColor();
            SKMapPath zonePath = canvas.GetMapPath();

            zonePath.MoveTo((float)(GpsBounds.Center.Latitude - GpsBounds.LatitudeDegrees), (float)GpsBounds.Center.Longitude);
            zonePath.LineTo((float)(GpsBounds.Center.Latitude + GpsBounds.LatitudeDegrees), (float)(GpsBounds.Center.Longitude + GpsBounds.LongitudeDegrees));
            zonePath.LineTo((float)(GpsBounds.Center.Latitude + GpsBounds.LatitudeDegrees), (float)(GpsBounds.Center.Longitude - GpsBounds.LongitudeDegrees));
            zonePath.Close();

            canvas.DrawPath(zonePath, paint);

            IResourceLocator resLocator = Mvx.Resolve<IResourceLocator>();
            string resPath = resLocator.GetResourcePath(ResourceKeys.ImagesKey, "symbol_logo.svg");
            SKSvg logoSvg = new SKSvg();
            logoSvg.Load(resLocator.ResourcesAssembly.GetManifestResourceStream(resPath));
            canvas.DrawPicture(logoSvg.Picture, GpsBounds.Center, new SKSize(50, 50));

            canvas.DrawPicture(logoSvg.Picture);
        }
    }
}
