using CoreLocation;
using MapKit;
using Xamarin.Forms.SKMaps.Platforms.Ios.Extensions;

namespace Xamarin.Forms.SKMaps.Platforms.Ios.UI.Renderers
{
    internal class SKPinAnnotation : MKPointAnnotation
    {
        public SKPin SharedPin { get; }

        public SKPinAnnotation(SKPin pin)
        {
            SharedPin = pin;

            Title = pin.Label;
            Subtitle = pin.Address;
            Coordinate = pin.Position.ToLocationCoordinate();
        }

        public override string Title
        {
            get => base.Title;
            set
            {
                if (Title != value)
                {
                    string titleKey = nameof(Title).ToLower();

                    WillChangeValue(titleKey);
                    base.Title = value;
                    DidChangeValue(titleKey);
                }
            }
        }

        public override string Subtitle
        {
            get => base.Subtitle;
            set
            {
                if (Subtitle != value)
                {
                    string subtitleKey = nameof(Subtitle).ToLower();

                    WillChangeValue(subtitleKey);
                    base.Subtitle = value;
                    DidChangeValue(subtitleKey);
                }
            }
        }

        public override CLLocationCoordinate2D Coordinate
        {
            get => base.Coordinate;
            set
            {
                if (Coordinate.Latitude != value.Latitude ||
                    Coordinate.Longitude != value.Longitude)
                {
                    string coordinateKey = nameof(Coordinate).ToLower();

                    WillChangeValue(coordinateKey);
                    base.Coordinate = value;
                    DidChangeValue(coordinateKey);
                }
            }
        }
    }
}
