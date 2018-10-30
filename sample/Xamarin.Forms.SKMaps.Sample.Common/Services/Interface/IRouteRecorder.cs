// **********************************************************************
// 
//   IRouteRecorder.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

namespace Xamarin.Forms.SKMaps.Sample.Services.Interface
{
    public interface IRouteRecorder
    {
        bool IsActive { get; }
        IMutableRoute ActiveRecording { get; }

        IMutableRoute Start();
        void Stop();
    }
}
