using FormsSkiaBikeTracker.Services.Interface;
using SkiaSharp.Extended.Svg;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.Services.SvgSourceHandlers
{
    public class FileSvgSourceHandler : ISvgSourceHandler
    {
        public Task<SKSvg> LoadImageAsync(ImageSource imageSource, CancellationToken cancellationToken = new CancellationToken())
        {
            SKSvg loader = new SKSvg();
            FileImageSource source = imageSource as FileImageSource;

            return Task.Run(() =>
                {
                    loader.Load(source.File);

                    return loader;
                },
                cancellationToken);
        }
    }
}
