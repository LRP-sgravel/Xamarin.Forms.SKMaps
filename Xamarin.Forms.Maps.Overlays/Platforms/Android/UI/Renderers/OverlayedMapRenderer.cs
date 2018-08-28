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
using Xamarin.Forms.Maps.Overlays.WeakSubscription;
using Xamarin.Forms.Platform.Android;
using static Xamarin.Forms.Maps.Overlays.DrawableMapOverlay;

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
                public bool NeedsRedraw { get; set; } = true;

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

            private IDisposable _overlayDirtySubscription;
            private Queue<SKBitmap> _overlayBitmapPool = new Queue<SKBitmap>();

            public OverlayTrackerTileProvider(Context context, GoogleMap nativeMap, DrawableMapOverlay sharedOverlay)
            {
                _Context = context;
                _NativeMap = nativeMap;
                SharedOverlay = sharedOverlay;

                _overlayDirtySubscription = SharedOverlay.WeakSubscribe<DrawableMapOverlay, MapOverlayInvalidateEventArgs>(nameof(SharedOverlay.RequestInvalidate),
                                                                                                     MarkOverlayDirty);
            }

            public Tile GetTile(int x, int y, int zoom)
            {
                TileInfo tileInfo = new TileInfo { X = x, Y = y, Zoom = zoom };

                if (_LastZoomLevel != zoom)
                {
                    Console.WriteLine($"Clearing tiles for zoom level {_LastZoomLevel}");

                    _LastZoomLevel = zoom;
                    ResetAllTiles();
                }
                else
                {
                    Task.Run(() => HandleGroundOverlayForTile(tileInfo));
                }

                return TileProvider.NoTile;
            }

            private async Task HandleGroundOverlayForTile(TileInfo tileInfo)
            {
                if (SharedOverlay.IsVisible)
                {
                    int virtualTileSize = SKMapExtensions.MercatorMapSize >> tileInfo.Zoom;
                    int xPixelsStart = tileInfo.X * virtualTileSize;
                    int yPixelsStart = tileInfo.Y * virtualTileSize;
                    double zoomScale = SKMapCanvas.MapTileSize / (double)virtualTileSize;
                    Rectangle mercatorSpan = new Rectangle(xPixelsStart, yPixelsStart, virtualTileSize, virtualTileSize);
                    SKMapSpan tileSpan = mercatorSpan.ToGps();

                    if (tileSpan.FastIntersects(SharedOverlay.GpsBounds))
                    {
                        SKBitmap bitmap = DrawTileToBitmap(tileSpan, zoomScale);
                        BitmapDescriptor bitmapDescriptor = BitmapDescriptorFactory.FromBitmap(bitmap.ToBitmap());
                        TaskCompletionSource<object> drawingTcs = new TaskCompletionSource<object>();

                        Console.WriteLine($"Refreshing ground tile at ({tileInfo.X}, {tileInfo.Y}) for zoom level {tileInfo.Zoom} ({zoomScale}) with GPS bounds {tileSpan}");

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            GroundOverlay overlay;

                            lock (_GroundOverlays)
                            {
                                overlay = _GroundOverlays.FirstOrDefault(o => (o.Tag as TileInfo)?.Equals(tileInfo) ?? false);
                            }

                            if (overlay == null)
                            {
                                GroundOverlayOptions overlayOptions = new GroundOverlayOptions().PositionFromBounds(tileSpan.ToLatLng())
                                                                                                .InvokeImage(bitmapDescriptor);

                                overlay = _NativeMap.AddGroundOverlay(overlayOptions);
                                overlay.Tag = tileInfo;

                                lock (_GroundOverlays)
                                {
                                    _GroundOverlays.Add(overlay);
                                }
                            }
                            else if ((overlay.Tag as TileInfo)?.NeedsRedraw ?? false)
                            {
                                overlay.SetImage(bitmapDescriptor);
                            }

                            drawingTcs.TrySetResult(null);
                        });

                        await drawingTcs.Task.ConfigureAwait(false);
                        ReleaseOverlayBitmap(bitmap);
                    }
                    else
                    {
                        Console.WriteLine($"Ground tile at ({tileInfo.X}, {tileInfo.Y}) for zoom level {tileInfo.Zoom} already exists");
                    }
                }
            }

            private SKBitmap DrawTileToBitmap(SKMapSpan tileSpan, double zoomScale)
            {
                Rectangle mercatorSpan = tileSpan.ToMercator();
                SKBitmap bitmap = GetOverlayBitmap();
                SKMapCanvas mapCanvas = new SKMapCanvas(bitmap, mercatorSpan, zoomScale);

                SharedOverlay.DrawOnMap(mapCanvas, tileSpan, zoomScale);
                return bitmap;
            }

            public void RemoveAllTiles()
            {
                List<GroundOverlay> oldOverlays;

                lock (_GroundOverlays)
                {
                    oldOverlays = new List<GroundOverlay>(_GroundOverlays);
                    _GroundOverlays.Clear();
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (GroundOverlay overlay in oldOverlays)
                    {
                        overlay.Remove();
                    }
                });
            }

            private void MarkAllTilesDirty()
            {
                List<GroundOverlay> overlays;

                lock (_GroundOverlays)
                {
                    overlays = new List<GroundOverlay>(_GroundOverlays);
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (GroundOverlay overlay in overlays)
                    {
                        (overlay.Tag as TileInfo).NeedsRedraw = true;
                    }
                });
            }

            private void ResetAllTiles()
            {
                RemoveAllTiles();
                Device.BeginInvokeOnMainThread(() =>
                {
                    TileOverlay.ClearTileCache();
                });
            }

            private void RefreshAllTiles()
            {
                MarkAllTilesDirty();
                Device.BeginInvokeOnMainThread(() =>
                {
                    TileOverlay.ClearTileCache();
                });
            }

            private void MarkOverlayDirty(object sender, MapOverlayInvalidateEventArgs args)
            {
                InvalidateSpan(args.GpsBounds);
            }

            private void InvalidateSpan(MapSpan area)
            {
                // TODO: Check if we need to do a full or partial refresh...
                RefreshAllTiles();
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
