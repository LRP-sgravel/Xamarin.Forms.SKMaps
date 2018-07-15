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
using System.Linq;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Maps.Overlays;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Maps.Overlays.Platforms.Droid.Extensions;
using Xamarin.Forms.Maps.Overlays.Platforms.Droid.UI.Renderers;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(OverlayedMap), typeof(OverlayedMapRenderer))]
namespace Xamarin.Forms.Maps.Overlays.Platforms.Droid.UI.Renderers
{
    class OverlayedMapRenderer : MapRenderer
    {
        private class OverlayTrackerTileProvider : Java.Lang.Object, ITileProvider
        {
            private DrawableMapOverlay _SharedOverlay { get; }
            private GoogleMap _NativeMap { get; }
            private List<GroundOverlay> _GroundOverlays { get; } = new List<GroundOverlay>();

            public OverlayTrackerTileProvider(GoogleMap nativeMap, DrawableMapOverlay sharedOverlay)
            {
                _NativeMap = nativeMap;
                _SharedOverlay = sharedOverlay;
            }

            public Tile GetTile(int x, int y, int zoom)
            {
                Console.WriteLine($"Requesting tile at ({x}, {y}) for zoom level {zoom}");

                if (zoom == 2)
                {
                    int tileSize = SKMapExtensions.MercatorMapSize >> zoom;
                    int xPixelsStart = x * tileSize;
                    int yPixelsStart = SKMapExtensions.MercatorMapSize - (y + 1) * tileSize;
                    SKMapSpan tileSpan = new Rectangle(xPixelsStart, yPixelsStart, tileSize, tileSize).ToGps();

                    if (tileSpan.FastIntersects(_SharedOverlay.GpsBounds))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                            {
                                GroundOverlayOptions overlayOptions = new GroundOverlayOptions().PositionFromBounds(tileSpan.ToLatLng())
                                                                                                .InvokeImage(BitmapDescriptorFactory.DefaultMarker());
                                GroundOverlay newOverlay = _NativeMap.AddGroundOverlay(overlayOptions);

                                _GroundOverlays.Add(newOverlay);
                            });
                    }
                }

                return TileProvider.NoTile;
            }
        }

        private OverlayedMap _SharedControl => Element as OverlayedMap;
        private MapView _NativeControl => Control as MapView;

        private List<TileOverlay> _TileTrackers { get; set; } = new List<TileOverlay>();

        public OverlayedMapRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            if (_SharedControl?.MapOverlays != null)
            {
                _SharedControl.MapOverlays.CollectionChanged -= MapOverlaysCollectionChanged;
            }

            base.OnElementChanged(e);

            if (_NativeControl != null)
            {
                if (_SharedControl?.MapOverlays != null)
                {
                    _SharedControl.MapOverlays.CollectionChanged += MapOverlaysCollectionChanged;
                }
            }
        }

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);

            SetupMapOverlays();
        }

        private void SetupMapOverlays()
        {
            MapOverlaysCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach(TileOverlay overlay in _TileTrackers)
                        {
                            overlay.Remove();
                        }
                        _TileTrackers.Clear();

                        foreach (DrawableMapOverlay sharedOverlay in _SharedControl.MapOverlays)
                        {
                            OverlayTrackerTileProvider tracker = new OverlayTrackerTileProvider(NativeMap, sharedOverlay);
                            TileOverlayOptions overlayOptions = new TileOverlayOptions().InvokeTileProvider(tracker);
                            TileOverlay overlay = NativeMap.AddTileOverlay(overlayOptions);

                            _TileTrackers.Add(overlay);
                        }
                        break;
                    }
            }
        }
    }
}
