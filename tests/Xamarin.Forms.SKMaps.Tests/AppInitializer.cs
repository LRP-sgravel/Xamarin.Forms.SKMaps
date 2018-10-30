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

namespace Xamarin.Forms.SKMaps.Sample.Test
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform, string deviceId)
        {
            if (platform == Platform.Android)
            {
                return ConfigureApp.Android
                                   .ApkFile("../../../Xamarin.Forms.SKMaps.Sample.Droid/bin/Release/Xamarin.Forms.SKMaps.Sample.Droid-Signed.apk")
                                   .DeviceSerial(deviceId)
                                   .EnableLocalScreenshots()
                                   .StartApp();
            }

            return ConfigureApp.iOS
                               .AppBundle("../../../Xamarin.Forms.SKMaps.Sample.iOS/bin/iPhoneSimulator/Release/Xamarin.Forms.SKMaps.Sample.iOS.app")
                               .DeviceIdentifier(deviceId)
                               .EnableLocalScreenshots()
                               .StartApp();
        }
    }
}

