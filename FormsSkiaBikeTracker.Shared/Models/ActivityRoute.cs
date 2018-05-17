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
using FormsSkiaBikeTracker.Services.Interface;
using Realms;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Models
{
    public class ActivityRoute : RealmObject, IRoute
    {
        [PrimaryKey]
        public long Id { get; set; }
        public IList<RoutePoint> RealmPoints { get; }

        public IEnumerable<Position> Points => RealmPoints.Select(r => r.ToPosition());
    }
}
