// **********************************************************************
// 
//   MapSpanExtension.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using MapKit;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;

namespace Xamarin.Forms.Maps.Overlays.Platforms.Ios.Extensions
{
    internal static class MapSpanExtension
    {
        public static MKMapRect ToMapRect(this MapSpan self)
        {
            Rectangle mercator = self.ToMercator();

            return mercator.ToMapRect();
        }

        public static SKMapSpan ToMapSpan(this MKMapRect self)
        {
            Rectangle mercator = new Rectangle(self.MinX, self.MinY, self.Width, self.Height);

            return mercator.ToGps();
        }

        public static Rectangle ToRectangle(this MKMapRect self)
        {
            return new Rectangle(self.MinX, self.MinY, self.Width, self.Height);
        }

        public static MKMapRect ToMapRect(this Rectangle self)
        {
            return new MKMapRect(new MKMapPoint(self.X, self.Y), new MKMapSize(self.Width, self.Height));
        }
    }
}
