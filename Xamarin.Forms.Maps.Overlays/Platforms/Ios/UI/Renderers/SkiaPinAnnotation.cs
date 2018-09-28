using MapKit;
using Xamarin.Forms.Maps.Overlays.Platforms.Ios.Extensions;

namespace Xamarin.Forms.Maps.Overlays.Platforms.Ios.UI.Renderers
{
    internal class SkiaPinAnnotation : MKPointAnnotation
    {
        public SKPin SharedPin { get; }

        public SkiaPinAnnotation(SKPin pin)
        {
            SharedPin = pin;

            Title = pin.Label;
            Subtitle = pin.Address;
            SetCoordinate(pin.Position.ToLocationCoordinate());
        }
    }
}
