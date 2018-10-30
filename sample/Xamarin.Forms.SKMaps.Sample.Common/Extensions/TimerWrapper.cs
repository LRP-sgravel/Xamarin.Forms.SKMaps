// **********************************************************************
// 
//   TimerWrapper.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using System;
using System.Timers;

namespace Xamarin.Forms.SKMaps.Sample.Extensions
{
    public class TimerWrapper : IDisposable
    {
        public event EventHandler<ElapsedEventArgs> Elapsed;

        public double IntervalMs
        {
            get => _Timer.Interval;
            set => _Timer.Interval = value;
        }

        public bool AutoReset
        {
            get => _Timer.AutoReset;
            set => _Timer.AutoReset = value;
        }

        private Timer _Timer { get; } = new Timer();

        public TimerWrapper()
        {
            _Timer.Elapsed += OnTimerElapsed;
        }

        public TimerWrapper(double intervalMs) : this()
        {
            IntervalMs = intervalMs;
        }

        public void Start() => _Timer.Start();
        public void Stop() => _Timer.Stop();

        private void OnTimerElapsed(object sender, ElapsedEventArgs args)
        {
            Elapsed?.Invoke(sender, args);
        }

        public void Dispose()
        {
            _Timer.Elapsed -= OnTimerElapsed;
            _Timer?.Dispose();
        }
    }
}
