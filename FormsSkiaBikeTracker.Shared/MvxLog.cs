// **********************************************************************
// 
//   MvxLog.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2018, Sylvain Gravel
// 
// ***********************************************************************

using MvvmCross.Logging;
using MvxEntry = MvvmCross.Mvx;

namespace FormsSkiaBikeTracker
{
    public class MvxLog
    {
        public static IMvxLog Instance => MvxEntry.Resolve<IMvxLogProvider>()
                                                  .GetLogFor<MvxLog>();
    }
}
