// **********************************************************************
// 
//   Athlete.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using System.Collections.Generic;
using Realms;
using Xamarin.Forms.SKMaps.Models;

namespace Xamarin.Forms.SKMaps.Sample.Models
{
    public class Athlete : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string PicturePath { get; set; }
        public int DistanceUnitStored { get; set; }
        public IList<Activity> Activities { get; }

        [Ignored]
        public DistanceUnit DistanceUnit
        {
            get => (DistanceUnit)DistanceUnitStored;
            set => DistanceUnitStored = (int)value;
        }
    }
}
