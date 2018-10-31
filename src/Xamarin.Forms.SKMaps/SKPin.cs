using SkiaSharp;
using System;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.SKMaps
{
    public abstract class SKPin : Pin
    {
        public class MapMarkerInvalidateEventArgs
        {
            public double Width { get; }
            public double Height { get; }

            internal MapMarkerInvalidateEventArgs(SKPin marker)
            {
                Width = marker.Width;
                Height = marker.Height;
            }
        }

        public event EventHandler<MapMarkerInvalidateEventArgs> RequestInvalidate;

        public static readonly BindableProperty WidthProperty = BindableProperty.Create(nameof(Width), typeof(double), typeof(SKPin), 32.0, propertyChanged: OnDrawablePropertyChanged);
        public static readonly BindableProperty HeightProperty = BindableProperty.Create(nameof(Height), typeof(double), typeof(SKPin), 32.0, propertyChanged: OnDrawablePropertyChanged);
        public static readonly BindableProperty AnchorXProperty = BindableProperty.Create(nameof(AnchorX), typeof(double), typeof(SKPin), 0.5);
        public static readonly BindableProperty AnchorYProperty = BindableProperty.Create(nameof(AnchorY), typeof(double), typeof(SKPin), 0.5);
        public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(SKPin), true);
        public static readonly BindableProperty ClickableProperty = BindableProperty.Create(nameof(Clickable), typeof(bool), typeof(SKPin), true);

        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public double AnchorX
        {
            get { return (double)GetValue(AnchorXProperty); }
            set { SetValue(AnchorXProperty, value); }
        }

        public double AnchorY
        {
            get { return (double)GetValue(AnchorYProperty); }
            set { SetValue(AnchorYProperty, value); }
        }

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public bool Clickable
        {
            get { return (bool)GetValue(ClickableProperty); }
            set { SetValue(ClickableProperty, value); }
        }

        private static void OnDrawablePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SKPin marker = bindable as SKPin;

            marker.Invalidate();
        }

        public void Invalidate()
        {
            RequestInvalidate?.Invoke(this, new MapMarkerInvalidateEventArgs(this));
        }

        public abstract void DrawPin(SKSurface surface);
    }
}
