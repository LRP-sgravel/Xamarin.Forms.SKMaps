using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using SkiaSharp.Extended.Svg;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin.Forms.SKMaps.Sample.Forms.Services.SvgSourceHandlers
{
    public class UriSvgSourceHandler : ISvgSourceHandler
    {
        public async Task<SKSvg> LoadImageAsync(ImageSource imageSource,
                                                CancellationToken cancellationToken = new CancellationToken())
        {
            SKSvg loader = new SKSvg();
            UriImageSource source = imageSource as UriImageSource;
            HttpClient client = new HttpClient();

            using (client)
            {
                HttpResponseMessage response = await client.GetAsync(source.Uri, cancellationToken)
                                                           .ConfigureAwait(false);
                Stream stream = await response.Content.ReadAsStreamAsync()
                                              .ConfigureAwait(false);

                using (stream)
                {
                    if (stream != null)
                    {
                        loader.Load(stream);
                    }
                }
            }

            return loader;
        }
    }
}
