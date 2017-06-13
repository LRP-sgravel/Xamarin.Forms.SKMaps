// **********************************************************************
// 
//   PointExtension.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using CoreLocation;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.iOS.Helpers
{
    public static class PointExtension
    {
        public static CLLocationCoordinate2D ToLocationCoordinate(this Point self)
        {
            return new CLLocationCoordinate2D(self.X, self.Y);
        }
    }
}
