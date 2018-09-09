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
using Xamarin.Forms.Maps.Overlays;
using Xamarin.Forms.Maps.Overlays.Platforms.Android.UI.Renderers;
using Xamarin.Forms.Platform.Android;
using static Android.Gms.Maps.GoogleMap;

[assembly: ExportRenderer(typeof(OverlayedMap), typeof(OverlayedMapRenderer))]
namespace Xamarin.Forms.Maps.Overlays.Platforms.Android.UI.Renderers
{
    internal class OverlayedMapRenderer : XamarinMapRenderer, IOnMarkerClickListener
    {
        private OverlayedMap _SharedControl => Element as OverlayedMap;
        private MapView _NativeControl => Control as MapView;

        private List<OverlayTrackerTileProvider> _TileTrackers { get; set; } = new List<OverlayTrackerTileProvider>();

        public OverlayedMapRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Map> args)
        {
            OverlayedMap oldMap = args.OldElement as OverlayedMap;
            OverlayedMap newMap = args.NewElement as OverlayedMap;

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

            if (pin is DrawableMapMarker)
            {
                DrawableMapMarker sharedMarker = pin as DrawableMapMarker;
                SKBitmap markerBitmap = DrawMarker(sharedMarker);

                options.SetIcon(BitmapDescriptorFactory.FromBitmap(markerBitmap.ToBitmap()))
                       .Visible(sharedMarker.IsVisible);
            }

            return options;
        }

        private void OnPinPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            DrawableMapMarker pin = sender as DrawableMapMarker;
            Marker marker = FindMarkerForPin(pin);

            if (pin != null)
            {
                if (args.PropertyName == DrawableMapMarker.WidthProperty.PropertyName ||
                    args.PropertyName == DrawableMapMarker.HeightProperty.PropertyName)
                {
                    UpdateMarkerIcon(pin, marker);
                }
                else if (args.PropertyName == DrawableMapMarker.IsVisibleProperty.PropertyName)
                {
                    marker.Visible = pin.IsVisible;
                }
            }
        }

        private void OnPinInvalidateRequested(object sender, DrawableMapMarker.MapMarkerInvalidateEventArgs args)
        {
            DrawableMapMarker pin = sender as DrawableMapMarker;
            Marker marker = FindMarkerForPin(pin);

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
            IEnumerable<DrawableMapOverlay> newItems = args.NewItems?
                                                           .OfType<DrawableMapOverlay>()
                                                           .DefaultIfEmpty();
            IEnumerable<DrawableMapOverlay> removedItems = args.OldItems?
                                                               .OfType<DrawableMapOverlay>()
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
                        foreach (DrawableMapOverlay mapOverlay in removedItems)
                        {
                            OverlayTrackerTileProvider tracker = FindTrackerForOverlay(mapOverlay);

                            if (tracker != null)
                            {
                                RemoveTracker(tracker);
                            }
                        }

                        foreach (DrawableMapOverlay mapOverlay in newItems)
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

                        foreach (DrawableMapOverlay overlay in _SharedControl.MapOverlays)
                        {
                            AddTrackerForOverlay(overlay);
                        }
                        break;
                    }
            }
        }

        private void PinsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            IEnumerable<DrawableMapMarker> newItems = args.NewItems?
                                                          .OfType<DrawableMapMarker>()
                                                          .DefaultIfEmpty();
            IEnumerable<DrawableMapMarker> removedItems = args.OldItems?
                                                              .OfType<DrawableMapMarker>()
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

        private void UnregisterPinCallbacks(IEnumerable<Pin> removedItems)
        {
            foreach (DrawableMapMarker pin in removedItems.OfType<DrawableMapMarker>())
            {
                pin.PropertyChanged -= OnPinPropertyChanged;
                pin.RequestInvalidate -= OnPinInvalidateRequested;
            }
        }

        private void RegisterPinCallbacks(IEnumerable<Pin> newItems)
        {
            foreach (DrawableMapMarker pin in newItems.OfType<DrawableMapMarker>())
            {
                pin.PropertyChanged += OnPinPropertyChanged;
                pin.RequestInvalidate += OnPinInvalidateRequested;
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

        private OverlayTrackerTileProvider FindTrackerForOverlay(DrawableMapOverlay mapOverlay)
        {
            return _TileTrackers.FirstOrDefault(t => t.SharedOverlay == mapOverlay);
        }

        private void UpdateMarkerIcon(DrawableMapMarker pin, Marker marker)
        {
            SKBitmap markerBitmap = DrawMarker(pin);

            marker.SetIcon(BitmapDescriptorFactory.FromBitmap(markerBitmap.ToBitmap()));
        }

        private SKBitmap DrawMarker(DrawableMapMarker sharedMarker)
        {
            Size markerSize = new Size(sharedMarker.Width * Context.Resources.DisplayMetrics.Density,
                                       sharedMarker.Height * Context.Resources.DisplayMetrics.Density);
            SKBitmap markerBitmap = new SKBitmap((int)markerSize.Width, (int)markerSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            SKCanvas canvas = new SKCanvas(markerBitmap);

            markerBitmap.Erase(SKColor.Empty);
            sharedMarker.DrawMarker(canvas);

            return markerBitmap;
        }

        public bool OnMarkerClick(Marker marker)
        {
            Pin clickedMarker = _SharedControl.Pins.FirstOrDefault(p => (string)p.Id == marker.Id);
            DrawableMapMarker drawableMarker = clickedMarker as DrawableMapMarker;

            return drawableMarker?.Clickable ?? false;
        }
    }
}
