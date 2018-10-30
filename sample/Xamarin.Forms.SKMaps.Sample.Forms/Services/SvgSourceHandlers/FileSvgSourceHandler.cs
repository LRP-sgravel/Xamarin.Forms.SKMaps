using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using SkiaSharp.Extended.Svg;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin.Forms.SKMaps.Sample.Forms.Services.SvgSourceHandlers
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
