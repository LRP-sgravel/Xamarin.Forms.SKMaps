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

using System.Runtime.CompilerServices;
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

        public static Distance Add(this Distance self, Distance addition)
        {
            return Distance.FromMeters(self.Meters + addition.Meters);
        }
    }
}
