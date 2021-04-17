using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ServingFresh.Views
{
    public partial class PrincipalPage : ContentPage
    {
        Location currentLocation;
        public PrincipalPage()
        {
            InitializeComponent();
            currentLocation = new Location();
            currentLocation.Latitude = 37.227124;
            currentLocation.Longitude = -121.886943;
            //GetCurrentLocation();
        }


        public async void GetCurrentLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync();
                //location.Latitude = 37.227124;
                //location.Longitude = -121.886943;
                currentLocation.Latitude = location.Latitude;
                currentLocation.Longitude = location.Longitude;

                if (location != null)
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);

                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null)
                    {
                        var geocodeAddress =
                            $"AdminArea:       {placemark.AdminArea}\n" +
                            $"CountryCode:     {placemark.CountryCode}\n" +
                            $"CountryName:     {placemark.CountryName}\n" +
                            $"FeatureName:     {placemark.FeatureName}\n" +
                            $"Locality:        {placemark.Locality}\n" +
                            $"PostalCode:      {placemark.PostalCode}\n" +
                            $"SubAdminArea:    {placemark.SubAdminArea}\n" +
                            $"SubLocality:     {placemark.SubLocality}\n" +
                            $"SubThoroughfare: {placemark.SubThoroughfare}\n" +
                            $"Thoroughfare:    {placemark.Thoroughfare}\n";

                        Debug.WriteLine(geocodeAddress);
                        //Application.Current.Properties["location"] = "";

                        //Application.Current.Properties["location"] = placemark.Locality + ", " + placemark.AdminArea;
                    }
                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }

            }
            catch (Exception c)
            {
                // Handle not supported on device exception
                Debug.WriteLine("LOCATION MESSAGE CA:" + c.Message);
            }

        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            Debug.WriteLine("LATITUDE: " + currentLocation.Latitude);
            Debug.WriteLine("LONGITUDE: " + currentLocation.Longitude);
            Application.Current.Properties["guest"] = true;
            Application.Current.Properties["user_email"] = "";
            Application.Current.Properties["user_first_name"] = "";
            Application.Current.Properties["user_last_name"] = "";
            Application.Current.Properties["user_phone_num"] = "";
            Application.Current.Properties["user_address"] = "";
            Application.Current.Properties["user_unit"] = "";
            Application.Current.Properties["user_city"] = "";
            Application.Current.Properties["user_state"] = "";
            Application.Current.Properties["user_zip_code"] = "";
            Application.Current.Properties["user_latitude"] = "";
            Application.Current.Properties["user_longitude"] = "";
            Application.Current.Properties["user_delivery_instructions"] = "";
            Application.Current.MainPage = new SelectionPage(currentLocation);
        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new LogInPage();
        }
    }
}
