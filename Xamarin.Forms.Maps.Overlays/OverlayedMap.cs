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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MvvmCross.WeakSubscription;
using Xamarin.Forms.Maps.Overlays.Skia;

namespace Xamarin.Forms.Maps.Overlays
{
    public class OverlayedMap : Map
    {
        public static readonly BindableProperty MapOverlaysProperty = BindableProperty.Create(nameof(MapOverlays),
                                                                                              typeof(ObservableCollection<DrawableMapOverlay>),
                                                                                              typeof(OverlayedMap),
                                                                                              new ObservableCollection<DrawableMapOverlay>());

        private IDisposable _overlaysCollectionChangedSubscription;

        public ObservableCollection<DrawableMapOverlay> MapOverlays => (ObservableCollection<DrawableMapOverlay>)GetValue(MapOverlaysProperty);

        public OverlayedMap()
        {
            _overlaysCollectionChangedSubscription = MapOverlays.WeakSubscribe(OnMapOverlaysCollectionChanged);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            UpdateInheritedBindingContext();
        }

        private void UpdateInheritedBindingContext()
        {
            if (MapOverlays != null)
            {
                foreach (DrawableMapOverlay overlay in MapOverlays)
                {
                    overlay.Parent = this;
                }
            }
        }

        private void OnMapOverlaysCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            IList<DrawableMapOverlay> newItems = (args.NewItems as IList<DrawableMapOverlay>) ?? new List<DrawableMapOverlay>();
            IList<DrawableMapOverlay> removedItems = (args.OldItems as IList<DrawableMapOverlay>) ?? new List<DrawableMapOverlay>();

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (DrawableMapOverlay overlay in newItems)
                    {
                        overlay.Parent = this;
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (DrawableMapOverlay overlay in removedItems)
                    {
                        overlay.Parent = null;
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (DrawableMapOverlay overlay in newItems)
                    {
                        overlay.Parent = this;
                    }
                    foreach (DrawableMapOverlay overlay in removedItems)
                    {
                        overlay.Parent = null;
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    UpdateInheritedBindingContext();
                    break;
                }
            }
        }
    }
}
