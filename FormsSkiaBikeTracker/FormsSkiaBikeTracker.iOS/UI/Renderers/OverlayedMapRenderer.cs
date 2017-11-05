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
using CoreGraphics;
using CoreLocation;
using FormsSkiaBikeTracker.Forms.UI.Controls;
using FormsSkiaBikeTracker.Forms.UI.Controls.Maps;
using FormsSkiaBikeTracker.iOS.Helpers;
using FormsSkiaBikeTracker.iOS.UI.Renderers;
using MapKit;
using MvvmCross.Platform.Platform;
using MvvmCross.Platform.WeakSubscription;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Xaml;

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
                    return new MapOverlayRenderer((overlay as MapOverlay).SharedOverlay, overlay);
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
            private MKMapRect _OverlayMapRect { get; set; }

            private MvxNamedNotifyPropertyChangedEventSubscription<DrawableMapOverlay> _boundsChangedSubscription;
            private float _screenScale;

            public MapOverlayRenderer(DrawableMapOverlay sharedOverlay, IMKOverlay overlay) : base(overlay)
            {
                _SharedOverlay = sharedOverlay;
                _screenScale = (float)UIScreen.MainScreen.NativeScale;

                _boundsChangedSubscription = _SharedOverlay.WeakSubscribe<DrawableMapOverlay>(nameof(_SharedOverlay.GpsBounds), OverlayGpsBoundsChanged);
                UpdateBoundsAndRefresh();
            }

            private void OverlayGpsBoundsChanged(object sender, PropertyChangedEventArgs e)
            {
                UpdateBoundsAndRefresh();
            }

            private void UpdateBoundsAndRefresh()
            {
                _OverlayMapRect = _NativeOverlay.BoundingMapRect;
                SetNeedsDisplay(_OverlayMapRect);
            }

            public override void DrawMapRect(MKMapRect mapRect, nfloat zoomScale, CGContext context)
            {
                CGRect coreDrawRect = RectForMapRect(mapRect);
                CGRect coreOverlayRect = RectForMapRect(_OverlayMapRect);
                SKBitmap overlayBitmap = new SKBitmap((int)Math.Ceiling(coreDrawRect.Width * zoomScale * _screenScale),
                                                      (int)Math.Ceiling(coreDrawRect.Height * zoomScale * _screenScale),
                                                      SKColorType.Rgba8888,
                                                      SKAlphaType.Premul);
                SKCanvas mapCanvas = new SKCanvas(overlayBitmap);

                MvxTrace.Trace(MvxTraceLevel.Diagnostic, $"Drawing tile at ({coreDrawRect.Left}, {coreDrawRect.Top}; {coreDrawRect.Width}, {coreDrawRect.Height}) with zoom {zoomScale}");
                MvxTrace.Trace(MvxTraceLevel.Diagnostic, $"Drawing overlay at ({coreOverlayRect.Left}, {coreOverlayRect.Top}; {coreOverlayRect.Width}, {coreOverlayRect.Height})");

                mapCanvas.Clear();
                _SharedOverlay.DrawOnMap(mapCanvas);

                context.SetStrokeColor(0, 1, 0, 1);
                context.SetLineWidth(context.GetCTM().Invert().TransformSize(new CGSize(2, 2)).Width);
                context.StrokeRect(coreDrawRect);

                context.SetFillColor(0.5f, 0.5f);
                context.FillRect(coreOverlayRect);

                context.DrawImage(coreDrawRect, overlayBitmap.ToCGImage());

                CGPoint originPoint = PointForMapPoint(MKMapPoint.FromCoordinate(new CLLocationCoordinate2D(0, 0)));
                CGPoint endPoint = PointForMapPoint(MKMapPoint.FromCoordinate(new CLLocationCoordinate2D(-1, 1)));
                CGSize pixelsSize = context.GetCTM().Invert().TransformSize(new CGSize(5, -5));
                CGRect origin = new CGRect(originPoint, pixelsSize);
                CGRect end = new CGRect(endPoint, pixelsSize);

                context.SaveState();
                context.TranslateCTM(pixelsSize.Width * -0.5f, pixelsSize.Height * -0.5f);
                context.SetFillColor(0.5f, 0, 0.5f, 1);
                context.FillEllipseInRect(origin);
                context.SetFillColor(0, 0.5f, 0.5f, 1);
                context.FillEllipseInRect(end);
                context.RestoreState();
            }

            public override bool CanDrawMapRect(MKMapRect mapRect, nfloat zoomScale)
            {
                Rectangle rect1 = new Rectangle(mapRect.MinX, mapRect.MinY, mapRect.Width, mapRect.Height);
                Rectangle rect2 = new Rectangle(_OverlayMapRect.MinX, _OverlayMapRect.MinY, _OverlayMapRect.Width, _OverlayMapRect.Height);

                return rect1.IntersectsWith(rect2);
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

            if(_NativeControl != null)
            {
                _NativeControl.RegionWillChange -= MapRegionWillChange;
            }

            base.OnElementChanged(e);

            if (_NativeControl != null)
            {
                try
                {
                    _NativeControl.OverlayRenderer = OverlayedMapDelegate.OverlayRenderer;
                    _NativeControl.RegionWillChange += MapRegionWillChange;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }

                if (_SharedControl?.MapOverlays != null)
                {
                    SetupMapOverlays();

                    _SharedControl.MapOverlays.CollectionChanged += MapOverlaysCollectionChanged;
                }
            }
        }

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

//                        renderer?.SetNeedsDisplay();
                    }
                }
            }
        }

        private void SetupMapOverlays()
        {
            MapOverlaysCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
