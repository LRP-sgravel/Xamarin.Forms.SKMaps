using SkiaSharp;
using System;

namespace Xamarin.Forms.Maps.Overlays
{
    public abstract class DrawableMapMarker : BindableObject
    {
        public class MapMarkerInvalidateEventArgs
        {
            public Position GpsPosition { get; set; }
            public Size MarkerSize { get; set; }
            public bool IsVisible { get; set; }

            internal MapMarkerInvalidateEventArgs(DrawableMapMarker marker)
            {
                GpsPosition = marker.GpsPosition;
                MarkerSize = marker.MarkerSize;
                IsVisible = marker.IsVisible;
            }
        }

        public event EventHandler<MapMarkerInvalidateEventArgs> RequestInvalidate;

        public static readonly BindableProperty GpsPositionProperty = BindableProperty.Create(nameof(GpsPosition), typeof(Position), typeof(DrawableMapMarker), new Position(0, 0), propertyChanged: OnDrawablePropertyChanged);
        public static readonly BindableProperty MarkerSizeProperty = BindableProperty.Create(nameof(MarkerSize), typeof(Size), typeof(DrawableMapMarker), new Size(32, 32), propertyChanged: OnDrawablePropertyChanged);
        public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(DrawableMapMarker), true, propertyChanged: OnDrawablePropertyChanged);

        public Position GpsPosition
        {
            get => (Position)GetValue(GpsPositionProperty);
            set => SetValue(GpsPositionProperty, value);
        }

        public Size MarkerSize
        {
            get { return (Size)GetValue(MarkerSizeProperty); }
            set { SetValue(MarkerSizeProperty, value); }
        }

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
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

        public abstract void DrawMarker(SKCanvas canvas);
    }
}
