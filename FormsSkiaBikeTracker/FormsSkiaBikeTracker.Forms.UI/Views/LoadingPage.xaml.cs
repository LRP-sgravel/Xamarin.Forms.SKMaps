using Xamarin.Forms;

namespace FormsSkiaBikeTracker.Forms.UI.Views
{
    public partial class LoadingPage
    {
        public LoadingPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
