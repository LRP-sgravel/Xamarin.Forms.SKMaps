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

using FormsSkiaBikeTracker.Forms.UI.Helpers;
using MapKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.iOS.Helpers
{
    public static class MapSpanExtension
    {
        public static MKMapRect ToMapRect(this MapSpan self)
        {
            Rectangle mercator = self.ToMercator();

            return mercator.ToMapRect();
        }

        public static MapSpan ToMapSpan(this MKMapRect self)
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
