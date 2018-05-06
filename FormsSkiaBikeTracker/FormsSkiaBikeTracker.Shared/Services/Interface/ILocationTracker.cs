using System;
using MvvmCross.Plugins.Location;

namespace FormsSkiaBikeTracker.Services.Interface
{
    public interface ILocationTracker
    {
        bool IsTracking { get; }
        MvxGeoLocation Location { get; }
        IMvxLocationWatcher LocationService { get; set; }

        event EventHandler<MvxGeoLocation> Moved;

        void Pause();
        void Start(int refreshMovementMeters = 30, int refreshSeconds = 15, bool foreground = true);
    }
}