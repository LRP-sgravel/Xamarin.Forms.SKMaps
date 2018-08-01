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
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using MvvmCross.WeakSubscription;
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

            public TileOverlay TileOverlay { get; set; }
            public DrawableMapOverlay SharedOverlay { get; }

            private int _LastZoomLevel { get; set; } = -1;
            private Context _Context { get; }
            private GoogleMap _NativeMap { get; }
            private List<GroundOverlay> _GroundOverlays { get; } = new List<GroundOverlay>();

            private IDisposable _boundsChangedSubscription;
            private IDisposable _overlayDirtySubscription;
            private IDisposable _isVisibleChangedSubscription;
            private Queue<SKBitmap> _overlayBitmapPool = new Queue<SKBitmap>();

            public OverlayTrackerTileProvider(Context context, GoogleMap nativeMap, DrawableMapOverlay sharedOverlay)
            {
                _Context = context;
                _NativeMap = nativeMap;
                SharedOverlay = sharedOverlay;

                _boundsChangedSubscription = SharedOverlay.WeakSubscribe<DrawableMapOverlay>(nameof(SharedOverlay.GpsBounds),
                                                                                              OverlayGpsBoundsChanged);
                _overlayDirtySubscription = SharedOverlay.WeakSubscribe<DrawableMapOverlay, MapSpan>(nameof(SharedOverlay.RequestInvalidate),
                                                                                                      MarkOverlayDirty);
                _isVisibleChangedSubscription = SharedOverlay.WeakSubscribe<DrawableMapOverlay>(nameof(SharedOverlay.IsVisible),
                                                                                              OverlayVisibilityChanged);
            }

            public Tile GetTile(int x, int y, int zoom)
            {
                Device.BeginInvokeOnMainThread(() =>
                    {
                        if (SharedOverlay.IsVisible)
                        {
                            TileInfo tileInfo = new TileInfo { X = x, Y = y, Zoom = zoom };

                            if (_LastZoomLevel != zoom)
                            {
                                Console.WriteLine($"Clearing tiles for zoom level {_LastZoomLevel}");

                                _LastZoomLevel = zoom;
                                RefreshAllGroundOverlays();
                            }
                            else
                            {
                                int virtualTileSize = SKMapExtensions.MercatorMapSize >> zoom;
                                int xPixelsStart = x * virtualTileSize;
                                int yPixelsStart = y * virtualTileSize;
                                Rectangle mercatorSpan = new Rectangle(xPixelsStart, yPixelsStart, virtualTileSize, virtualTileSize);
                                SKMapSpan tileSpan = mercatorSpan.ToGps();

                                if (tileSpan.FastIntersects(SharedOverlay.GpsBounds))
                                {
                                    bool overlayExists;

                                    lock (_GroundOverlays)
                                    {
                                        overlayExists = _GroundOverlays.FirstOrDefault(o => (o.Tag as TileInfo)?.Equals(tileInfo) ?? false) != null;
                                    }

                                    Console.WriteLine($"Requesting tile at ({x}, {y}) for zoom level {zoom}");

                                    if (!overlayExists)
                                    {
                                        GroundOverlayOptions overlayOptions;
                                        SKBitmap bitmap = GetOverlayBitmap();
                                        double zoomScale = SKMapCanvas.MapTileSize / (double)virtualTileSize;
                                        SKMapCanvas mapCanvas = new SKMapCanvas(bitmap, mercatorSpan, zoomScale);

                                        SharedOverlay.DrawOnMap(mapCanvas, tileSpan, zoomScale);

                                        overlayOptions = new GroundOverlayOptions().PositionFromBounds(tileSpan.ToLatLng())
                                                                                    .InvokeImage(BitmapDescriptorFactory.FromBitmap(bitmap.ToBitmap()));

                                        GroundOverlay newOverlay = _NativeMap.AddGroundOverlay(overlayOptions);

                                        Console.WriteLine($"Adding ground tile at ({x}, {y}) for zoom level {zoom} ({zoomScale}) with GPS bounds {tileSpan} and Mercator {mercatorSpan}");

                                        newOverlay.Tag = tileInfo;

                                        lock (_GroundOverlays)
                                        {
                                            _GroundOverlays.Add(newOverlay);
                                        }

                                        // Let's exit this method so the main thread can render to screen while we free our resources in the background.
                                        Task.Run(() => ReleaseOverlayBitmap(bitmap));
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Ground tile at ({x}, {y}) for zoom level {zoom} already exists");
                                }
                            }
                        }
                    });

                return TileProvider.NoTile;
            }

            public void RemoveAllTiles()
            {
                List<GroundOverlay> oldOverlays;

                lock (_GroundOverlays)
                {
                    oldOverlays = new List<GroundOverlay>(_GroundOverlays);
                    _GroundOverlays.Clear();
                }

                foreach (GroundOverlay overlay in oldOverlays)
                {
                    overlay.Remove();
                }
            }

            private void RefreshAllGroundOverlays()
            {
                RemoveAllTiles();
                TileOverlay.ClearTileCache();
            }

            private void OverlayGpsBoundsChanged(object sender, PropertyChangedEventArgs args)
            {
                InvalidateSpan(SharedOverlay.GpsBounds);
            }

            private void OverlayVisibilityChanged(object sender, PropertyChangedEventArgs args)
            {
                InvalidateSpan(SharedOverlay.GpsBounds);
            }

            private void MarkOverlayDirty(object sender, MapSpan area)
            {
                InvalidateSpan(area);
            }

            private void InvalidateSpan(MapSpan area)
            {
                // TODO: Check if we need to do a full or partial refresh...
                RefreshAllGroundOverlays();
            }

            private SKBitmap GetOverlayBitmap()
            {
                SKBitmap overlayBitmap;

                lock (_overlayBitmapPool)
                {
                    if (_overlayBitmapPool.Count == 0)
                    {
                        int bitmapSize = (int)(SKMapCanvas.MapTileSize * Math.Min(2, _Context.Resources.DisplayMetrics.Density));

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

        private List<OverlayTrackerTileProvider> _TileTrackers { get; set; } = new List<OverlayTrackerTileProvider>();

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
                        foreach (DrawableMapOverlay mapOverlay in newItems)
                        {
                            AddTrackerForOverlay(mapOverlay);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (DrawableMapOverlay mapOverlay in removedItems)
                        {
                            OverlayTrackerTileProvider tracker = _TileTrackers.FirstOrDefault(t => t.SharedOverlay == mapOverlay);

                            if (tracker != null)
                            {
                                RemoveTracker(tracker);
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (DrawableMapOverlay mapOverlay in removedItems)
                        {
                            OverlayTrackerTileProvider tracker = _TileTrackers.FirstOrDefault(t => t.SharedOverlay == mapOverlay);

                            if (tracker != null)
                            {
                                RemoveTracker(tracker);
                            }
                        }

                        foreach (DrawableMapOverlay overlay in newItems)
                        {
                            AddTrackerForOverlay(overlay);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        List<OverlayTrackerTileProvider> oldTrackers = new List<OverlayTrackerTileProvider>(_TileTrackers);

                        foreach (OverlayTrackerTileProvider tracker in oldTrackers)
                        {
                            RemoveTracker(tracker);
                        }

                        foreach (DrawableMapOverlay overlay in _SharedControl.MapOverlays)
                        {
                            AddTrackerForOverlay(overlay);
                        }
                        break;
                    }
            }
        }

        private void AddTrackerForOverlay(DrawableMapOverlay sharedOverlay)
        {
            OverlayTrackerTileProvider tracker = new OverlayTrackerTileProvider(Context, NativeMap, sharedOverlay);
            TileOverlayOptions overlayOptions = new TileOverlayOptions().InvokeTileProvider(tracker);
            TileOverlay overlay = NativeMap.AddTileOverlay(overlayOptions);

            tracker.TileOverlay = overlay;
            _TileTrackers.Add(tracker);
        }

        private void RemoveTracker(OverlayTrackerTileProvider tracker)
        {
            TileOverlay overlay = tracker.TileOverlay;

            tracker.RemoveAllTiles();
            overlay.Remove();

            _TileTrackers.Remove(tracker);
        }
    }
}
