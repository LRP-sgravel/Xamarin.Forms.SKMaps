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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MapKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Maps.Overlays;
using Xamarin.Forms.Maps.Overlays.Platforms.Ios.UI.Renderers;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(OverlayedMap), typeof(OverlayedMapRenderer))]
namespace Xamarin.Forms.Maps.Overlays.Platforms.Ios.UI.Renderers
{
    class OverlayedMapRenderer : MapRenderer
    {
        private static class OverlayedMapDelegate
        {
            public static MKOverlayRenderer OverlayRenderer(MKMapView mapView, IMKOverlay overlay)
            {
                if (overlay is SkiaMapOverlay)
                {
                    return new SkiaMapOverlayRenderer(mapView, (overlay as SkiaMapOverlay).SharedOverlay, overlay);
                }

                return null;
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
                _NativeControl.OverlayRenderer = OverlayedMapDelegate.OverlayRenderer;

                if (_SharedControl?.MapOverlays != null)
                {
                    _SharedControl.MapOverlays.CollectionChanged += MapOverlaysCollectionChanged;
                    SetupMapOverlays();
                }
            }
        }

        private void MapOverlaysCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            IEnumerable<DrawableMapOverlay> newItems = args.NewItems?
                                                           .Cast<DrawableMapOverlay>()
                                                           .DefaultIfEmpty();
            IEnumerable<DrawableMapOverlay> removedItems = args.OldItems?
                                                               .Cast<DrawableMapOverlay>()
                                                               .DefaultIfEmpty();
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    _NativeControl.AddOverlays(newItems.Select(o => (IMKOverlay)new SkiaMapOverlay(o, _SharedControl))
                                                       .ToArray());
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    _NativeControl.RemoveOverlays(_NativeControl.Overlays
                                                                .Where(o => removedItems.Contains((o as SkiaMapOverlay).SharedOverlay))
                                                                .ToArray());
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    _NativeControl.RemoveOverlays(_NativeControl.Overlays
                                                                .Where(o => removedItems.Contains((o as SkiaMapOverlay).SharedOverlay))
                                                                .ToArray());
                    _NativeControl.AddOverlays(newItems.Select(o => (IMKOverlay)new SkiaMapOverlay(o, _SharedControl))
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
                                                             .Select(o => (IMKOverlay)new SkiaMapOverlay(o, _SharedControl))
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
