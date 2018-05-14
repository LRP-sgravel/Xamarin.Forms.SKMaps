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

using Realms;

namespace FormsSkiaBikeTracker.Models
{
    public class Athlete : RealmObject
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string PicturePath { get; set; }
    }
}
