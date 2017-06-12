using System;
using FormsSkiaBikeTracker.Models;
using PropertyChanged;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Pages
{
    public partial class LoginPage
    {
        [AddINotifyPropertyChangedInterface]
        public class AthleteLoginWrapper
        {
            public Athlete Athlete { get; set; }
            public bool IsExpanded { get; set; }
            public string EnteredPassword { get; set; }
            public Color BackgroundColor
            {
                get
                {
                    if (IsExpanded)
                    {
                        return Color.AliceBlue;
                    }

                    return Color.Transparent;
                }
            }
        }
    }
}