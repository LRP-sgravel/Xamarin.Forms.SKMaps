// **********************************************************************
// 
//   ImageOverlay.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System;
using System.Threading.Tasks;
using LRPFramework.Views.Forms.SourceHandler;
using MvvmCross.Logging;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Overlays;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Maps.Overlays.Skia;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace FormsSkiaBikeTracker.Forms.Controls.Maps
{
    public class ImageOverlay : DrawableMapOverlay
    {
        public static readonly BindableProperty PositionProperty =
            BindableProperty.Create(nameof(Position), typeof(Position), typeof(ImageOverlay), default(Position), propertyChanged: OnPositionChanged);
        public static readonly BindableProperty IconProperty =
            BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(ImageOverlay), propertyChanged: OnIconChanged);
        public static readonly BindableProperty IconSizePixelsProperty =
            BindableProperty.Create(nameof(IconSizePixels), typeof(Size), typeof(ImageOverlay), new Size(75, 75), propertyChanged: OnIconSizeChanged);
        
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public Size IconSizePixels
        {
            get => (Size)GetValue(IconSizePixelsProperty);
            set => SetValue(IconSizePixelsProperty, value);
        }

        public Position Position
        {
            get => (Position)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        private SKBitmap _bitmapIcon;

        private SKMapSpan _iconMaxArea;
        private SKSvg _svgIcon;

        public ImageOverlay()
        {
            UpdateIconArea();
        }

        private static void OnPositionChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ImageOverlay overlay = bindable as ImageOverlay;

            overlay.UpdateBounds();
        }

        private static async void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ImageOverlay overlay = bindable as ImageOverlay;

            try
            {
                await overlay.LoadIconImageSource(overlay.Icon);
            }
            catch (Exception e)
            {
                MvxLog.Instance.Log(MvxLogLevel.Error, () => $"Error loading icon image ({e.Message})");
            }
        }

        private static void OnIconSizeChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ImageOverlay overlay = bindable as ImageOverlay;

            overlay.UpdateIconArea();
        }

        public override void DrawOnMap(SKMapCanvas canvas, SKMapSpan canvasMapRect, double zoomScale)
        {
            SKMapSpan iconArea = SKMapCanvas.PixelsToMaximumMapSpanAtScale(IconSizePixels, zoomScale);
            MapSpan centeredSpan = new SKMapSpan(Position, iconArea.LatitudeDegrees, iconArea.LongitudeDegrees).ToMapSpan();

            // More precise/zoom based culling to reduce drawing calls
            if (Icon != null && canvasMapRect.FastIntersects(centeredSpan))
            {
                if (_svgIcon != null)
                {
                    canvas.DrawPicture(_svgIcon.Picture, centeredSpan.Center, IconSizePixels);
                }
                else if (_bitmapIcon != null)
                {
                    canvas.DrawBitmap(_bitmapIcon, centeredSpan);
                }
            }
        }

        private void UpdateBounds()
        {
            GpsBounds = new MapSpan(Position, _iconMaxArea.LatitudeDegrees, _iconMaxArea.LongitudeDegrees);
        }

        private void UpdateIconArea()
        {
            _iconMaxArea = SKMapCanvas.PixelsToMaximumMapSpanAtScale(IconSizePixels, SKMapCanvas.MaxZoomScale);
            UpdateBounds();
        }

        private async Task LoadIconImageSource(ImageSource source)
        {
            _svgIcon = null;
            _bitmapIcon = null;

            if (IsSvgImageSource(source))
            {
                ISvgImageSourceHandler svgHandler = GetSvgImageHandler(source.GetType());

                _svgIcon = await svgHandler.LoadImageAsync(source)
                                           .ConfigureAwait(false);
            }
            else if (source is SKBitmapImageSource)
            {
                _bitmapIcon = (source as SKBitmapImageSource).Bitmap;
            }

            Invalidate();
        }

        private bool IsSvgImageSource(ImageSource source)
        {
            Type type = source.GetType();

            return type == typeof(UriImageSource) ||
                   type == typeof(FileImageSource) ||
                   type == typeof(StreamImageSource);
        }

        private ISvgImageSourceHandler GetSvgImageHandler(Type type)
        {
            ISvgImageSourceHandler result = null;

            if (type == typeof(UriImageSource))
                result = new UriSvgLoaderSourceHandler();
            else if (type == typeof(FileImageSource))
                result = new FileSvgImageSourceHandler();
            else if (type == typeof(StreamImageSource))
                result = new StreamSvgImageSourceHandler();

            return result;
        }
    }
}
