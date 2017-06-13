// **********************************************************************
// 
//   DrawableMapOverlay.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Sylvain Gravel
// 
// ***********************************************************************

using LRPLib.Views.XForms;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Controls
{
    public abstract class DrawableMapOverlay : DrawableView
    {
        public abstract Rectangle GpsBounds { get; }
    }
}
