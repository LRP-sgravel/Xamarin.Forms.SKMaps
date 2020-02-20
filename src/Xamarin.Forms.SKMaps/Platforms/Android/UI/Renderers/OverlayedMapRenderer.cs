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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using SkiaSharp;
using SkiaSharp.Views.Android;
using Xamarin.Forms;
using Xamarin.Forms.SKMaps;
using Xamarin.Forms.SKMaps.Platforms.Android.UI.Renderers;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using static Android.Gms.Maps.GoogleMap;

[assembly: ExportRenderer(typeof(SKMap), typeof(SKMapRenderer))]
namespace Xamarin.Forms.SKMaps.Platforms.Android.UI.Renderers
{
    internal class SKMapRenderer : MapRenderer, IOnMarkerClickListener
    {
        private SKMap _SharedControl => Element as SKMap;
        private MapView _NativeControl => Control as MapView;

        private List<OverlayTrackerTileProvider> _TileTrackers { get; set; } = new List<OverlayTrackerTileProvider>();

        public SKMapRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Map> args)
        {
            SKMap oldMap = args.OldElement as SKMap;
            SKMap newMap = args.NewElement as SKMap;

            if(oldMap != null)
            {
                if (oldMap.MapOverlays != null)
                {
                    oldMap.MapOverlays.CollectionChanged -= MapOverlaysCollectionChanged;
                }

                if (oldMap.Pins as ObservableCollection<Pin> != null)
                {
                    ((ObservableCollection<Pin>)oldMap.Pins).CollectionChanged -= PinsCollectionChanged;
                }

                NativeMap?.SetOnMarkerClickListener(null);

                UnregisterPinCallbacks(oldMap.Pins);
            }

            base.OnElementChanged(args);

            if (newMap != null)
            {
                if (newMap.MapOverlays != null)
                {
                    newMap.MapOverlays.CollectionChanged += MapOverlaysCollectionChanged;
                }

                if (newMap.Pins as ObservableCollection<Pin> != null)
                {
                    ((ObservableCollection<Pin>)newMap.Pins).CollectionChanged += PinsCollectionChanged;
                }

                RegisterPinCallbacks(newMap.Pins);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && Element != null)
            {
                UnregisterPinCallbacks(Element.Pins);
            }
        }

        protected override MarkerOptions CreateMarker(Pin pin)
        {
            MarkerOptions options = base.CreateMarker(pin);

            if (pin is SKPin)
            {
                SKPin sharedMarker = pin as SKPin;
                SKPixmap markerBitmap = DrawMarker(sharedMarker);

                options.SetIcon(BitmapDescriptorFactory.FromBitmap(markerBitmap.ToBitmap()))
                       .Visible(sharedMarker.IsVisible);
                options.Anchor((float)sharedMarker.AnchorX, (float)sharedMarker.AnchorY);
            }

            return options;
        }

        private void OnPinPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            SKPin pin = sender as SKPin;
            Marker marker = GetMarkerForPin(pin);

