using System;
using System.Threading.Tasks;
using MvvmCross.Plugin.Location;

namespace FormsSkiaBikeTracker.Services.Interface
{
    public class LocationMovedEventArgs : EventArgs
    {
        public MvxGeoLocation Location { get; set; }
    }

    public interface ILocationTracker
    {
        bool IsTracking { get; }
        MvxGeoLocation Location { get; }
        IMvxLocationWatcher LocationService { get; set; }

        event EventHandler<LocationMovedEventArgs> Moved;

        void Pause();
        Task Start(int refreshMovementMeters = 30, int refreshSeconds = 15, bool foreground = true);
    }
}