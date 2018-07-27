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
using Android.Graphics;
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
    internal class OverlayedMapRenderer : MapRenderer
    {
        private GroundOverlay newOverlay;

        private class OverlayTrackerTileProvider : Java.Lang.Object, ITileProvider
        {
            private class TileInfo : Java.Lang.Object
            {
                public int X { get; set; }
                public int Y { get; set; }
                public int Zoom { get; set; }

                public override bool Equals(Java.Lang.Object obj)
                {
                    if (obj is TileInfo)
                    {
                        TileInfo tile = obj as TileInfo;

                        return tile.X == X && tile.Y == Y && tile.Zoom == Zoom;
                    }

                    return false;
                }
            }

            private int _LastZoomLevel { get; set; } = -1;
            private DrawableMapOverlay _SharedOverlay { get; }
            private GoogleMap _NativeMap { get; }
            private List<GroundOverlay> _GroundOverlays { get; } = new List<GroundOverlay>();

            private object _cleanupLock = new object();

            public OverlayTrackerTileProvider(GoogleMap nativeMap, DrawableMapOverlay sharedOverlay)
            {
                _NativeMap = nativeMap;
                _SharedOverlay = sharedOverlay;
            }

            public Tile GetTile(int x, int y, int zoom)
            {
                Device.BeginInvokeOnMainThread(() =>
                    {
                        TileInfo tileInfo = new TileInfo { X = x, Y = y, Zoom = zoom };

                        if (_LastZoomLevel != zoom)
                        {
                            List<GroundOverlay> oldOverlays = new List<GroundOverlay>(_GroundOverlays);

                            Console.WriteLine($"Clearing tiles for zoom level {_LastZoomLevel}");

                            _GroundOverlays.Clear();
                            foreach (GroundOverlay overlay in oldOverlays)
                            {
                                overlay.Remove();
                            }
                        }
                        _LastZoomLevel = zoom;

                        Console.WriteLine($"Requesting tile at ({x}, {y}) for zoom level {zoom}");

                        bool overlayExists = _GroundOverlays.FirstOrDefault(o => (o.Tag as TileInfo)?.Equals(tileInfo) ?? false) != null;

                        if (!overlayExists)
                        {
                            int tileSize = SKMapExtensions.MercatorMapSize >> zoom;
                            int xPixelsStart = x * tileSize;
                            int yPixelsStart = y * tileSize;
                            SKMapSpan tileSpan = new Rectangle(xPixelsStart, yPixelsStart, tileSize, tileSize).ToGps();

                            //if(tileSpan.FastIntersects(_SharedOverlay.GpsBounds))
                            {
                                LatLngBounds nativeBounds = tileSpan.ToLatLng();
                                GroundOverlayOptions overlayOptions = new GroundOverlayOptions().PositionFromBounds(nativeBounds)
                                                                                                .InvokeImage(BitmapDescriptorFactory.DefaultMarker());
                                GroundOverlay newOverlay = _NativeMap.AddGroundOverlay(overlayOptions);

                                Console.WriteLine($"Adding ground tile at ({x}, {y}) for zoom level {zoom} with GPS bounds {tileSpan} (Native = {nativeBounds})");

                                newOverlay.Tag = tileInfo;
                                _GroundOverlays.Add(newOverlay);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Ground tile at ({x}, {y}) for zoom level {zoom} already exists");
                        }
                    });

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

/*            const int tileSize = 256;
            LatLngBounds nativeBounds = new LatLngBounds(new LatLng(-40, -40), new LatLng(40, 40));
            GroundOverlayOptions overlayOptions;

            using (Bitmap tileBitmap = Bitmap.CreateBitmap(tileSize, tileSize, Bitmap.Config.Argb8888))
            using (Canvas bitmapCanvas = new Canvas(tileBitmap))
            using (Paint paint = new Paint())
            {
                paint.Color = Color.Red.ToAndroid();

                bitmapCanvas.DrawCircle(tileSize * 0.5f, tileSize * 0.5f, tileSize * 0.5f, paint);

                overlayOptions = new GroundOverlayOptions().PositionFromBounds(nativeBounds)
                                                           .InvokeTransparency(0)
                                                           .InvokeZIndex(100)
                                                           .InvokeImage(BitmapDescriptorFactory.FromBitmap(tileBitmap));
            }

            this.newOverlay = NativeMap.AddGroundOverlay(overlayOptions);*/

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
