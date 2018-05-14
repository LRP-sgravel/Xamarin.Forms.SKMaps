// **********************************************************************
// 
//   AppInitializer.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************
using Xamarin.UITest;

namespace FormsSkiaBikeTracker.Test
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform, string deviceId)
        {
            if (platform == Platform.Android)
            {
                return ConfigureApp.Android
                                   .ApkFile("../../../FormsSkiaBikeTracker.Droid/bin/Release/FormsSkiaBikeTracker.Droid-Signed.apk")
                                   .DeviceSerial(deviceId)
                                   .EnableLocalScreenshots()
                                   .StartApp();
            }

            return ConfigureApp.iOS
                               .AppBundle("../../../FormsSkiaBikeTracker.iOS/bin/iPhoneSimulator/Release/FormsSkiaBikeTracker.iOS.app")
                               .DeviceIdentifier(deviceId)
                               .EnableLocalScreenshots()
                               .StartApp();
        }
    }
}

