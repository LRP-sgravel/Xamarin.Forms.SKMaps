using SkiaSharp;
using System;

namespace Xamarin.Forms.Maps.Overlays
{
    public abstract class DrawableMapMarker : Pin
    {
        public class MapMarkerInvalidateEventArgs
        {
            public double Width { get; }
            public double Height { get; }

            internal MapMarkerInvalidateEventArgs(DrawableMapMarker marker)
            {
                Width = marker.Width;
                Height = marker.Height;
            }
        }

        public event EventHandler<MapMarkerInvalidateEventArgs> RequestInvalidate;

        public static readonly BindableProperty WidthProperty = BindableProperty.Create(nameof(Width), typeof(double), typeof(DrawableMapMarker), 32.0, propertyChanged: OnDrawablePropertyChanged);
        public static readonly BindableProperty HeightProperty = BindableProperty.Create(nameof(Height), typeof(double), typeof(DrawableMapMarker), 32.0, propertyChanged: OnDrawablePropertyChanged);
        public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(DrawableMapMarker), true);
        public static readonly BindableProperty ClickableProperty = BindableProperty.Create(nameof(Clickable), typeof(bool), typeof(DrawableMapMarker), true);

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
            DrawableMapMarker marker = bindable as DrawableMapMarker;

            marker.Invalidate();
        }

        protected void Invalidate()
        {
            RequestInvalidate?.Invoke(this, new MapMarkerInvalidateEventArgs(this));
        }

        public abstract void DrawMarker(SKSurface surface);
    }
}
