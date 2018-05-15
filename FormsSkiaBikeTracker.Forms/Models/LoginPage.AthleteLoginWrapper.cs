using System.ComponentModel;
using FormsSkiaBikeTracker.Models;
using MvvmCross.ViewModels;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.Pages
{
    public partial class LoginPage
    {
        public class AthleteLoginWrapper : MvxNotifyPropertyChanged
        {
            private Athlete _athlete;
            public Athlete Athlete
            {
                get => _athlete;
                set
                {
                    if (Athlete != value)
                    {
                        _athlete = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private bool _isExpanded;
            public bool IsExpanded
            {
                get => _isExpanded;
                set
                {
                    if (IsExpanded != value)
                    {
                        _isExpanded = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private string _enteredPassword;
            public string EnteredPassword
            {
                get => _enteredPassword;
                set
                {
                    if (EnteredPassword != value)
                    {
                        _enteredPassword = value;
                        RaisePropertyChanged();
                    }
                }
            }

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