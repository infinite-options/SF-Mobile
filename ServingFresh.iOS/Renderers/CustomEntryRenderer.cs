using System;
using ServingFresh.iOS.Renderers;
using ServingFresh.Models;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
namespace ServingFresh.iOS.Renderers
{
    public class CustomEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.BorderStyle = UITextBorderStyle.None;
                //Below line is useful to give border color 
                Control.TintColor = UIColor.White;
                Control.Layer.CornerRadius = 0;
                Control.TextColor = UIColor.Black;
            }
        }
    }
}
