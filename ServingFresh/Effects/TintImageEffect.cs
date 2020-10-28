using System;
using Xamarin.Forms;

namespace ServingFresh.Effects
{
    public class TintImageEffect : RoutingEffect
    {
        public const string GroupName = "MyCompany";
        public const string Name = "TintImageEffect";

        public Color TintColor { get; set; }

        public TintImageEffect() : base($"{GroupName}.{Name}") { }
    }
}
