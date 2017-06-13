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
using FormsSkiaBikeTracker.iOS.Helpers;
using FormsSkiaBikeTracker.iOS.UI.Renderers;
using MapKit;
using Xamarin.Forms;
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
                    return new MapOverlayRenderer();
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
            public override void DrawMapRect(MKMapRect drawRect, nfloat zoomScale, CGContext context)
            {
                MKMapRect mapRect = Overlay.BoundingMapRect;
                CGRect coreRect = RectForMapRect(mapRect);

                context.ScaleCTM(1.0f, -1.0f);
                context.TranslateCTM(0.0f, -coreRect.Size.Height);
//                CGContextDrawImage(context, coreRect, imageReference);

                base.DrawMapRect(mapRect, zoomScale, context);
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

            base.OnElementChanged(e);

            if (_NativeControl != null)
            {
                try
                {
                    _NativeControl.OverlayRenderer = OverlayedMapDelegate.OverlayRenderer;
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

        private void SetupMapOverlays()
        {
            MapOverlaysCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
