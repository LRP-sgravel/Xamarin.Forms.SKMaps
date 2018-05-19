// **********************************************************************
// 
//   Activity.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System;
using Realms;

namespace FormsSkiaBikeTracker.Models
{
    public class Activity : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset StartTime { get; set; }
        public ActivityRoute Route { get; set; }
        public ActivityStatistics Statistics { get; set; }
    }
}
