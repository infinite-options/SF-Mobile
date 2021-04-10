using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using static ServingFresh.Views.SelectionPage;

namespace ServingFresh.Views
{
    public partial class ConfirmationPage : ContentPage
    {
        public ConfirmationPage(PurchaseDataObject purchase, ScheduleInfo deliveryInfo)
        {
            InitializeComponent();
            cartItemsNumber.Text = purchase.items.Count.ToString();
            contactMessage.Text = "If we have question. We will contact you at " + purchase.delivery_email + " or " + purchase.delivery_phone_num;
            expectedDeliveryMessage.Text = "Your order will be delivered on: " + deliveryInfo.deliveryTimeStamp.ToString("dddd, MMM dd, yyyy");
            PlaceLocationOnMap(double.Parse(purchase.delivery_latitude), double.Parse(purchase.delivery_longitude));
        }

        void PlaceLocationOnMap(double latitude, double longitude)
        {
            Position position = new Position(latitude, longitude);
            map.MapType = MapType.Street;
            var mapSpan = new MapSpan(position, 0.001, 0.001);
            Pin address = new Pin();
            address.Label = "Delivery Address";
            address.Type = PinType.SearchResult;
            address.Position = position;
            map.MoveToRegion(mapSpan);
            map.Pins.Add(address);
        }

        void ReturnToStore(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SelectionPage();
        }

    }  
}
