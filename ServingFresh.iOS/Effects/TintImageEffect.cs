using System;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using CoreTintImageEffect = ServingFresh.Effects.TintImageEffect;

[assembly: ResolutionGroupName("MyCompany")]
[assembly: ExportEffect(typeof(ServingFresh.iOS.Effects.TintImageEffect), nameof(ServingFresh.iOS.Effects.TintImageEffect))]

namespace ServingFresh.iOS.Effects
{
    public class TintImageEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {

                var effect = (CoreTintImageEffect)Element.Effects.FirstOrDefault(e => e is CoreTintImageEffect);

                if (Element is ImageButton imb)
                {
                    Console.Out.Write(Control);
                }

                if (effect == null)
                    return;

                if (Control is UIImageView image)
                {
                    image.Image = image.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    image.TintColor = effect.TintColor.ToUIColor();
                }
                else if (Control is UIButton button)
                {
                    button.SetImage(button.CurrentImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    button.TintColor = effect.TintColor.ToUIColor();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error occurred when setting the {typeof(TintImageEffect)} effect: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override void OnDetached() { }
    }
}
