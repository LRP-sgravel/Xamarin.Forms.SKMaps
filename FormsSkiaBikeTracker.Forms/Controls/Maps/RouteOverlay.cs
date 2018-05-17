// **********************************************************************
// 
//   RouteOverlay.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using FormsSkiaBikeTracker.Services.Interface;
using LRPFramework.Views.Forms.Extensions;
using MvvmCross.WeakSubscription;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Maps.Overlays.Skia;

namespace FormsSkiaBikeTracker.Forms.Controls.Maps
{
    public class RouteOverlay : DrawableMapOverlay
    {
        public static readonly BindableProperty RouteProperty = BindableProperty.Create(nameof(Route), typeof(IRoute), typeof(RouteOverlay), propertyChanged: OnRouteChanged);
        public static readonly BindableProperty LineColorProperty = BindableProperty.Create(nameof(LineColor), typeof(Color), typeof(RouteOverlay), Color.Black);
        public static readonly BindableProperty LineWidthProperty = BindableProperty.Create(nameof(LineWidth), typeof(float), typeof(RouteOverlay), 3.0f);

        public IRoute Route
        {
            get => (IRoute)GetValue(RouteProperty);
            set => SetValue(RouteProperty, value);
        }

        public Color LineColor
        {
            get => (Color)GetValue(LineColorProperty);
            set => SetValue(LineColorProperty, value);
        }

        public float LineWidth
        {
            get => (float)GetValue(LineWidthProperty);
            set => SetValue(LineWidthProperty, value);
        }

        private object _mutableRouteSubscription;

        private static void OnRouteChanged(BindableObject bindable, object oldValue, object newValue)
        {
            RouteOverlay overlay = bindable as RouteOverlay;

            overlay.TrySubscribeToMutableRoute();
            overlay.UpdateLOD();
            overlay.UpdateBounds();
        }

        public override void DrawOnMap(SKMapCanvas canvas, SKMapSpan canvasMapRect, double zoomScale)
        {
            if (Route != null && Route.Points.Count() > 1)
            {
                Position firstPoint = Route.Points.FirstOrDefault();
                List<Position> currentRoute = new List<Position>(Route.Points.Skip(1));
                SKMapPath path = canvas.CreateEmptyMapPath();

                path.MoveTo(firstPoint);
                foreach (Position nextPoint in currentRoute)
                {
                    path.LineTo(nextPoint);
                }

                using (SKPaint paint = new SKPaint())
                {
                    paint.Color = LineColor.ToSKColor();
                    paint.StrokeWidth = LineWidth;
                    paint.Style = SKPaintStyle.Stroke;
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void UpdateBounds()
        {
            if (Route != null)
            {
                GpsBounds = MapSpanExtensions.WorldSpan;
            }
            else
            {
                GpsBounds = MapSpan.FromCenterAndRadius(new Position(), Distance.FromMeters(0));
            }
        }
        
        private void UpdateLOD()
        {
            
        }

        private void TrySubscribeToMutableRoute()
        {
            IMutableRoute mutableRoute = Route as IMutableRoute;

            _mutableRouteSubscription = null;
            if (mutableRoute != null)
            {
                _mutableRouteSubscription = mutableRoute.WeakSubscribe<IMutableRoute, PointsAddedEventArgs>(nameof(mutableRoute.PointsAdded),
                                                                                                            OnRoutePointsAdded);
            }
        }

        private void OnRoutePointsAdded(object sender, PointsAddedEventArgs e)
        {
        }
    }
}
