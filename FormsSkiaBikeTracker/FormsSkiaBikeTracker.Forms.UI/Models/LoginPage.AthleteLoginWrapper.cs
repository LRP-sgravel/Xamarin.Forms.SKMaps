using FormsSkiaBikeTracker.Models;
using PropertyChanged;

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
        }
    }
}