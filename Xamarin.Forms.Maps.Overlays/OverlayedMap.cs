// **********************************************************************
// 
//   OverlayedMap.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms.Maps.Overlays.WeakSubscription;

namespace Xamarin.Forms.Maps.Overlays
{
    public class OverlayedMap : Map
    {
        public static readonly BindableProperty MapOverlaysProperty = BindableProperty.Create(nameof(MapOverlays),
                                                                                              typeof(ObservableCollection<DrawableMapOverlay>),
                                                                                              typeof(OverlayedMap),
                                                                                              new ObservableCollection<DrawableMapOverlay>());
        public static readonly BindableProperty MapMarkersProperty = BindableProperty.Create(nameof(MapMarkers),
                                                                                             typeof(ObservableCollection<DrawableMapMarker>),
                                                                                             typeof(OverlayedMap),
                                                                                             new ObservableCollection<DrawableMapMarker>());

        public ObservableCollection<DrawableMapOverlay> MapOverlays => GetValue(MapOverlaysProperty) as ObservableCollection<DrawableMapOverlay>;
        public ObservableCollection<DrawableMapMarker> MapMarkers => GetValue(MapMarkersProperty) as ObservableCollection<DrawableMapMarker>;

        private IDisposable _overlaysCollectionChangedSubscription;
        private IDisposable _markersCollectionChangedSubscription;

        public OverlayedMap()
        {
            _overlaysCollectionChangedSubscription = MapOverlays.WeakSubscribe(OnMapItemsCollectionChanged);
            _markersCollectionChangedSubscription = MapOverlays.WeakSubscribe(OnMapItemsCollectionChanged);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            UpdateInheritedBindingContext();
        }

        private void UpdateInheritedBindingContext()
        {
            UpdateInheritedBindingContext(Enumerable.Union<BindableObject>(MapOverlays, MapMarkers), this.BindingContext);
        }

        private void UpdateInheritedBindingContext(IEnumerable<BindableObject> items, object bindingContext)
        {
            foreach (BindableObject overlay in items)
            {
                overlay.BindingContext = bindingContext;
            }
        }

        private void OnMapItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            IEnumerable<BindableObject> newItems = (args.NewItems?.Cast<BindableObject>()) ?? new List<BindableObject>();
            IEnumerable<BindableObject> removedItems = (args.OldItems?.Cast<BindableObject>()) ?? new List<BindableObject>();

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    UpdateInheritedBindingContext(newItems, this.BindingContext);
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    UpdateInheritedBindingContext(removedItems, null);
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    UpdateInheritedBindingContext(newItems, this.BindingContext);
                    UpdateInheritedBindingContext(removedItems, null);
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    UpdateInheritedBindingContext((sender as ICollection)?.Cast<BindableObject>(), this.BindingContext);
                    break;
                }
            }
        }
    }
}
