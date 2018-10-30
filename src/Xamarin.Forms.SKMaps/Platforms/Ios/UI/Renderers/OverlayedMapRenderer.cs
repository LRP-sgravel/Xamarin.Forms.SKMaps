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
using MapKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.SKMaps.Platforms.Ios.Extensions;
using Xamarin.Forms.SKMaps.Platforms.Ios.UI.Renderers;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.SKMaps;
using Xamarin.Forms.Maps;

[assembly: ExportRenderer(typeof(SKMap), typeof(SKMapRenderer))]
namespace Xamarin.Forms.SKMaps.Platforms.Ios.UI.Renderers
{
    class SKMapRenderer : MapRenderer
    {
        private static class OverlayedMapDelegate
        {
            public static MKOverlayRenderer OverlayRenderer(MKMapView mapView, IMKOverlay overlay)
            {
                if (overlay is SKMapKitOverlay)
                {
                    return new SKMapOverlayRenderer(mapView, (overlay as SKMapKitOverlay).SharedOverlay, overlay);
                }

                return null;
            }
        }

        private SKMap _SharedControl => Element as SKMap;
        private MKMapView _NativeControl => Control as MKMapView;

        protected override void OnElementChanged(ElementChangedEventArgs<View> args)
        {
            SKMap oldMap = args.OldElement as SKMap;
            SKMap newMap = args.NewElement as SKMap;

            if (oldMap != null)
            {
                _NativeControl.OverlayRenderer = null;
                _NativeControl.GetViewForAnnotation = null;

                if (oldMap.MapOverlays != null)
                {
                    oldMap.MapOverlays.CollectionChanged -= MapOverlaysCollectionChanged;
                }

                if (oldMap.Pins as ObservableCollection<Pin> != null)
                {
                    ((ObservableCollection<Pin>)oldMap.Pins).CollectionChanged -= PinsCollectionChanged;
                }
                UnregisterPinCallbacks(oldMap.Pins);
            }
            
            base.OnElementChanged(args);

            if (newMap != null)
            {
                _NativeControl.OverlayRenderer = OverlayedMapDelegate.OverlayRenderer;
                _NativeControl.GetViewForAnnotation = GetViewForPin;

                if (_SharedControl?.MapOverlays != null)
                {
                    _SharedControl.MapOverlays.CollectionChanged += MapOverlaysCollectionChanged;
                    SetupMapOverlays();
                }

                if (newMap.Pins as ObservableCollection<Pin> != null)
                {
                    ((ObservableCollection<Pin>)newMap.Pins).CollectionChanged += PinsCollectionChanged;
                }
                RegisterPinCallbacks(newMap.Pins);
            }
        }

        private void MapOverlaysCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            IEnumerable<SKMapOverlay> newItems = args.NewItems?
                                                     .Cast<SKMapOverlay>()
                                                     .DefaultIfEmpty();
            IEnumerable<SKMapOverlay> removedItems = args.OldItems?
                                                         .Cast<SKMapOverlay>()
                                                         .DefaultIfEmpty();
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    _NativeControl.AddOverlays(newItems.Select(o => (IMKOverlay)new SKMapKitOverlay(o, _SharedControl))
                                                       .ToArray());
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    _NativeControl.RemoveOverlays(_NativeControl.Overlays
                                                                .Where(o => removedItems.Contains((o as SKMapKitOverlay).SharedOverlay))
                                                                .ToArray());
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    _NativeControl.RemoveOverlays(_NativeControl.Overlays
                                                                .Where(o => removedItems.Contains((o as SKMapKitOverlay).SharedOverlay))
                                                                .ToArray());
                    _NativeControl.AddOverlays(newItems.Select(o => (IMKOverlay)new SKMapKitOverlay(o, _SharedControl))
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
                                                             .Select(o => (IMKOverlay)new SKMapKitOverlay(o, _SharedControl))
                                                             .ToArray());
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

        private void SetupMapOverlays()
        {
            MapOverlaysCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private MKAnnotationView GetViewForPin(MKMapView mapView, IMKAnnotation annotation)
        {
            SKPinAnnotation skiaAnnotation = annotation as SKPinAnnotation;

            if (skiaAnnotation != null)
            {
                SKPin pin = skiaAnnotation.SharedPin;
                SKPinAnnotationView pinView = mapView.DequeueReusableAnnotation(SKPinAnnotationView.ViewIdentifier) as SKPinAnnotationView
                                                    ?? CreateAnnotationView(skiaAnnotation);

                pinView.Annotation = skiaAnnotation;
                pinView.UpdateImage();
                pinView.UpdateAnchor();
                pinView.Hidden = !pin.IsVisible;

                return pinView;
            }

            return null;
        }

        private SKPinAnnotationView CreateAnnotationView(SKPinAnnotation skiaAnnotation)
        {
            return new SKPinAnnotationView(skiaAnnotation);
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

        private void OnPinPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Pin pin = sender as Pin;
            SKPin skiaPin = pin as SKPin;
            MKPointAnnotation annotation = FindAnnotationForPin(pin) as MKPointAnnotation;

            if (skiaPin != null)
            {
                if (args.PropertyName == SKPin.WidthProperty.PropertyName ||
                    args.PropertyName == SKPin.HeightProperty.PropertyName)
                {
                    UpdateAnnotationIcon(skiaPin);
                }
                else if (args.PropertyName == SKPin.IsVisibleProperty.PropertyName)
                {
                    MKAnnotationView view = _NativeControl.ViewForAnnotation(annotation);

                    if (view != null)
                    {
                        view.Hidden = !skiaPin.IsVisible;
                    }
                }
            }

            if (pin != null && annotation != null)
            {
                if (args.PropertyName == Pin.LabelProperty.PropertyName)
                {
                    annotation.Title = pin.Label;
                }
                else if (args.PropertyName == Pin.AddressProperty.PropertyName)
                {
                    annotation.Subtitle = pin.Address;
                }
                else if (args.PropertyName == Pin.PositionProperty.PropertyName)
                {
                    annotation.Coordinate = pin.Position.ToLocationCoordinate();
                }
            }
        }

        protected override IMKAnnotation CreateAnnotation(Pin pin)
        {
            if (pin is SKPin)
            {
                IMKAnnotation result = new SKPinAnnotation(pin as SKPin);

                pin.Id = result;

                return result;
            }

            return base.CreateAnnotation(pin);
        }

        private IMKAnnotation FindAnnotationForPin(Pin pin)
        {
            if (pin == null)
            {
                return null;
            }

            return pin.Id as IMKAnnotation;
        }

        private void OnPinInvalidateRequested(object sender, SKPin.MapMarkerInvalidateEventArgs args)
        {
            SKPin pin = sender as SKPin;

            if (pin != null)
            {
                UpdateAnnotationIcon(pin);
            }
        }

        private void UpdateAnnotationIcon(SKPin pin)
        {
            IMKAnnotation annotation = FindAnnotationForPin(pin);
            MKAnnotationView view = _NativeControl.ViewForAnnotation(annotation);
            SKPinAnnotationView skPinView = view as SKPinAnnotationView;

            skPinView?.UpdateImage();
            skPinView?.UpdateAnchor();
        }
    }
}
