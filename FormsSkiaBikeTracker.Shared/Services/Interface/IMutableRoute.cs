// **********************************************************************
// 
//   IMutableRoute.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System;
using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace FormsSkiaBikeTracker.Services.Interface
{
    public class PointsAddedEventArgs : EventArgs
    {
        public IEnumerable<Position> NewPoints { get; internal set; }
    }

    public interface IMutableRoute : IRoute
    {
        event EventHandler<PointsAddedEventArgs> PointsAdded;

        void AddPoint(Position point);
        void AddPoints(IEnumerable<Position> points);
    }
}
