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
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using SkiaSharp;
using SkiaSharp.Views.Android;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Maps.Overlays;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Maps.Overlays.Platforms.Droid.Extensions;
using Xamarin.Forms.Maps.Overlays.Platforms.Droid.UI.Renderers;
using Xamarin.Forms.Maps.Overlays.Skia;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(OverlayedMap), typeof(OverlayedMapRenderer))]
namespace Xamarin.Forms.Maps.Overlays.Platforms.Droid.UI.Renderers
{
    internal class OverlayedMapRenderer : MapRenderer
    {
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
            private Context _Context { get; }
            private DrawableMapOverlay _SharedOverlay { get; }
            private GoogleMap _NativeMap { get; }
            private List<GroundOverlay> _GroundOverlays { get; } = new List<GroundOverlay>();
            public TileOverlay Overlay { get; internal set; }

            private Queue<SKBitmap> _overlayBitmapPool = new Queue<SKBitmap>();
            private object _cleanupLock = new object();

            public OverlayTrackerTileProvider(Context context, GoogleMap nativeMap, DrawableMapOverlay sharedOverlay)
            {
                _Context = context;
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

                            _LastZoomLevel = zoom;
                            Overlay.ClearTileCache();
                        }
                        else
                        {
                            Console.WriteLine($"Requesting tile at ({x}, {y}) for zoom level {zoom}");

                            bool overlayExists = _GroundOverlays.FirstOrDefault(o => (o.Tag as TileInfo)?.Equals(tileInfo) ?? false) != null;

                            if (!overlayExists)
                            {
                                int virtualTileSize = SKMapExtensions.MercatorMapSize >> zoom;
                                int xPixelsStart = x * virtualTileSize;
                                int yPixelsStart = y * virtualTileSize;
                                Rectangle mercatorSpan = new Rectangle(xPixelsStart, yPixelsStart, virtualTileSize, virtualTileSize);
                                SKMapSpan tileSpan = mercatorSpan.ToGps();

                                if (tileSpan.FastIntersects(_SharedOverlay.GpsBounds))
                                {
                                    GroundOverlayOptions overlayOptions;
                                    SKBitmap bitmap = GetOverlayBitmap();
                                    double zoomScale = SKMapCanvas.MapTileSize / (double)virtualTileSize;
                                    SKMapCanvas mapCanvas = new SKMapCanvas(bitmap, mercatorSpan, zoomScale);

                                    _SharedOverlay.DrawOnMap(mapCanvas, tileSpan, zoomScale);

                                    overlayOptions = new GroundOverlayOptions().PositionFromBounds(tileSpan.ToLatLng())
                                                                                .InvokeImage(BitmapDescriptorFactory.FromBitmap(bitmap.ToBitmap()));

                                    GroundOverlay newOverlay = _NativeMap.AddGroundOverlay(overlayOptions);

                                    Console.WriteLine($"Adding ground tile at ({x}, {y}) for zoom level {zoom} ({zoomScale}) with GPS bounds {tileSpan} and Mercator {mercatorSpan}");

                                    newOverlay.Tag = tileInfo;
                                    _GroundOverlays.Add(newOverlay);

                                    // Let's exit this method so the main thread can render to screen while we free our resources in the background.
                                    Task.Run(() => ReleaseOverlayBitmap(bitmap));
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Ground tile at ({x}, {y}) for zoom level {zoom} already exists");
                            }
                        }
                    });

                return TileProvider.NoTile;
            }

            private SKBitmap GetOverlayBitmap()
            {
                SKBitmap overlayBitmap;

                lock (_overlayBitmapPool)
                {
                    if (_overlayBitmapPool.Count == 0)
                    {
                        int bitmapSize = (int)(SKMapCanvas.MapTileSize * _Context.Resources.DisplayMetrics.Density);

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
                            OverlayTrackerTileProvider tracker = new OverlayTrackerTileProvider(Context, NativeMap, sharedOverlay);
                            TileOverlayOptions overlayOptions = new TileOverlayOptions().InvokeTileProvider(tracker);
                            TileOverlay overlay = NativeMap.AddTileOverlay(overlayOptions);

                            tracker.Overlay = overlay;
                            _TileTrackers.Add(overlay);
                        }
                        break;
                    }
            }
        }
    }
}
