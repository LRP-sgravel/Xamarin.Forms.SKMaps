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

using FormsSkiaBikeTracker.Forms.UI.Pages;
using FormsSkiaBikeTracker.Shared.Helpers;
using FormsSkiaBikeTracker.Shared.Models.Maps;
using LRPLib.Views.XForms;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Forms.UI.Controls.Maps
{
    public abstract class DrawableMapOverlay : DrawableView
    {
        public static readonly BindableProperty GpsBoundsProperty = BindableProperty.Create(nameof(GpsBounds), typeof(MapSpan), typeof(DrawableMapOverlay), new MapSpan(new Position(0, 0), 0.1, 0.1));

        public MapSpan GpsBounds
        {
            get => (MapSpan)GetValue(GpsBoundsProperty);
            set => SetValue(GpsBoundsProperty, value.WrapIfRequired());
        }

        protected DrawableMapOverlay()
        {
            HasTransparency = true;
        }

        protected override void Paint(SKCanvas canvas)
        {
        }

        public abstract void DrawOnMap(SKMapCanvas canvas, SKMapSpan canvasMapRect, double pixelScale);
    }
}
