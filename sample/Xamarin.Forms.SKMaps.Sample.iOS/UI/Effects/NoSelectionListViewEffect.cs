using System.Linq;
using Xamarin.Forms.SKMaps.Sample.Ios.UI.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using SharedNoSelectionEffect = Xamarin.Forms.SKMaps.Sample.Forms.UI.Effects.NoSelectionListViewEffect;

[assembly: ExportEffect(typeof(NoSelectionListViewEffect), nameof(NoSelectionListViewEffect))]
namespace Xamarin.Forms.SKMaps.Sample.Ios.UI.Effects
{
    class NoSelectionListViewEffect : PlatformEffect
    {
        private UITableView _NativeControl => Control as UITableView;
        private SharedNoSelectionEffect _Effect => Element.Effects.FirstOrDefault(e => e is SharedNoSelectionEffect) as SharedNoSelectionEffect;

        protected override void OnAttached()
        {
            _NativeControl.AllowsSelection = false;
        }

        protected override void OnDetached()
        {
        }
    }
}
