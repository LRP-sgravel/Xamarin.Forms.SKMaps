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
using LRPFramework.Views.Forms;
using SkiaSharp;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Maps.Overlays.Skia;

namespace Xamarin.Forms.Maps.Overlays
{
    public abstract class DrawableMapOverlay : DrawableView
    {
        public event EventHandler<MapSpan> RequestInvalidate;

        public static readonly BindableProperty GpsBoundsProperty = BindableProperty.Create(nameof(GpsBounds), typeof(MapSpan), typeof(DrawableMapOverlay), new MapSpan(new Position(0, 0), 0.1, 0.1));

        public MapSpan GpsBounds
        {
            get => (MapSpan)GetValue(GpsBoundsProperty);
            set => SetValue(GpsBoundsProperty, value.WrapIfRequired());
        }

        protected DrawableMapOverlay()
        {
            HasTransparency = true;
            GpsBounds = MapSpanExtensions.WorldSpan;
        }

        protected sealed override void Paint(SKCanvas canvas)
        {
            // Use the DrawOnMap method
        }

        protected new void Invalidate()
        {
            if (IsVisible)
            {
                RequestInvalidate?.Invoke(this, GpsBounds);
            }
        }

        public abstract void DrawOnMap(SKMapCanvas canvas, SKMapSpan canvasMapRect, double zoomScale);
    }
}
