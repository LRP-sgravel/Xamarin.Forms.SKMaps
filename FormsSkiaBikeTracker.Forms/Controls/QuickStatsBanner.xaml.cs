using System.Windows.Input;
using FormsSkiaBikeTracker.Extensions;
using LRPFramework.Services.Resources;
using MvvmCross;
using MvvmCross.Localization;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Overlays.Extensions;
using Xamarin.Forms.Maps.Overlays.Models;
using Xamarin.Forms.Xaml;

namespace FormsSkiaBikeTracker.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuickStatsBanner : ContentView
    {
        public static readonly BindableProperty CurrentSpeedProperty = BindableProperty.Create(nameof(CurrentSpeed),
                                                                                               typeof(double),
                                                                                               typeof(QuickStatsBanner),
                                                                                               0.0,
                                                                                               propertyChanged: OnSpeedChanged);
        public static readonly BindableProperty SpeedUnitProperty = BindableProperty.Create(nameof(SpeedUnit),
                                                                                            typeof(DistanceUnit),
                                                                                            typeof(QuickStatsBanner),
                                                                                            DistanceUnit.Kilometer,
                                                                                            propertyChanged: OnSpeedUnitsChanged);
        public static readonly BindableProperty TotalDistanceTraveledProperty = BindableProperty.Create(nameof(TotalDistanceTraveled),
                                                                                                        typeof(Distance),
                                                                                                        typeof(QuickStatsBanner),
                                                                                                        Distance.FromMeters(0),
                                                                                                        propertyChanged: OnDistanceChanged);
        public static readonly BindableProperty StartStopCommandProperty = BindableProperty.Create(nameof(StartStopCommand),
                                                                                                   typeof(ICommand),
                                                                                                   typeof(QuickStatsBanner),
                                                                                                   propertyChanged: OnStartStopCommandChanged);
        public static readonly BindableProperty StartStopCommandParameterProperty = BindableProperty.Create(nameof(StartStopCommandParameter),
                                                                                                            typeof(object),
                                                                                                            typeof(QuickStatsBanner),
                                                                                                            propertyChanged: OnStartStopCommandChanged);
        public static readonly BindableProperty IsActivityRunningProperty = BindableProperty.Create(nameof(IsActivityRunning),
                                                                                                    typeof(bool),
                                                                                                    typeof(QuickStatsBanner),
                                                                                                    false,
                                                                                                    propertyChanged: OnIsActivityRunningChanged);

        public Distance TotalDistanceTraveled
        {
            get => (Distance)GetValue(TotalDistanceTraveledProperty);
            set => SetValue(TotalDistanceTraveledProperty, value);
        }
        
        public DistanceUnit SpeedUnit
        {
            get => (DistanceUnit)GetValue(SpeedUnitProperty);
            set => SetValue(SpeedUnitProperty, value);
        }

        public double CurrentSpeed
        {
            get => (double)GetValue(CurrentSpeedProperty);
            set => SetValue(CurrentSpeedProperty, value);
        }

        public ICommand StartStopCommand
        {
            get => (ICommand)GetValue(StartStopCommandProperty);
            set => SetValue(StartStopCommandProperty, value);
        }

        public object StartStopCommandParameter
        {
            get => (object)GetValue(StartStopCommandParameterProperty);
            set => SetValue(StartStopCommandParameterProperty, value);
        }

        public bool IsActivityRunning
        {
            get => (bool)GetValue(IsActivityRunningProperty);
            set => SetValue(IsActivityRunningProperty, value);
        }

        public QuickStatsBanner()
        {
            IMvxTextProvider textProvider = Mvx.Resolve<IMvxTextProvider>();
            InitializeComponent();

            SpeedHeaderLabel.Text = textProvider.GetText(Constants.GeneralNamespace, nameof(QuickStatsBanner), "Speed");
            DistanceHeaderLabel.Text = textProvider.GetText(Constants.GeneralNamespace, nameof(QuickStatsBanner), "Distance");

            UpdateSpeedText();
            UpdateDistanceText();
            UpdateStartStopIcon();
        }

        private static void OnSpeedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            QuickStatsBanner view = bindable as QuickStatsBanner;

            view.UpdateSpeedText();
        }

        private static void OnSpeedUnitsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            QuickStatsBanner view = bindable as QuickStatsBanner;

            view.UpdateSpeedText();
            view.UpdateDistanceText();
        }

        private static void OnDistanceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            QuickStatsBanner view = bindable as QuickStatsBanner;

            view.UpdateDistanceText();
        }

        private static void OnStartStopCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            QuickStatsBanner view = bindable as QuickStatsBanner;

            view.UpdateStartStopCommand();
        }

        private static void OnIsActivityRunningChanged(BindableObject bindable, object oldValue, object newValue)
        {
            QuickStatsBanner view = bindable as QuickStatsBanner;

            view.UpdateStartStopIcon();
        }

        private void UpdateSpeedText()
        {
            CurrentSpeedLabel.Text = $"{CurrentSpeed:0.00}{SpeedUnit.ToShortSpeedText()}";
        }

        private void UpdateDistanceText()
        {
            DistanceLabel.Text = $"{TotalDistanceTraveled.ToDistanceUnit(SpeedUnit):0.00}{SpeedUnit.ToShortText()}";
        }

        private void UpdateStartStopCommand()
        {
            StartStopTapGesture.Command = StartStopCommand;
            StartStopTapGesture.CommandParameter = StartStopCommandParameter;
        }

        public void UpdateStartStopIcon()
        {
            string iconName = IsActivityRunning ? "stop.svg" : "play.svg";
            IResourceLocator resLocator = Mvx.Resolve<IResourceLocator>();
            string resourceFullName = resLocator.GetResourcePath(ResourceKeys.ImagesKey, iconName);

            StartStopImage.Source = ImageSource.FromResource(resourceFullName, resLocator.ResourcesAssembly);
        }
    }
}