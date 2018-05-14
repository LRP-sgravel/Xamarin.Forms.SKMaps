// **********************************************************************
// 
//   Tests.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************
using NUnit.Framework;
using Xamarin.UITest;

namespace FormsSkiaBikeTracker.Test
{
    [TestFixture(Platform.Android, "emulator-5554")]
    //    [TestFixture(Platform.Android, "8825ea1004cf6196")]
    [TestFixture(Platform.iOS, "CC1A7354-8452-4C33-9BF3-453905AC0456")]
    //    [TestFixture(Platform.iOS, "DC95AAB0-8EBB-4B45-B7C0-C020D9BD00C3")]
    public class Tests
    {
        IApp app;
        private Platform _Platform;
        private string _DeviceId;

        public Tests(Platform platform, string deviceId)
        {
            _Platform = platform;
            _DeviceId = deviceId;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(_Platform, _DeviceId);
        }

        [Test]
        public void AppLaunches()
        {
            app.Screenshot("First screen.");
        }
    }
}

