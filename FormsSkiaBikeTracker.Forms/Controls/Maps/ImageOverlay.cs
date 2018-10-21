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
using FormsSkiaBikeTracker.Forms.Services.SvgSourceHandlers;
using FormsSkiaBikeTracker.Services.Interface;
using MvvmCross.Logging;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Overlays;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace FormsSkiaBikeTracker.Forms.Controls.Maps
{
    public class ImageOverlay : SKPin
    {
        public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(ImageOverlay), propertyChanged: OnIconChanged);
        
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        private SKBitmap _bitmapIcon;
        private SKSvg _svgIcon;

        public ImageOverlay()
        {
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

        public override void DrawMarker(SKSurface surface)
        {
            SKCanvas canvas = surface.Canvas;

            if (_svgIcon != null)
            {
                SKMatrix fillMatrix = GetFillMatrix(canvas, _svgIcon.Picture.CullRect);

                canvas.DrawPicture(_svgIcon.Picture, ref fillMatrix);
            }
            else if(_bitmapIcon != null)
            {
                canvas.DrawBitmap(_bitmapIcon, 0, 0);
            }
        }

        private SKMatrix GetFillMatrix(SKCanvas canvas, SKRect sourceRect)
        {
            try
            {
                float fillScale = (float)GetFillScale(new Size(sourceRect.Size.Width, sourceRect.Size.Height), canvas.LocalClipBounds);
                SKMatrix initialTranslate = SKMatrix.MakeTranslation(sourceRect.Width * -0.5f,
                                                                     sourceRect.Height * -0.5f);
                SKMatrix finalTranslate = SKMatrix.MakeTranslation(canvas.LocalClipBounds.Width * 0.5f,
                                                                   canvas.LocalClipBounds.Height * 0.5f);
                SKMatrix scale = SKMatrix.MakeScale(fillScale, fillScale);
                SKMatrix result = SKMatrix.MakeIdentity();

                SKMatrix.Concat(ref result, result, finalTranslate);
                SKMatrix.Concat(ref result, result, scale);
                SKMatrix.Concat(ref result, result, initialTranslate); 

                return result;
            }
            catch (Exception)
            {
                return SKMatrix.MakeIdentity();
            }
        }

        private double GetFillScale(Size source, SKRect destination)
        {
            double xScale = destination.Width / source.Width;
            double yScale = destination.Height / source.Height;

            return Math.Min(xScale, yScale);
        }

        private async Task LoadIconImageSource(ImageSource source)
        {
            _svgIcon = null;
            _bitmapIcon = null;

            if (IsSvgImageSource(source))
            {
                ISvgSourceHandler svgHandler = GetSvgImageHandler(source.GetType());

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

        private ISvgSourceHandler GetSvgImageHandler(Type type)
        {
            ISvgSourceHandler result = null;

            if (type == typeof(UriImageSource))
                result = new UriSvgSourceHandler();
            else if (type == typeof(FileImageSource))
                result = new FileSvgSourceHandler();
            else if (type == typeof(StreamImageSource))
                result = new StreamSvgSourceHandler();

            return result;
        }
    }
}
