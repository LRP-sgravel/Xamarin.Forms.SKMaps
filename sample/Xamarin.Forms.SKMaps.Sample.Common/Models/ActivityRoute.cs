// **********************************************************************
// 
//   ActivityRoute.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using Realms;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.SKMaps.Sample.Models
{
    public class ActivityRoute : RealmObject, IRoute
    {
        public IList<RoutePoint> RealmPoints { get; }

        public IEnumerable<Position> Points => RealmPoints.Select(r => r.ToPosition());
    }
}
