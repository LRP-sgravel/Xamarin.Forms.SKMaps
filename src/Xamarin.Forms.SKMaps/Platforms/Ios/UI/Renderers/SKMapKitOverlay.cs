// **********************************************************************
// 
//   SkiaMapOverlay.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using CoreLocation;
using MapKit;
using Xamarin.Forms.SKMaps.Platforms.Ios.Extensions;

namespace Xamarin.Forms.SKMaps.Platforms.Ios.UI.Renderers
{
    internal class SKMapKitOverlay : MKOverlay
    {
        public override CLLocationCoordinate2D Coordinate => SharedOverlay.GpsBounds.Center.ToLocationCoordinate();
        public override MKMapRect BoundingMapRect => SharedOverlay.GpsBounds.ToMapRect();

        public SKMapOverlay SharedOverlay { get; }
        private SKMap _SharedControl { get; }

        public SKMapKitOverlay(SKMapOverlay sharedOverlay, SKMap sharedControl)
        {
            SharedOverlay = sharedOverlay;
            _SharedControl = sharedControl;
        }
    }
}
