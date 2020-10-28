using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ServingFresh.Views
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();

            userEmailAddress.Text = (string)Application.Current.Properties["user_email"];
            userFirstName.Text = (string)Application.Current.Properties["user_first_name"];
            userLastName.Text = (string)Application.Current.Properties["user_last_name"];
            userPassword.Text = "*******";
            userConfirmPassword.Text = "*******";

            userAddress.Text = (string)Application.Current.Properties["user_address"];
            userUnitNumber.Text = (string)Application.Current.Properties["user_unit"];
            userCity.Text = (string)Application.Current.Properties["user_city"];
            userState.Text = (string)Application.Current.Properties["user_state"];
            userZipcode.Text = (string)Application.Current.Properties["user_zip_code"];
            userPhoneNumber.Text = (string)Application.Current.Properties["user_phone_num"];

            Position position = new Position(Double.Parse(Application.Current.Properties["user_latitude"].ToString()), Double.Parse(Application.Current.Properties["user_longitude"].ToString()));
            map.MapType = MapType.Street;
            var mapSpan = new MapSpan(position, 0.001, 0.001);

            Pin address = new Pin();
            address.Label = "Delivery Address";
            address.Type = PinType.SearchResult;
            address.Position = position;

            map.MoveToRegion(mapSpan);
            map.Pins.Add(address);
        }

        void DeliveryDaysClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SelectionPage();
        }

        void OrdersClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage(null);
        }

        void InfoClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new InfoPage();
        }

        void ProfileClick(System.Object sender, System.EventArgs e)
        {
            // AGAIN SINCE YOU ARE IN THE PROFILE PAGE NOTHING SHOULD HAPPEN
            // WHEN CLICK
        }

        void SaveChangesClick(System.Object sender, System.EventArgs e)
        {

        }

        void LogOut(System.Object sender, System.EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("user_id :" + Application.Current.Properties["user_id"]);
            System.Diagnostics.Debug.WriteLine("time_stamp :" + Application.Current.Properties["time_stamp"]);
            System.Diagnostics.Debug.WriteLine("platform :" + Application.Current.Properties["platform"]);
            System.Diagnostics.Debug.WriteLine("user_email :" + Application.Current.Properties["user_email"]);
            System.Diagnostics.Debug.WriteLine("user_first_name :" + Application.Current.Properties["user_first_name"]);
            System.Diagnostics.Debug.WriteLine("user_last_name :" + Application.Current.Properties["user_last_name"]);
            System.Diagnostics.Debug.WriteLine("user_phone_num :" + Application.Current.Properties["user_phone_num"]);
            System.Diagnostics.Debug.WriteLine("user_addres :" + Application.Current.Properties["user_address"]);
            System.Diagnostics.Debug.WriteLine("user_unit :" + Application.Current.Properties["user_unit"]);
            System.Diagnostics.Debug.WriteLine("user_city :" + Application.Current.Properties["user_city"]);
            System.Diagnostics.Debug.WriteLine("user_state :" + Application.Current.Properties["user_state"]);
            System.Diagnostics.Debug.WriteLine("user_zip_code :" + Application.Current.Properties["user_zip_code"]);
            System.Diagnostics.Debug.WriteLine("user_latitude :" + Application.Current.Properties["user_latitude"]);
            System.Diagnostics.Debug.WriteLine("user_longitude :" + Application.Current.Properties["user_longitude"]);
            RemoveAppProperties();
            Application.Current.MainPage = new LogInPage();
        }

        public void RemoveAppProperties()
        {
            Application.Current.Properties.Remove("user_id");
            Application.Current.Properties.Remove("time_stamp");
            Application.Current.Properties.Remove("platform");
            Application.Current.Properties.Remove("user_email");
            Application.Current.Properties.Remove("user_first_name");
            Application.Current.Properties.Remove("user_last_name");
            Application.Current.Properties.Remove("user_phone_num");
            Application.Current.Properties.Remove("user_address");
            Application.Current.Properties.Remove("user_unit");
            Application.Current.Properties.Remove("user_city");
            Application.Current.Properties.Remove("user_state");
            Application.Current.Properties.Remove("user_zip_code");
            Application.Current.Properties.Remove("user_latitude");
            Application.Current.Properties.Remove("user_longitude");
            Application.Current.Properties.Remove("user_delivery_instructions");
        }
    }
}
