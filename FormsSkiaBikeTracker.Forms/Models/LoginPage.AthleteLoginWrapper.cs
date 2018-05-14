using System.ComponentModel;
using FormsSkiaBikeTracker.Models;
using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.Pages
{
    public partial class LoginPage
    {
        public class AthleteLoginWrapper : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

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