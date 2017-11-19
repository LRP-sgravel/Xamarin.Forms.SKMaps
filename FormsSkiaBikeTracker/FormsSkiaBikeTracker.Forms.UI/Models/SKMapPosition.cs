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

namespace FormsSkiaBikeTracker.Forms.UI.Pages
{
    public class SKMapPosition
    {
        // Just like the Xamarin Forms Maps Position, but it doesn't clamp at the 180th meridian, allowing us to wrap around the globe
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public SKMapPosition(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
