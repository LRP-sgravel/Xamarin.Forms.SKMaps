// **********************************************************************
// 
//   SKMapPosition.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

namespace Xamarin.Forms.Maps.Overlays.Models
{
    public class SKMapPosition
    {
        // Just like the Xamarin Forms Maps Position, but it doesn't clamp at the 180th meridian, allowing us to wrap around the globe
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public SKMapPosition(Position position)
        {
            Latitude = position.Latitude;
            Longitude = position.Longitude;
        }

        public SKMapPosition(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public Position ToPosition()
        {
            return new Position(Latitude, Longitude);
        }
    }
}
