// **********************************************************************
// 
//   RedBorderEffect.cs
//   
//   This file is subject to the terms and conditions defined in
//   file 'LICENSE.txt', which is part of this source code package.
//   
//   Copyright (c) 2017, Le rond-point
// 
// ***********************************************************************

using Xamarin.Forms;

namespace Xamarin.Forms.SKMaps.Sample.Forms.UI.Effects
{
    public class InvalidEntryBehavior : Behavior<Entry>
    {
        private Color _originalBackgroundColor;

        public InvalidEntryBehavior()
        {
        }

        protected override void OnAttachedTo(BindableObject bindable)
        {
            Entry entry = bindable as Entry;

            if (entry != null)
            {
                _originalBackgroundColor = entry.BackgroundColor;
                entry.BackgroundColor = Color.IndianRed;
            }
        }

        protected override void OnDetachingFrom(BindableObject bindable)
        {
            Entry entry = bindable as Entry;

            if (entry != null)
            {
                entry.BackgroundColor = _originalBackgroundColor;
            }
        }
    }
}
