// **********************************************************************
// 
//   IRoute.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Services.Interface
{
    public interface IRoute
    {
        IEnumerable<Position> Points { get; }
    }
}
