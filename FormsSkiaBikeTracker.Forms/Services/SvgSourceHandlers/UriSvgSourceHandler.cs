using FormsSkiaBikeTracker.Services.Interface;
using SkiaSharp.Extended.Svg;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.Services.SvgSourceHandlers
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
