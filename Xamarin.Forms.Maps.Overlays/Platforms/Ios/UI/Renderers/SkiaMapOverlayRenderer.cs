// **********************************************************************
// 
//   SkiaMapOverlayRenderer.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using CoreGraphics;
using MapKit;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Maps.Overlays.Platforms.Ios.Extensions;
using Xamarin.Forms.Maps.Overlays.Skia;
using Xamarin.Forms.Maps.Overlays.WeakSubscription;
using static Xamarin.Forms.Maps.Overlays.SKMapOverlay;

namespace Xamarin.Forms.Maps.Overlays.Platforms.Ios.UI.Renderers
{
    internal class SkiaMapOverlayRenderer : MKOverlayRenderer
    {
        private SkiaMapOverlay _NativeOverlay => Overlay as SkiaMapOverlay;
        private SKMapOverlay _SharedOverlay { get; }
        private MKMapView _NativeMap { get; }

        private IDisposable _overlayDirtySubscription;
        private Queue<SKBitmap> _overlayBitmapPool = new Queue<SKBitmap>();

        public SkiaMapOverlayRenderer(MKMapView mapView, SKMapOverlay sharedOverlay, IMKOverlay overlay) : base(overlay)
        {
            _SharedOverlay = sharedOverlay;
            _NativeMap = mapView;

            _overlayDirtySubscription = _SharedOverlay.WeakSubscribe<SKMapOverlay, MapOverlayInvalidateEventArgs>(nameof(_SharedOverlay.RequestInvalidate),
                                                                                                                        MarkOverlayDirty);
        }

        private void MarkOverlayDirty(object sender, MapOverlayInvalidateEventArgs args)
        {
            InvalidateSpan(args.GpsBounds);
        }

        private void InvalidateSpan(MapSpan area)
        {
            // TODO: Check if we need to do a full or partial refresh...
            if (_SharedOverlay.IsVisible)
            {
                _NativeMap.RemoveOverlay(_NativeOverlay);
                _NativeMap.AddOverlay(_NativeOverlay);
            }
        }

        public override void DrawMapRect(MKMapRect mapRect, nfloat zoomScale, CGContext context)
        {
            SKMapSpan rectSpan = mapRect.ToMapSpan();

            if (_SharedOverlay.IsVisible && rectSpan.FastIntersects(_SharedOverlay.GpsBounds))
            {
                CGRect coreDrawRect = RectForMapRect(mapRect);
                SKBitmap drawBitmap = GetOverlayBitmap();
                SKMapCanvas mapCanvas = new SKMapCanvas(drawBitmap, mapRect.ToRectangle(), zoomScale, true);

                _SharedOverlay.DrawOnMap(mapCanvas, rectSpan, zoomScale);

                Console.WriteLine($"Drawing tile for zoom scale {zoomScale} with GPS bounds {mapRect} and Mercator {mapRect.ToRectangle()}");

                context.DrawImage(coreDrawRect, drawBitmap.ToCGImage());

                // Let's exit this method so MapKit renders to screen while we free our resources in the background.
                Task.Run(() => ReleaseOverlayBitmap(drawBitmap));
            }
        }

        private SKBitmap GetOverlayBitmap()
        {
            SKBitmap overlayBitmap;

            lock (_overlayBitmapPool)
            {
                if (_overlayBitmapPool.Count == 0)
                {
                    int bitmapSize = SKMapCanvas.MapTileSize;

                    overlayBitmap = new SKBitmap(bitmapSize, bitmapSize, SKColorType.Rgba8888, SKAlphaType.Premul);
                    overlayBitmap.Erase(SKColor.Empty);
                }
                else
                {
                    overlayBitmap = _overlayBitmapPool.Dequeue();
                }
            }

            return overlayBitmap;
        }

        private void ReleaseOverlayBitmap(SKBitmap tileBitmap)
        {
            tileBitmap.Erase(SKColor.Empty);

            lock (_overlayBitmapPool)
            {
                _overlayBitmapPool.Enqueue(tileBitmap);
            }
        }
    }
}
