// **********************************************************************
// 
//   OverlayedMapRenderer.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CoreFoundation;
using CoreGraphics;
using CoreLocation;
using FormsSkiaBikeTracker.Forms.UI.Controls;
using FormsSkiaBikeTracker.Forms.UI.Controls.Maps;
using FormsSkiaBikeTracker.iOS.Helpers;
using FormsSkiaBikeTracker.iOS.UI.Renderers;
using FormsSkiaBikeTracker.Shared.Models.Maps;
using MapKit;
using MvvmCross.Platform.WeakSubscription;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(OverlayedMap), typeof(OverlayedMapRenderer))]
namespace FormsSkiaBikeTracker.iOS.UI.Renderers
{
    class OverlayedMapRenderer : MapRenderer
    {
        private static class OverlayedMapDelegate
        {
            public static MKOverlayRenderer OverlayRenderer(MKMapView mapView, IMKOverlay overlay)
            {
                if (overlay is MapOverlay)
                {
                    return new MapOverlayRenderer(mapView, (overlay as MapOverlay).SharedOverlay, overlay);
                }

                return null;
            }
        }

        private class MapOverlay : MKOverlay
        {
            public override CLLocationCoordinate2D Coordinate => SharedOverlay.GpsBounds.Center.ToLocationCoordinate();
            public override MKMapRect BoundingMapRect => SharedOverlay.GpsBounds.ToMapRect();

            public DrawableMapOverlay SharedOverlay { get; }
            private OverlayedMap _SharedControl { get; }

            public MapOverlay(DrawableMapOverlay sharedOverlay, OverlayedMap sharedControl)
            {
                SharedOverlay = sharedOverlay;
                _SharedControl = sharedControl;
            }
        }

        private class MapOverlayRenderer : MKOverlayRenderer
        {
            private MapOverlay _NativeOverlay => Overlay as MapOverlay;
            private DrawableMapOverlay _SharedOverlay { get; }
            private MKMapView _NativeMap { get; }

            private object _boundsChangedSubscription;
            private object _overlayDirtySubscription;
            private Queue<SKBitmap> _overlayBitmapPool = new Queue<SKBitmap>();

            public MapOverlayRenderer(MKMapView mapView, DrawableMapOverlay sharedOverlay, IMKOverlay overlay) : base(overlay)
            {
                _SharedOverlay = sharedOverlay;
                _NativeMap = mapView;

                _boundsChangedSubscription = _SharedOverlay.WeakSubscribe<DrawableMapOverlay>(nameof(_SharedOverlay.GpsBounds), OverlayGpsBoundsChanged);
                _overlayDirtySubscription = new MvxWeakEventSubscription<DrawableMapOverlay, MapSpan>(_SharedOverlay, nameof(_SharedOverlay.RequestInvalidate), MarkOverlayDirty);
            }

            private void OverlayGpsBoundsChanged(object sender, PropertyChangedEventArgs args)
            {
                InvalidateSpan(_SharedOverlay.GpsBounds);
            }

            private void MarkOverlayDirty(object sender, MapSpan area)
            {
                InvalidateSpan(area);
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
                CGRect coreDrawRect = RectForMapRect(mapRect);
                SKBitmap drawBitmap = GetOverlayBitmap();
                SKMapCanvas mapCanvas = new SKMapCanvas(drawBitmap, mapRect.ToRectangle(), zoomScale);
                SKMapSpan rectSpan = mapRect.ToMapSpan();

                if (rectSpan.FastIntersects(_SharedOverlay.GpsBounds))
                {
                    _SharedOverlay.DrawOnMap(mapCanvas, rectSpan, zoomScale);

                    context.SaveState();
                    context.DrawImage(coreDrawRect, drawBitmap.ToCGImage());
                    context.RestoreState();
                }

                // Let's exit this method so MapKit renders to screen while we free our resources in the background.
                Task.Run(() => ReleaseOverlayBitmap(drawBitmap));
            }

            private SKBitmap GetOverlayBitmap()
            {
                SKBitmap overlayBitmap;

                lock (_overlayBitmapPool)
                {
                    if (_overlayBitmapPool.Count == 0)
                    {
                        overlayBitmap = new SKBitmap(SKMapCanvas.MapTileSize, SKMapCanvas.MapTileSize, SKColorType.Rgba8888, SKAlphaType.Premul);
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

        private OverlayedMap _SharedControl => Element as OverlayedMap;
        private MKMapView _NativeControl => Control as MKMapView;

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (_SharedControl?.MapOverlays != null)
            {
                _SharedControl.MapOverlays.CollectionChanged -= MapOverlaysCollectionChanged;
            }

#if DEBUG_MAP
            if (_NativeControl != null)
            {
                _NativeControl.RegionWillChange -= MapRegionWillChange;
            }
#endif

            base.OnElementChanged(e);

            if (_NativeControl != null)
            {
                _NativeControl.OverlayRenderer = OverlayedMapDelegate.OverlayRenderer;
#if DEBUG_MAP
                _NativeControl.RegionWillChange += MapRegionWillChange;
#endif

                if (_SharedControl?.MapOverlays != null)
                {
                    _SharedControl.MapOverlays.CollectionChanged += MapOverlaysCollectionChanged;
                    SetupMapOverlays();
                }
            }
        }

#if DEBUG_MAP
        private void MapRegionWillChange(object sender, MKMapViewChangeEventArgs args)
        {
            if (_SharedControl?.MapOverlays != null)
            {
                foreach (DrawableMapOverlay overlay in _SharedControl?.MapOverlays)
                {
                    MapOverlay nativeOverlay = _NativeControl.Overlays.FirstOrDefault(o => (o as MapOverlay).SharedOverlay == overlay) as MapOverlay;

                    if (nativeOverlay != null)
                    {
                        MKOverlayRenderer renderer = _NativeControl.RendererForOverlay(nativeOverlay);

                        renderer?.SetNeedsDisplay();
                    }
                }
            }
        }
#endif

        private void MapOverlaysCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            IList<DrawableMapOverlay> newItems = args.NewItems as IList<DrawableMapOverlay>;
            IList<DrawableMapOverlay> removedItems = args.OldItems as IList<DrawableMapOverlay>;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    _NativeControl.AddOverlays(newItems.Select(o => (IMKOverlay)new MapOverlay(o, _SharedControl))
                                                       .ToArray());
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    _NativeControl.RemoveOverlays(_NativeControl.Overlays
                                                                .Where(o => removedItems.Contains((o as MapOverlay).SharedOverlay))
                                                                .ToArray());
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    _NativeControl.RemoveOverlays(_NativeControl.Overlays
                                                                .Where(o => removedItems.Contains((o as MapOverlay).SharedOverlay))
                                                                .ToArray());
                    _NativeControl.AddOverlays(newItems.Select(o => (IMKOverlay)new MapOverlay(o, _SharedControl))
                                                       .ToArray());
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    if (_NativeControl.Overlays != null)
                    {
                        _NativeControl.RemoveOverlays(_NativeControl.Overlays);
                    }

                    _NativeControl.AddOverlays(_SharedControl.MapOverlays
                                                             .Select(o => (IMKOverlay)new MapOverlay(o, _SharedControl))
                                                             .ToArray());
                    break;
                }
            }
        }
        
        private void SetupMapOverlays()
        {
            MapOverlaysCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
