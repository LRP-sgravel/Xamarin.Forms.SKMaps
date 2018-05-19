// **********************************************************************
// 
//   DistanceExtensions.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using Xamarin.Forms.Maps.Overlays.Models;

namespace Xamarin.Forms.Maps.Overlays.Extensions
{
    public static class DistanceExtensions
    {
        public static double ToDistanceUnit(this Distance self, DistanceUnit unit)
        {
            switch (unit)
            {
                case DistanceUnit.Kilometer:
                    return self.Kilometers;
                case DistanceUnit.Meters:
                    return self.Meters;
                case DistanceUnit.Miles:
                    return self.Miles;
            }

            return 0;
        }
    }
}
