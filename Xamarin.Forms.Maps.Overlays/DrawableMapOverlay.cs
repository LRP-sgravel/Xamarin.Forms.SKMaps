// **********************************************************************
// 
//   DrawableMapOverlay.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using System;
using SkiaSharp;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Maps.Overlays.Skia;

namespace Xamarin.Forms.Maps.Overlays
{
    public abstract class DrawableMapOverlay : BindableObject
    {
        public event EventHandler<MapSpan> RequestInvalidate;

        public static readonly BindableProperty GpsBoundsProperty = BindableProperty.Create(nameof(GpsBounds), typeof(MapSpan), typeof(DrawableMapOverlay), new MapSpan(new Position(0, 0), 0.1, 0.1));
        public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(DrawableMapOverlay), true);

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public MapSpan GpsBounds
        {
            get => (MapSpan)GetValue(GpsBoundsProperty);
            set => SetValue(GpsBoundsProperty, value.WrapIfRequired());
        }

        protected DrawableMapOverlay()
        {
            GpsBounds = MapSpanExtensions.WorldSpan;
        }

        protected void Invalidate()
        {
            if (IsVisible)
            {
                RequestInvalidate?.Invoke(this, GpsBounds);
            }
        }

        public abstract void DrawOnMap(SKMapCanvas canvas, SKMapSpan canvasMapRect, double zoomScale);
    }
}
