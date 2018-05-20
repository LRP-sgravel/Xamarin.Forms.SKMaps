// **********************************************************************
// 
//   ResizableViewCell.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using System;
using MvvmCross.WeakSubscription;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.Controls
{
    public class ResizableViewCell : ViewCell
    {
        private IDisposable _contentMeasureInvalidatedSubscription;

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(View))
            {
                _contentMeasureInvalidatedSubscription = View?.WeakSubscribe(nameof(View.MeasureInvalidated),
                                                                             ContentViewMeasureInvalidated);
            }
        }

        private void ContentViewMeasureInvalidated(object sender, EventArgs eventArgs)
        {
            ForceUpdateSize();
        }
    }
}
