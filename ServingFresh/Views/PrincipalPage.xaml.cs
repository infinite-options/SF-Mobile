using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using static ServingFresh.Views.SignUpPage;
using ServingFresh.Models;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ServingFresh.Views
{
    public partial class PrincipalPage : ContentPage
    {
        Location currentLocation;
        private AddressAutocomplete addressToValidate = null;
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

        async void FindLocalProduceBaseOnLocation(System.Object sender, System.EventArgs e)
        {
            if (AddressEntry.Text != null || zipcodeEntry.Text != null)
            {
                if (AddressEntry.Text != null && zipcodeEntry.Text == null)
                {
                    var needToEnterUnit = await DisplayAlert("Do you have a unit number in your address?", "Your answer will help us validate your delivery address. Thank you!", "Yes", "No");
                    if (needToEnterUnit)
                    {
                        var unit = await DisplayPromptAsync("Enter you unit!", "", "Continue", "Cancel");
                        
                        if (unit != "")
                        {
                            // validate complete address with all the input required
                            Debug.WriteLine("RESULT FROM PROMPT " + unit);
                            addressToValidate.Unit = unit;
                            var client = new AddressValidation();
                            var location = await client.ValidateAddress(addressToValidate.Street, addressToValidate.Unit, addressToValidate.City, addressToValidate.State, addressToValidate.ZipCode);
                            if(location != null)
                            {
                                var zone = await client.getZoneFromLocation(location.Latitude.ToString(), location.Longitude.ToString());
                                if(zone != "OUTSIDE ZONE RANGE" && zone != "")
                                {
                                    SetUser(location);
                                    // may need to add the give data
                                }
                                else
                                {
                                    await DisplayAlert("Sorry, your address seems to be outside our supported areas", "Please share with us your interest at pmarathay@gmail.com and we will send you an email as soon as we start serving your area", "OK");
                                    return;
                                }
                            }
                            else
                            {
                                await DisplayAlert("We can't delivery to this address", "Unfortunately, we can't delivery to this address because our algorithm can't verify it. Try a new address", "OK");
                                return;
                            }
                        }
                    }
                    else
                    {
                        addressToValidate.Unit = "";
                        var client = new AddressValidation();
                        var location = await client.ValidateAddress(addressToValidate.Street, addressToValidate.Unit, addressToValidate.City, addressToValidate.State, addressToValidate.ZipCode);
                        if (location != null)
                        {
                            var zone = await client.getZoneFromLocation(location.Latitude.ToString(), location.Longitude.ToString());
                            if (zone != "OUTSIDE ZONE RANGE" && zone != "")
                            {
                                SetUser(location);
                                // may need to add the give data
                            }
                            else
                            {
                                await DisplayAlert("Sorry, your address seems to be outside our supported areas", "Please share with us your interest at pmarathay@gmail.com and we will send you an email as soon as we start serving your area", "OK");
                                return;
                            }
                        }
                        else
                        {
                            await DisplayAlert("We can't delivery to this address", "Unfortunately, we can't delivery to this address because our algorithm can't verify it. Try a new address", "OK");
                            return;
                        }
                    }
                }
                else
                {
                    var client = new AddressValidation();
                    var tempListView = new ListView();
                    var tempCollection = new ObservableCollection<AddressAutocomplete>();

                    if(zipcodeEntry.Text != null)
                    {
                        await addr.GetPlacesPredictionsAsync(tempListView, tempCollection, zipcodeEntry.Text);

                        if(tempCollection.Count != 0)
                        {

                            var location = await client.ConvertAddressToGeoCoordiantes(tempCollection[0].Address);
                            
                            //Debug.WriteLine("ZIPCODE: ADDRESS {0}, CITY {0}, STATE {0}", tempCollection[0].Address, tempCollection[0].City, tempCollection[0].State);
                            //Debug.WriteLine("ZIPCODE: LATITUDE {0}, LONGITUDE {0}", location.Latitude, location.Longitude);
                            if (location != null)
                            {
                                var zone = await client.getZoneFromLocation(location.Latitude.ToString(), location.Longitude.ToString());
                                if (zone != "OUTSIDE ZONE RANGE" && zone != "")
                                {
                                    SetUser(location);
                                    // may need to add the give data
                                }
                                else
                                {
                                    await DisplayAlert("Sorry, your address seems to be outside our supported areas", "Please share with us your interest at pmarathay@gmail.com and we will send you an email as soon as we start serving your area", "OK");
                                    return;
                                }
                            }
                           
                        }
                        else
                        {
                            await DisplayAlert("We can't delivery to this address", "Unfortunately, we can't delivery to this address because our algorithm can't verify it. Try a new address", "OK");
                            return;
                        }
                    }
                }
            }
            else
            {
                SetUser(currentLocation);
            }
        }

        void SetUser(Location location)
        {

            if (user.getUserID() == "")
            {
                user.setUserType("GUEST");
                user.setUserLatitude(location.Latitude.ToString());
                user.setUserLongitude(location.Longitude.ToString());
            }
            else
            {
                user.setUserType("GUEST");
                user.setUserLatitude(location.Latitude.ToString());
                user.setUserLongitude(location.Longitude.ToString());
            }

            Application.Current.MainPage = new SelectionPage();
        }

        Models.Address addr = new Models.Address();

        private ObservableCollection<AddressAutocomplete> _addresses;
        public ObservableCollection<AddressAutocomplete> Addresses
        {
            get => _addresses ?? (_addresses = new ObservableCollection<AddressAutocomplete>());
            set
            {
                if (_addresses != value)
                {
                    _addresses = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _addressText;
        public string AddressText
        {
            get => _addressText;
            set
            {
                if (_addressText != value)
                {
                    _addressText = value;
                    OnPropertyChanged();
                }
            }
        }

        void NavigateToLogIn(System.Object sender, System.EventArgs e)
        {
            logInRow.Height = this.Height - 200;
            logInFrame.Margin = new Thickness(5, -this.Height + 400, 5, 0);
        }

        void NavigateToSignUp(System.Object sender, System.EventArgs e)
        {
            signUpRow.Height = this.Height - 200;
            signUpFrame.Margin = new Thickness(5, -this.Height + 400, 5, 0);
        }

        public async Task GetPlacesPredictionsAsync()
        {
            await addr.GetPlacesPredictionsAsync(addressList, Addresses, AddressEntry.Text);
        }

        void OnAddressChanged(object sender, EventArgs eventArgs)
        {
            addr.OnAddressChanged(addressList, Addresses, AddressEntry.Text);
        }

        void addressEntryFocused(object sender, EventArgs eventArgs)
        {
            addr.addressEntryFocused(addressList, addressFrame);
        }

        void addressEntryUnfocused(object sender, EventArgs eventArgs)
        {
            addr.addressEntryUnfocused(addressList, addressFrame);
        }

        void addressSelected(System.Object sender, SelectedItemChangedEventArgs e)
        {
            addressToValidate = addr.addressSelected(addressList, AddressEntry, addressFrame);
        }

        async void SignInDirectUser(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
            var result = await client.SignInDirectUser(logInButton, userEmailAddress, userPassword);
            if(result != null)
            {
                Debug.WriteLine("You have an acccount");
       

                user.setUserID(result.getUserID());
                user.setUserSessionTime(result.getUserSessionTime());
                user.setUserPlatform(result.getUserPlatform());
                user.setUserType(result.getUserType());
                user.setUserEmail(result.getUserEmail());
                user.setUserFirstName(result.getUserFirstName());
                user.setUserLastName(result.getUserLastName());
                user.setUserPhoneNumber(result.getUserPhoneNumber());
                user.setUserAddress(result.getUserAddress());
                user.setUserUnit(result.getUserUnit());
                user.setUserCity(result.getUserCity());
                user.setUserState(result.getUserState());
                user.setUserZipcode(result.getUserZipcode());
                user.setUserLatitude(result.getUserLatitude());
                user.setUserLongitude(result.getUserLongitude());
                Application.Current.MainPage = new SelectionPage();
            }
            else
            {
                Debug.WriteLine("Not Log In");
            }
        }


        void HideLogInUI(System.Object sender, System.EventArgs e)
        {
            logInRow.Height = 0;
            logInFrame.Margin = new Thickness(5, 0, 5, 0);
        }

        void HideSignUpUI(System.Object sender, System.EventArgs e)
        {
            signUpRow.Height = 0;
            signUpFrame.Margin = new Thickness(5, 0, 5, 0);
        }

        void ShowHidePassword(System.Object sender, System.EventArgs e)
        {
            Label label = (Label)sender;
            if (label.Text == "Show password")
            {
                userPassword.IsPassword = false;
                label.Text = "Hide password";
            }
            else
            {
                userPassword.IsPassword = true;
                label.Text = "Show password";
            }
        }


        void SignInWithFacebookFromPrincipal(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
            
            client.SignInWithFacebook();
        }

        void SignInWithGoogleFromPrincipal(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
            client.SignInWithGoogle();
        }

        void SignInWithAppleFromPricipal(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
        }

        void SignUpUserFromPrincipal(System.Object sender, System.EventArgs e)
        {

        }
    }
}
