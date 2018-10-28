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
using FormsSkiaBikeTracker.Services.Interface;
using MvvmCross;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace FormsSkiaBikeTracker.Forms.Controls
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
            get => (SKBitmapImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public int BorderWidth
        {
            get => (int)GetValue(BorderWidthProperty);
            set => SetValue(BorderWidthProperty, value);
        }

        private SKPath ClippingPath { get; set; }

        static CircledImageView()
        {
            IResourceLocator resLocator = Mvx.Resolve<IResourceLocator>();
            string resPath = resLocator.GetResourcePath(ResourceKeys.ImagesKey, "symbol_logo.svg");

            DefaultImage = new SKSvg();
            DefaultImage.Load(resLocator.ResourcesAssembly.GetManifestResourceStream(resPath));
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            float circleDiameter = (float)Math.Min(Width, Height);

            canvas.Scale(args.Info.Width / (float)Width);
            canvas.Clear();

            if (ClippingPath != null)
            {
                canvas.ClipPath(ClippingPath);
            }

            if (Source != null && Source.Bitmap != null)
            {
                double fillScale = GetFillScale(new SKSize(Source.Bitmap.Width, Source.Bitmap.Height), new SKSize(circleDiameter, circleDiameter));
                double destinationWidth = Source.Bitmap.Width * fillScale;
                double destinationHeight = Source.Bitmap.Height * fillScale;
                SKRect bitmapBounds = SKRect.Create(0, 0, Source.Bitmap.Width, Source.Bitmap.Height);
                SKRect destinationBounds = SKRect.Create((float)(Width - destinationWidth) * 0.5f,
                                                         (float)(Height - destinationHeight) * 0.5f,
                                                         (float)destinationWidth,
                                                         (float)destinationHeight);

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
                                          Color = BorderColor.ToSKColor(),
                                          StrokeWidth = BorderWidth,
                                          IsAntialias = true,
                                      };

                using (borderPaint)
                {
                    float centerX = (float)Width * 0.5f;
                    float centerY = (float)Height * 0.5f;
                    float borderRadius = (circleDiameter - BorderWidth) * 0.5f;

                    canvas.DrawCircle(centerX, centerY, borderRadius, borderPaint);
                }
            }
        }

        private SKMatrix GetFillMatrix(SKRect sourceRect)
        {
            try
            {
                float circleDiameter = (float)Math.Min(Width, Height);
                float fillScale = GetFillScale(sourceRect.Size, new SKSize(circleDiameter, circleDiameter));
                SKMatrix initialTranslate = SKMatrix.MakeTranslation(sourceRect.Width * -0.5f,
                                                                     sourceRect.Height * -0.5f);
                SKMatrix finalTranslate = SKMatrix.MakeTranslation((float)Width * 0.5f,
                                                                   (float)Height * 0.5f);
                SKMatrix scale = SKMatrix.MakeScale(fillScale, fillScale);
                SKMatrix result = SKMatrix.MakeIdentity();

                SKMatrix.Concat(ref result, finalTranslate, scale);
                SKMatrix.Concat(ref result, result, initialTranslate);

                return result;
            }
            catch (Exception)
            {
                return SKMatrix.MakeIdentity();
            }
        }

        private float GetFillScale(SKSize source, SKSize destination)
        {
            float xScale = destination.Width / source.Width;
            float yScale = destination.Height / source.Height;

            return Math.Max(xScale, yScale);
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

        private static void InvalidatePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            CircledImageView view = bindable as CircledImageView;

            view.InvalidateSurface();
        }
    }
}
