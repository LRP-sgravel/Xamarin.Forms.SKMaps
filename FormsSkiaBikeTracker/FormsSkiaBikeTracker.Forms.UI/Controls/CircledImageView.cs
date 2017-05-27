// **********************************************************************
// 
//   CircledImageView.xaml.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************
using System;
using LRPLib.Services.Resources;
using LRPLib.Views.XForms;
using LRPLib.Views.XForms.Extensions;
using MvvmCross.Platform;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Controls
{
    public class CircledImageView : SKCanvasView
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source),
                                                                                         typeof(SKBitmapImageSource),
                                                                                         typeof(CircledImageView),
                                                                                         null,
                                                                                         BindingMode.OneWay,
                                                                                         null,
                                                                                         InvalidatePropertyChanged);

        public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth),
                                                                                              typeof(int),
                                                                                              typeof(CircledImageView),
                                                                                              0,
                                                                                              BindingMode.OneWay,
                                                                                              null,
                                                                                              InvalidatePropertyChanged);

        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor),
                                                                                              typeof(Color),
                                                                                              typeof(CircledImageView),
                                                                                              Color.White,
                                                                                              BindingMode.OneWay,
                                                                                              null,
                                                                                              InvalidatePropertyChanged);

        private static SKSvg DefaultImage;

        public SKBitmapImageSource Source
        {
            get { return (SKBitmapImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public int BorderWidth
        {
            get { return (int)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        static CircledImageView()
        {
            IResourceLocator resLocator = Mvx.Resolve<IResourceLocator>();
            string resPath = resLocator.GetResourcePath(ResourceKeys.ImagesKey, "symbol_logo.svg");

            DefaultImage = new SKSvg();
            DefaultImage.Load(resLocator.ResourcesAssembly.GetManifestResourceStream(resPath));
        }

        public CircledImageView()
        {
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            float centerX = (float)Bounds.Width * 0.5f;
            float centerY = (float)Bounds.Height * 0.5f;
            float imageSize = Math.Min(centerX, centerY);
            float borderRadius = imageSize - BorderWidth * 0.5f;

            args.Surface.Canvas.Clear(SKColor.Empty);
            args.Surface.Canvas.Scale(CanvasSize.Width / (float)Width, CanvasSize.Height / (float)Height);
            canvas.ClipPath(ClippingPath);

            if (Source != null && Source.Bitmap != null)
            {
                float scaleFactor = canvas.TotalMatrix.ScaleX;
                float nativeImageSize = imageSize * scaleFactor;
                SKRect bitmapBounds = SKRect.Create(0, 0, Source.Bitmap.Width, Source.Bitmap.Height);
                SKRect destinationBounds = SKRect.Create((float)(Width - nativeImageSize) * 0.5f,
                                                         (float)(Height - nativeImageSize) * 0.5f,
                                                         nativeImageSize,
                                                         nativeImageSize);

                canvas.DrawBitmap(Source.Bitmap, bitmapBounds, destinationBounds);
            }
            else
            {
                var matrix = GetFillMatrix(DefaultImage.Picture.CullRect);

                canvas.DrawPicture(DefaultImage.Picture, ref matrix);
            }

            if (BorderWidth > 0 && BorderColor != Color.Transparent)
            {
                SKPaint borderPaint = new SKPaint
                                      {
                                          Style = SKPaintStyle.Stroke,
                                          Color = ColorExtensions.ToSKColor(BorderColor),
                                          StrokeWidth = BorderWidth,
                                          IsAntialias = true,
                                      };

                using (borderPaint)
                {
                    canvas.DrawCircle(centerX, centerY, borderRadius, borderPaint);
                }
            }
        }

        private SKMatrix GetFillMatrix(SKRect sourceRect)
        {
            try
            {
                float xScale = (float)Width / sourceRect.Width;
                float yScale = (float)Height / sourceRect.Height;
                float fillScale = Math.Min(xScale, yScale);

                SKMatrix initialTranslate = SKMatrix.MakeTranslation(sourceRect.Width * -0.5f,
                                                                     sourceRect.Height * -0.5f);
                SKMatrix finalTranslate = SKMatrix.MakeTranslation((float)Width * 0.5f,
                                                                   (float)Height * 0.5f);
                SKMatrix scale = SKMatrix.MakeScale(fillScale, fillScale);
                SKMatrix result = SKMatrix.MakeIdentity();

                SKMatrix.Concat(ref result, result, finalTranslate);
                SKMatrix.Concat(ref result, result, scale);
                SKMatrix.Concat(ref result, result, initialTranslate);

                return  result;
            }
            catch (Exception)
            {
                return SKMatrix.MakeIdentity();
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(Width) ||
                propertyName == nameof(Height))
            {
                RefreshClippingMask();
            }
        }

        private void RefreshClippingMask()
        {
            float centerX = (float)Bounds.Width * 0.5f;
            float centerY = (float)Bounds.Height * 0.5f;
            float radius = Math.Min(centerX, centerY);

            SKPath circlePath = new SKPath();
            circlePath.AddCircle(centerX, centerY, radius);

            ClippingPath = circlePath;
        }

        public SKPath ClippingPath { get; set; }

        private static void InvalidatePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            CircledImageView view = bindable as CircledImageView;

            view.InvalidateSurface();
        }
    }
}
