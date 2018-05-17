// **********************************************************************
// 
//   ActivityStatistics.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using Realms;

namespace FormsSkiaBikeTracker.Models
{
    public class ActivityStatistics : RealmObject
    {
        [PrimaryKey]
        public long Id { get; set; }
        public Activity Activity { get; set; }
    }
}