            if (pin != null)
            {
                if (args.PropertyName == SKPin.WidthProperty.PropertyName ||
                    args.PropertyName == SKPin.HeightProperty.PropertyName)
                {
                    UpdateMarkerIcon(pin, marker);
                }
                else if (args.PropertyName == SKPin.AnchorXProperty.PropertyName ||
                         args.PropertyName == SKPin.AnchorYProperty.PropertyName)
                {
                    marker.SetAnchor((float)pin.AnchorX, (float)pin.AnchorY);
                }
                else if (args.PropertyName == SKPin.IsVisibleProperty.PropertyName)
                {
                    marker.Visible = pin.IsVisible;
                }
            }
        }

        private void OnPinInvalidateRequested(object sender, SKPin.MapMarkerInvalidateEventArgs args)
        {
            SKPin pin = sender as SKPin;
            Marker marker = GetMarkerForPin(pin);

            UpdateMarkerIcon(pin, marker);
        }

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);

            map.SetOnMarkerClickListener(this);

            SetupMapOverlays();
            SetupPinCallbacks();
        }

        private void SetupMapOverlays()
        {
            MapOverlaysCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void SetupPinCallbacks()
        {
            PinsCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void MapOverlaysCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            IEnumerable<SKMapOverlay> newItems = args.NewItems?
                                                           .OfType<SKMapOverlay>()
                                                           .DefaultIfEmpty();
            IEnumerable<SKMapOverlay> removedItems = args.OldItems?
                                                               .OfType<SKMapOverlay>()
                                                               .DefaultIfEmpty();
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (SKMapOverlay mapOverlay in newItems)
                        {
                            AddTrackerForOverlay(mapOverlay);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (SKMapOverlay mapOverlay in removedItems)
                        {
                            OverlayTrackerTileProvider tracker = FindTrackerForOverlay(mapOverlay);

                            if (tracker != null)
                            {
                                RemoveTracker(tracker);
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (SKMapOverlay mapOverlay in removedItems)
                        {
                            OverlayTrackerTileProvider tracker = FindTrackerForOverlay(mapOverlay);

                            if (tracker != null)
                            {
                                RemoveTracker(tracker);
                            }
                        }

                        foreach (SKMapOverlay mapOverlay in newItems)
                        {
                            AddTrackerForOverlay(mapOverlay);
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

                        foreach (SKMapOverlay overlay in _SharedControl.MapOverlays)
                        {
                            AddTrackerForOverlay(overlay);
                        }
                        break;
                    }
            }
        }

        private void PinsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            IEnumerable<SKPin> newItems = args.NewItems?
                                                          .OfType<SKPin>()
                                                          .DefaultIfEmpty();
            IEnumerable<SKPin> removedItems = args.OldItems?
                                                              .OfType<SKPin>()
                                                              .DefaultIfEmpty();
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        RegisterPinCallbacks(newItems);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        UnregisterPinCallbacks(removedItems);
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        UnregisterPinCallbacks(removedItems);
                        RegisterPinCallbacks(newItems);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        // Nothing to do, event is already registered
                        break;
                    }
            }
        }

        private void RegisterPinCallbacks(IEnumerable<Pin> newItems)
        {
            foreach (SKPin pin in newItems.OfType<SKPin>())
            {
                pin.PropertyChanged += OnPinPropertyChanged;
                pin.RequestInvalidate += OnPinInvalidateRequested;
            }
        }

        private void UnregisterPinCallbacks(IEnumerable<Pin> removedItems)
        {
            foreach (SKPin pin in removedItems.OfType<SKPin>())
            {
                pin.PropertyChanged -= OnPinPropertyChanged;
                pin.RequestInvalidate -= OnPinInvalidateRequested;
            }
        }

        private void AddTrackerForOverlay(SKMapOverlay sharedOverlay)
        {
            if (NativeMap != null)
            {
                OverlayTrackerTileProvider tracker = new OverlayTrackerTileProvider(Context, NativeMap, sharedOverlay);
                TileOverlayOptions overlayOptions = new TileOverlayOptions().InvokeTileProvider(tracker);
                TileOverlay overlay = NativeMap.AddTileOverlay(overlayOptions);

                tracker.TileOverlay = overlay;
                _TileTrackers.Add(tracker);
            }
        }

        private void RemoveTracker(OverlayTrackerTileProvider tracker)
        {
            TileOverlay overlay = tracker.TileOverlay;

            tracker.RemoveAllTiles();
            overlay.Remove();

            _TileTrackers.Remove(tracker);
        }

        private OverlayTrackerTileProvider FindTrackerForOverlay(SKMapOverlay mapOverlay)
        {
            return _TileTrackers.FirstOrDefault(t => t.SharedOverlay == mapOverlay);
        }

        private void UpdateMarkerIcon(SKPin pin, Marker marker)
        {
            SKPixmap markerBitmap = DrawMarker(pin);

            marker.SetIcon(BitmapDescriptorFactory.FromBitmap(markerBitmap.ToBitmap()));
        }

        private SKPixmap DrawMarker(SKPin sharedMarker)
        {
            double bitmapWidth = sharedMarker.Width * Context.Resources.DisplayMetrics.Density;
            double bitmapHeight = sharedMarker.Height * Context.Resources.DisplayMetrics.Density;
            SKSurface surface = SKSurface.Create((int)bitmapWidth, (int)bitmapHeight, SKColorType.Rgba8888, SKAlphaType.Premul);

            surface.Canvas.Clear(SKColor.Empty);
            sharedMarker.DrawPin(surface);

            return surface.PeekPixels();
        }

        public bool OnMarkerClick(Marker marker)
        {
            Pin clickedMarker = _SharedControl.Pins.FirstOrDefault(p => (string)p.Id == marker.Id);
            SKPin drawableMarker = clickedMarker as SKPin;

            return drawableMarker?.Clickable ?? false;
        }
    }
}
