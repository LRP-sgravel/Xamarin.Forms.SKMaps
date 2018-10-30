// **********************************************************************
// 
//   MutableActivityRoute.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System;
using System.Collections.Generic;
using Xamarin.Forms.SKMaps.Sample.Services.Interface;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.SKMaps.Sample.Models
{
    public class MutableRoute : IMutableRoute
    {
        public event EventHandler<PointsAddedEventArgs> PointsAdded;

        private List<Position> _routePoints = new List<Position>();
        public IEnumerable<Position> Points => _routePoints;

        public MutableRoute()
        {
        }

        public MutableRoute(IEnumerable<Position> routePoints)
        {
            this._routePoints = new List<Position>(routePoints);
        }

        public void AddPoint(Position point)
        {
            _routePoints.Add(point);
            PointsAdded?.Invoke(this,
                                new PointsAddedEventArgs
                                {
                                    NewPoints = new List<Position>(new[] { point })
                                });
        }

        public void AddPoints(IEnumerable<Position> points)
        {
            _routePoints.AddRange(points);
            PointsAdded?.Invoke(this,
                                new PointsAddedEventArgs
                                {
                                    NewPoints = new List<Position>(points)
                                });
        }
    }
}
