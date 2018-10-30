// **********************************************************************
// 
//   RealmGpsCoordinates.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using Realms;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.SKMaps.Sample.Models
{
    public class RoutePoint : RealmObject
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Position ToPosition()
        {
            return new Position(Latitude, Longitude);
        }
    }
}
