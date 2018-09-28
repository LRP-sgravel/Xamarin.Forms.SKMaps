using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using SkiaSharp;
using SkiaSharp.Views.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Maps.Overlays.Platforms.Droid.Extensions;
using Xamarin.Forms.Maps.Overlays.Skia;
using Xamarin.Forms.Maps.Overlays.WeakSubscription;
using static Xamarin.Forms.Maps.Overlays.SKMapOverlay;

namespace Xamarin.Forms.Maps.Overlays.Platforms.Android.UI.Renderers
{
    internal class OverlayTrackerTileProvider : Java.Lang.Object, ITileProvider
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
        public SKMapOverlay SharedOverlay { get; }

        private int _LastZoomLevel { get; set; } = -1;
        private Context _Context { get; }
        private GoogleMap _NativeMap { get; }
        private List<GroundOverlay> _GroundOverlays { get; } = new List<GroundOverlay>();

        private IDisposable _overlayDirtySubscription;
        private Queue<SKBitmap> _overlayBitmapPool = new Queue<SKBitmap>();

        public OverlayTrackerTileProvider(Context context, GoogleMap nativeMap, SKMapOverlay sharedOverlay)
        {
            _Context = context;
            _NativeMap = nativeMap;
            SharedOverlay = sharedOverlay;

            _overlayDirtySubscription = SharedOverlay.WeakSubscribe<SKMapOverlay, MapOverlayInvalidateEventArgs>(nameof(SharedOverlay.RequestInvalidate),
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
                int virtualTileSize = Extensions.SKMapExtensions.MercatorMapSize >> tileInfo.Zoom;
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
}
