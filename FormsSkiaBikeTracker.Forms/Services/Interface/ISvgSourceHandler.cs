using SkiaSharp.Extended.Svg;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Services.Interface
{
    public interface ISvgSourceHandler : IRegisterable
    {
        Task<SKSvg> LoadImageAsync(ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken));
    }
}
