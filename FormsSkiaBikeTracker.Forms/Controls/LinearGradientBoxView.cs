using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.Controls
{
    public enum GradientDirection
    {
        LeftToRight,
        TopToBottom
    }

    public class LinearGradientBoxView : SKCanvasView
    {
        public static readonly BindableProperty DirectionProperty = BindableProperty.Create("Direction", typeof(GradientDirection), typeof(LinearGradientBoxView), GradientDirection.LeftToRight);
        public static readonly BindableProperty StartColorProperty = BindableProperty.Create("StartColor", typeof(Color), typeof(LinearGradientBoxView), Color.Default);
        public static readonly BindableProperty EndColorProperty = BindableProperty.Create("EndColor", typeof(Color), typeof(LinearGradientBoxView), Color.Transparent);

        public GradientDirection Direction
        {
            get => (GradientDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public Color StartColor
        {
            get => (Color)GetValue(StartColorProperty);
            set => SetValue(StartColorProperty, value);
        }

        public Color EndColor
        {
            get => (Color)GetValue(EndColorProperty);
            set => SetValue(EndColorProperty, value);
        }

        public SKPath ClippingPath { get; set; }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            SKColor startColor = StartColor.ToSKColor();
            SKColor endColor = EndColor.ToSKColor();
            SKPoint endPoint = new SKPoint(args.Info.Width, 0);

            if (Direction == GradientDirection.TopToBottom)
            {
                endPoint = new SKPoint(0, args.Info.Height);
            }

            using (SKShader gradientShader = SKShader.CreateLinearGradient(new SKPoint(0, 0),
                                                                           endPoint,
                                                                           new[] { startColor, endColor },
                                                                           null,
                                                                           SKShaderTileMode.Clamp))
            using (SKPaint paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Shader = gradientShader;

                canvas.DrawPaint(paint);
            }

            if (ClippingPath != null)
            {
                canvas.ClipPath(ClippingPath);
            }
        }
    }
}
