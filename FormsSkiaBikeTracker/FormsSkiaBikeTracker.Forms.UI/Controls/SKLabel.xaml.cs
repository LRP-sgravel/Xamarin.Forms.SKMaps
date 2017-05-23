using System.IO;
using LRPLib.Services.Resources;
using LRPLib.Views.XForms.Extensions;
using MvvmCross.Platform;
using SkiaSharp;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Controls
{
    public partial class SKLabel
    {
        public static readonly BindableProperty FontResourcePathProperty = BindableProperty.Create(nameof(FontResourcePath), typeof(string), typeof(SKLabel), string.Empty, BindingMode.OneWay, null, FontResourcePathPropertyChanged);
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(SKLabel), string.Empty, BindingMode.OneWay, null, ResizePropertyChanged);
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(SKLabel), Color.Black, BindingMode.OneWay, null, InvalidatePropertyChanged);
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(SKLabel), 14.0, BindingMode.OneWay, null, ResizePropertyChanged);

        public string FontResourcePath
        {
            get { return (string)GetValue(FontResourcePathProperty); }
            set { SetValue(FontResourcePathProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        private SKTypeface _Typeface { get; set; }

        public SKLabel()
        {
            InitializeComponent();
        }
        ~SKLabel()
        {
            _Typeface?.Dispose();
        }

        private static void FontResourcePathPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            SKLabel view = bindable as SKLabel;
            IResourceLocator resLocator = Mvx.Resolve<IResourceLocator>();
            string resourcePath = resLocator.GetResourcePath("Fonts", view.FontResourcePath);
            Stream resStream = resLocator.ResourcesAssembly.GetManifestResourceStream(resourcePath);

            if (resStream != null)
            {
                using (resStream)
                {
                    view._Typeface = SKTypeface.FromStream(resStream);
                }
            }
            else
            {
                view._Typeface = null;
            }

            ResizePropertyChanged(bindable, oldvalue, newvalue);
        }
        
        private static void InvalidatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SKLabel view = bindable as SKLabel;

            view.Invalidate();
        }
                
        private static void ResizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SKLabel view = bindable as SKLabel;

            view.InvalidateMeasure();
            view.Invalidate();
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (HasSomethingToDraw())
            {
                SKPaint paint = new SKPaint();

                using (paint)
                {
                    SKRect textBounds = new SKRect();

                    paint.Typeface = _Typeface;
                    paint.TextSize = (float)FontSize;
                    paint.MeasureText(Text, ref textBounds);

                    return new SizeRequest(new Size(textBounds.Width, textBounds.Height + paint.FontMetrics.Bottom));
                }
            }

            return new SizeRequest();
        }

        protected override void Paint(SKCanvas canvas)
        {
            if (HasSomethingToDraw())
            {
                SKPaint paint = new SKPaint();

                using (paint)
                {
                    paint.Typeface = _Typeface;
                    paint.Color = TextColor.ToSKColor();
                    paint.TextSize = (float)FontSize;

                    canvas.DrawText(Text, 0, (float)Height - paint.FontMetrics.Bottom, paint);
                }
            }
        }

        private bool HasSomethingToDraw()
        {
            return _Typeface != null && !string.IsNullOrEmpty(Text) && TextColor != Color.Transparent && FontSize > 0;
        }
    }
}
