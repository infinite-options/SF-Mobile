using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ServingFresh.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
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
        }
    }
}
