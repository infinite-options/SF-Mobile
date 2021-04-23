using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using static ServingFresh.Views.SignUpPage;
using ServingFresh.Models;
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

        void FindLocalProduceBaseOnLocation(System.Object sender, System.EventArgs e)
        {
            if(user == null)
            {
                user = new User();
                user.setUserType("GUEST");
                user.setUserLatitude(currentLocation.Latitude.ToString());
                user.setUserLongitude(currentLocation.Longitude.ToString());
            }
            else
            {
                if(user.getUserID() == "")
                {
                    user.setUserType("GUEST");
                    user.setUserLatitude(currentLocation.Latitude.ToString());
                    user.setUserLongitude(currentLocation.Longitude.ToString());
                }
            }

            Application.Current.MainPage = new SelectionPage();
        }

        void NavigateToLogIn(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new LogInPage();
        }

        void NavigateToSignUp(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SignUpPage();
        }
    }
}
