using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using SkiaSharp.Extended.Svg;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin.Forms.SKMaps.Sample.Forms.Services.SvgSourceHandlers
{
    public class StreamSvgSourceHandler : ISvgSourceHandler
    {
        public Task<SKSvg> LoadImageAsync(ImageSource imageSource, CancellationToken cancellationToken = new CancellationToken())
        {
            SKSvg loader = new SKSvg();
            StreamImageSource source = imageSource as StreamImageSource;

            return Task.Run(async () =>
                {
                    Stream stream = await ((IStreamImageSource)source).GetStreamAsync(cancellationToken);

                    if (stream != null)
                    {
                        loader.Load(stream);
                    }

                    return loader;
                },
                cancellationToken);
        }
    }
}
