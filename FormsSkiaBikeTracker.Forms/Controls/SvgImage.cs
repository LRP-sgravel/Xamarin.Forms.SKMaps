using FormsSkiaBikeTracker.Forms.Services.SvgSourceHandlers;
using FormsSkiaBikeTracker.Services.Interface;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace FormsSkiaBikeTracker.Forms.Controls
{
    public class SvgImage : SKCanvasView
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(SvgImage));
        public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(SvgImage), Aspect.AspectFit, BindingMode.OneWay, null, TransformUpdatePropertyChanged);
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(SvgImage), Color.Transparent);

        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public Aspect Aspect
        {
            get => (Aspect)GetValue(AspectProperty);
            set => SetValue(AspectProperty, value);
        }

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private SKSvg _SvgCanvas { get; set; }
        private bool _TransformIsDirty { get; set; } = true;
        private SKMatrix _Transform { get; set; }

        private static void TransformUpdatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SvgImage view = bindable as SvgImage;

            view.MarkAsDirty();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            if (IsSetupToDraw())
            {
                SKCanvas canvas = args.Surface.Canvas;
                SKMatrix picMatrix = new SKMatrix();
                SKPaint paint = new SKPaint();
                float scale = args.Info.Width / (float)Width;

                if (_TransformIsDirty)
                {
                    UpdateTransform();
                }

                SKMatrix.Concat(ref picMatrix, canvas.TotalMatrix, _Transform);
                SKMatrix.Concat(ref picMatrix, SKMatrix.MakeScale(scale, scale), picMatrix);

                if (Color != Color.Transparent)
                {
                    paint.ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn);
                }

                canvas.Clear();
                canvas.Save();
                canvas.SetMatrix(picMatrix);
                canvas.DrawPicture(_SvgCanvas.Picture, paint);
                canvas.Restore();
            }
        }

        protected override async void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == SourceProperty.PropertyName)
            {
                ISvgSourceHandler handler = GetSvgImageHandler(Source.GetType());

                _SvgCanvas = await handler.LoadImageAsync(Source);
                MarkAsDirty();
            }
            else if (propertyName == HeightProperty.PropertyName ||
                     propertyName == WidthProperty.PropertyName)
            {
                MarkAsDirty();
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (_SvgCanvas == null || widthConstraint == 0 || heightConstraint == 0)
            {
                return new SizeRequest();
            }
            else
            {
                SizeRequest desiredSize = new SizeRequest(new Size(_SvgCanvas.CanvasSize.Width, _SvgCanvas.CanvasSize.Height));
                double desiredWidth = desiredSize.Request.Width;
                double desiredHeight = desiredSize.Request.Height;
                double desiredAspect = desiredSize.Request.Width / desiredSize.Request.Height;
                double constraintAspect = widthConstraint / heightConstraint;
                double width = desiredWidth;
                double height = desiredHeight;

                if (constraintAspect > desiredAspect)
                {
                    // constraint area is proportionally wider than image
                    switch (Aspect)
                    {
                        case Aspect.AspectFit:
                        case Aspect.AspectFill:
                            height = Math.Min(desiredHeight, heightConstraint);
                            width = desiredWidth * (height / desiredHeight);
                            break;
                        case Aspect.Fill:
                            width = Math.Min(desiredWidth, widthConstraint);
                            height = desiredHeight * (width / desiredWidth);
                            break;
                    }
                }
                else if (constraintAspect < desiredAspect)
                {
                    // constraint area is proportionally taller than image
                    switch (Aspect)
                    {
                        case Aspect.AspectFit:
                        case Aspect.AspectFill:
                            width = Math.Min(desiredWidth, widthConstraint);
                            height = desiredHeight * (width / desiredWidth);
                            break;
                        case Aspect.Fill:
                            height = Math.Min(desiredHeight, heightConstraint);
                            width = desiredWidth * (height / desiredHeight);
                            break;
                    }
                }
                else
                {
                    // constraint area is same aspect as image
                    width = Math.Min(desiredWidth, widthConstraint);
                    height = desiredHeight * (width / desiredWidth);
                }

                return new SizeRequest(new Size(width, height));
            }
        }

        private void MarkAsDirty()
        {
            _TransformIsDirty = true;
            InvalidateSurface();
        }

        protected virtual ISvgSourceHandler GetSvgImageHandler(Type type)
        {
            ISvgSourceHandler result = null;

            if (type == typeof(UriImageSource))
            {
                result = new UriSvgSourceHandler();
            }
            else if (type == typeof(FileImageSource))
            {
                result = new FileSvgSourceHandler();
            }
            else if (type == typeof(StreamImageSource))
            {
                result = new StreamSvgSourceHandler();
            }

            return result;
        }

        private bool IsSetupToDraw()
        {
            return _SvgCanvas != null && _SvgCanvas.Picture != null &&
                   _SvgCanvas.Picture.CullRect.Width > 0 && _SvgCanvas.Picture.CullRect.Height > 0 &&
                   Width > 0 && Height > 0;
        }

        private void UpdateTransform()
        {
            try
            {
                double xScale = Width / _SvgCanvas.Picture.CullRect.Width;
                double yScale = Height / _SvgCanvas.Picture.CullRect.Height;

                switch (Aspect)
                {
                    case Aspect.AspectFill:
                        {
                            double fillScale = Math.Max(xScale, yScale);

                            xScale = fillScale;
                            yScale = fillScale;
                            break;
                        }
                    case Aspect.AspectFit:
                        {
                            double fitScale = Math.Min(xScale, yScale);

                            xScale = fitScale;
                            yScale = fitScale;
                            break;
                        }
                }

                SKMatrix initialTranslate = SKMatrix.MakeTranslation(_SvgCanvas.Picture.CullRect.Width * -0.5f,
                                                                     _SvgCanvas.Picture.CullRect.Height * -0.5f);
                SKMatrix finalTranslate = SKMatrix.MakeTranslation((float)Width * 0.5f,
                                                                   (float)Height * 0.5f);
                SKMatrix scale = SKMatrix.MakeScale((float)xScale, (float)yScale);
                SKMatrix result = SKMatrix.MakeIdentity();

                SKMatrix.Concat(ref result, result, finalTranslate);
                SKMatrix.Concat(ref result, result, scale);
                SKMatrix.Concat(ref result, result, initialTranslate);

                _Transform = result;
                _TransformIsDirty = false;
            }
            catch (Exception)
            {
                _Transform = SKMatrix.MakeIdentity();
            }
        }
    }
}