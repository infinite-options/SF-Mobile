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
using Xamarin.Auth;
using ServingFresh.LogIn.Classes;
using System.Net.Http;
using ServingFresh.Config;
using Newtonsoft.Json;

namespace ServingFresh.Views
{
    public partial class PrincipalPage : ContentPage
    {
        public readonly static Models.User user = new Models.User();
        Location currentLocation;
        private AddressAutocomplete addressToValidate = null;
        public PrincipalPage()
        {
            InitializeComponent();

            currentLocation = new Location();
            currentLocation.Latitude = 37.227124;
            currentLocation.Longitude = -121.886943;
            GetBusinesses();
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
            if (!String.IsNullOrEmpty(AddressEntry.Text))
            {
                var client = new AddressValidation();
                var addressStatus = client.ValidateAddressString(addressToValidate.Street, addressToValidate.Unit, addressToValidate.City, addressToValidate.State, addressToValidate.ZipCode);

                if (addressStatus != null)
                {
                    if (addressStatus == "Y" || addressStatus == "S")
                    {

                        var location = await client.ConvertAddressToGeoCoordiantes(addressToValidate.Street, addressToValidate.City, addressToValidate.State);
                        if (location != null)
                        {
                            var isAddressInZones = await client.getZoneFromLocation(location.Latitude.ToString(), location.Longitude.ToString());

                            if (isAddressInZones != "" && isAddressInZones != "OUTSIDE ZONE RANGE")
                            {
                                SetUser(location);
                            }
                            else
                            {
                                await DisplayAlert("Oops", "You address is outside our delivery areas", "OK");
                                return;
                            }
                        }
                        else
                        {
                            await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                            return;
                        }

                    }
                    else if (addressStatus == "D")
                    {
                        var unit = await DisplayPromptAsync("It looks like your address is missing its unit number", "Please enter your address unit number in the space below", "OK","Cancel");
                        if(unit != null)
                        {
                            addressToValidate.Unit = unit;
                        }
                        return;
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
            bool animate = false;
            scrollView.ScrollToAsync(0, 50, animate);
            Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(),true);
            //logInRow.Height = this.Height - 200;
            //logInFrame.Margin = new Thickness(5, -this.Height + 400, 5, 0);
        }

        void NavigateToSignUp(System.Object sender, System.EventArgs e)
        {
            bool animate = false;
            scrollView.ScrollToAsync(0, 50, animate);
            Application.Current.MainPage.Navigation.PushModalAsync(new AddressPage(), true);
            //addressRow.Height = this.Height - 200;
            //addressFrameSignUp.Margin = new Thickness(5, -this.Height + 400, 5, 0);
        }

        async void OnAddressChanged(object sender, EventArgs eventArgs)
        {
            //var newList = new ObservableCollection<AddressAutocomplete>();

            //addr.OnAddressChanged(addressList, AddressEntry.Text);
            addressList.ItemsSource = await addr.GetPlacesPredictionsAsync(AddressEntry.Text);
        }

        void addressEntryFocused(object sender, EventArgs eventArgs)
        {
            addr.addressEntryFocused(addressList, addressFrame);
        }

        void addressEntryUnfocused(object sender, EventArgs eventArgs)
        {
            addr.addressEntryUnfocused(addressList, addressFrame);
        }

        async void addressSelected(System.Object sender, SelectedItemChangedEventArgs e)
        {
            
            addressToValidate = addr.addressSelected(addressList, AddressEntry, addressFrame);
            string zipcode = await addr.getZipcode(addressToValidate.PredictionID);
            if( zipcode != null)
            {
                addressToValidate.ZipCode = zipcode;
            }

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
            Application.Current.MainPage.Navigation.PopModalAsync();
            //logInRow.Height = 0;
            //logInFrame.Margin = new Thickness(5, 0, 5, 0);
        }

        void HideSignUpUI(System.Object sender, System.EventArgs e)
        {
            //signUpRow.Height = 0;
            //signUpFrame.Margin = new Thickness(5, 0, 5, 0);
            Application.Current.MainPage.Navigation.PopModalAsync();
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

        async void SignUpDirectUserFromPrincipal(System.Object sender, System.EventArgs e)
        {
            if (ValidateSignUpInfo(newUserFirstName, newUserLastName, newUserEmail1, newUserEmail2, newUserPassword1, newUserPassword2))
            {
                if(ValidateEmail(newUserEmail1, newUserEmail2))
                {
                    if(ValidatePassword(newUserPassword1, newUserPassword2))
                    {
                        // user is ready to be sign in.
                        var client = new SignUp();
                        var content = client.SetDirectUser(user, newUserPassword1.Text);
                        var signUpStatus = await SignUp.SignUpNewUser(content);

                        if(signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                        {
                            user.setUserID(signUpStatus);
                            user.setUserPlatform("DIRECT");
                            user.setUserType("CUSTOMER");
                            client.SendUserToSelectionPage();
                        }
                        else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                        {
                            await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "Please check that your password is the same in both entries", "OK");
                        return;
                    }
                }
                else
                {
                    await DisplayAlert("Oops", "Please check that your email is the same in both entries", "OK");
                    return;
                }
            }
            else
            {
                await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                return;
            }
        }

        async void ContinueWithSignUp(System.Object sender, System.EventArgs e)
        {

            if (ValidateSignUpInfo(signUpFirstName, signUpLastName, signUpEmail, signUpPhone, signUpAddress1Entry, signUpCityEntry, signUpStateEntry, signUpZipcodeEntry))
            {
                //try to validate address if address doesn't return true ask to enter unit number and try again
                var client = new AddressValidation();
                var addressStatus = client.ValidateAddressString(signUpAddress1Entry.Text, signUpAddress2Entry.Text, signUpCityEntry.Text, signUpStateEntry.Text, signUpZipcodeEntry.Text);

                if(addressStatus != null)
                {
                    if(addressStatus == "Y" || addressStatus == "S")
                    {

                        var location = await client.ConvertAddressToGeoCoordiantes(signUpAddress1Entry.Text, signUpCityEntry.Text, signUpStateEntry.Text);
                        if(location != null)
                        {
                            var isAddressInZones = await client.getZoneFromLocation(location.Latitude.ToString(), location.Longitude.ToString());

                            if (isAddressInZones != "" && isAddressInZones != "OUTSIDE ZONE RANGE")
                            {
                                addressRow.Height = 0;
                                addressFrameSignUp.Margin = new Thickness(5, 0, 5, 0);
                                signUpRow.Height = this.Height - 200;
                                signUpFrame.Margin = new Thickness(5, -this.Height + 400, 5, 0);

                                user.setUserFirstName(signUpFirstName.Text);
                                user.setUserLastName(signUpLastName.Text);
                                user.setUserEmail(signUpEmail.Text);
                                user.setUserPhoneNumber(signUpPhone.Text);
                                user.setUserAddress(signUpAddress1Entry.Text);
                                user.setUserUnit(signUpAddress2Entry.Text);
                                user.setUserCity(signUpCityEntry.Text);
                                user.setUserState(signUpStateEntry.Text);
                                user.setUserZipcode(signUpZipcodeEntry.Text);
                                user.setUserLatitude(location.Latitude.ToString());
                                user.setUserLongitude(location.Longitude.ToString());

                                newUserFirstName.Text = user.getUserFirstName();
                                newUserLastName.Text = user.getUserLastName();
                                newUserEmail1.Text = user.getUserEmail();
                                newUserEmail2.Text = user.getUserEmail();
                            }
                            else
                            {
                                await DisplayAlert("Oops","You address is outside our delivery areas","OK");
                                return;
                            }
                        }
                        else
                        {
                            await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                            return;
                        }

                    }else if (addressStatus == "D")
                    {
                        await DisplayAlert("Oops", "Please enter your address unit number", "OK");
                        return;
                    }
                }
            }
            else
            {
                await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                return;
            }
        }

        public bool ValidateSignUpInfo(Entry firstName, Entry lastName, Entry email, Entry phoneNumber,  Entry address1, Entry city, Entry state, Entry zipcode)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(firstName.Text)
                || String.IsNullOrEmpty(lastName.Text)
                || String.IsNullOrEmpty(email.Text)
                || String.IsNullOrEmpty(address1.Text)
                || String.IsNullOrEmpty(city.Text)
                || String.IsNullOrEmpty(state.Text)
                || String.IsNullOrEmpty(zipcode.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool ValidateSignUpInfo(Entry address1, Entry city, Entry state, Entry zipcode)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(address1.Text)
                || String.IsNullOrEmpty(city.Text)
                || String.IsNullOrEmpty(state.Text)
                || String.IsNullOrEmpty(zipcode.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool ValidateSignUpInfo(Entry firstName, Entry lastName, Entry email1, Entry email2, Entry password1, Entry password2)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(firstName.Text)
                || String.IsNullOrEmpty(lastName.Text)
                || String.IsNullOrEmpty(email1.Text)
                || String.IsNullOrEmpty(email2.Text)
                || String.IsNullOrEmpty(password1.Text)
                || String.IsNullOrEmpty(password2.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool ValidateDirectSignInCredentials(Entry email, Entry password)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(email.Text)
                || String.IsNullOrEmpty(password.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool ValidateEmail(Entry email1, Entry email2)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(email1.Text)||String.IsNullOrEmpty(email2.Text)))
            {
                if(email1.Text == email2.Text)
                {
                    result = true;
                }
            }
            return result;
        }

        public bool ValidatePassword(Entry password1, Entry password2)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(password1.Text) || String.IsNullOrEmpty(password2.Text)))
            {
                if (password1.Text == password2.Text)
                {
                    result = true;
                }
            }
            return result;
        }

        void HideAddressModal(System.Object sender, System.EventArgs e)
        {
            addressRow.Height = 0;
            addressFrameSignUp.Margin = new Thickness(5, 0, 5, 0);
        }

        async void signUpAddress1Entry_TextChanged(System.Object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            SignUpAddressList.ItemsSource = await addr.GetPlacesPredictionsAsync(signUpAddress1Entry.Text);
        }

        void signUpAddress1Entry_Focused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            addr.addressEntryFocused(SignUpAddressList, signUpAddressFrame);
        }

        void signUpAddress1Entry_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            addr.addressEntryUnfocused(SignUpAddressList, signUpAddressFrame);
        }

        async void SignUpAddressList_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            addressToValidate = addr.addressSelected(SignUpAddressList, signUpAddress1Entry, signUpAddressFrame);
            string zipcode = await addr.getZipcode(addressToValidate.PredictionID);
            if (zipcode != null)
            {
                addressToValidate.ZipCode = zipcode;
            }
            addr.addressSelectedFillEntries(addressToValidate, signUpAddress1Entry, signUpAddress2Entry, signUpCityEntry, signUpStateEntry, signUpZipcodeEntry);
            addr.addressEntryUnfocused(SignUpAddressList, signUpAddressFrame);
        }

        void ContinueWithFacebook(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
            var authenticator = client.GetFacebookAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
                authenticator.Completed += FacebookAuthetication;
                authenticator.Error += Authenticator_Error;
            presenter.Login(authenticator);
        }

        void ContinueWithGoogle(System.Object sender, System.EventArgs e)
        {
            var client = new SignIn();
            var authenticator = client.GetGoogleAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            AuthenticationState.Authenticator = authenticator;
            authenticator.Completed += GoogleAuthetication;
            authenticator.Error += Authenticator_Error;
            presenter.Login(authenticator);
        }

        private async void FacebookAuthetication(object sender, Xamarin.Auth.AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthetication;
                authenticator.Error -= Authenticator_Error;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    var clientLogIn = new SignIn();
                    var clientSignUp = new SignUp();

                    var facebookUser = clientLogIn.GetFacebookUser(e.Account.Properties["access_token"]);
                    var content = clientSignUp.SetDirectUser(user, e.Account.Properties["access_token"], "", facebookUser.id, facebookUser.email, "FACEBOOK");
                    var signUpStatus = await SignUp.SignUpNewUser(content);

                    if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                    {
                        user.setUserID(signUpStatus);
                        user.setUserPlatform("FACEBOOK");
                        user.setUserType("CUSTOMER");
                        clientSignUp.SendUserToSelectionPage();
                    }
                    else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                    {
                        await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                    }
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
            }
        }

        private async void GoogleAuthetication(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthetication;
                authenticator.Error -= Authenticator_Error;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    var clientLogIn = new SignIn();
                    var clientSignUp = new SignUp();

                    var googleUser = await clientLogIn.GetGoogleUser(e);
                    var content = clientSignUp.SetDirectUser(user, e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], googleUser.id, googleUser.email, "GOOGLE");
                    var signUpStatus = await SignUp.SignUpNewUser(content);

                    if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                    {
                        user.setUserID(signUpStatus);
                        user.setUserPlatform("GOOGLE");
                        user.setUserType("CUSTOMER");
                        clientSignUp.SendUserToSelectionPage();
                    }
                    else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                    {
                        await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                    }
                }
                catch (Exception g)
                {
                    Debug.WriteLine(g.Message);
                }
            }
        }

        private async void Authenticator_Error(object sender, Xamarin.Auth.AuthenticatorErrorEventArgs e)
        {
            await DisplayAlert("An error occur when authenticating", "Please try again", "OK");
        }

        public async void GetBusinesses()
        {
            var userLat = "37.227124";
            var userLong = "-121.886943";

            Debug.WriteLine("INPUT 1: " + userLat);
            Debug.WriteLine("INPUT 2: " + userLong);

            //if (userLat == "0" && userLong == "0"){ userLong = "-121.8866517"; userLat = "37.2270928";}

            var client = new HttpClient();
            var response = await client.GetAsync(Constant.ProduceByLocation + userLong + "," + userLat);
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("URL: " + Constant.ProduceByLocation + userLong + "," + userLat);

            Debug.WriteLine("CALL TO ENDPOINT SUCCESSFULL?: " + response.IsSuccessStatusCode);
            Debug.WriteLine("JSON RETURNED: " + result);

            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<ServingFreshBusiness>(result);

                GetDataForSingleList(data.result, data.types);
            }
            else
            {
                //await DisplayAlert("Oops!", "Our system is down. We are working to fix this issue.", "OK");
                return;
            }
        }

        private void GetDataForSingleList(IList<Items> listOfItems, IList<string> types)
        {
            try
            {
                if (listOfItems.Count != 0 && listOfItems != null)
                {
                    List<Items> listUniqueItems = new List<Items>();
                    Dictionary<string, Items> uniqueItems = new Dictionary<string, Items>();
                    foreach (Items a in listOfItems)
                    {
                        string key = a.item_name + a.item_desc + a.item_price;
                        if (!uniqueItems.ContainsKey(key))
                        {
                            uniqueItems.Add(key, a);
                        }
                        else
                        {
                            var savedItem = uniqueItems[key];

                            if (savedItem.item_price != a.item_price)
                            {
                                if (savedItem.business_price != Math.Min(savedItem.business_price, a.business_price))
                                {
                                    savedItem = a;
                                }
                            }
                            else
                            {
                                List<DateTime> creationDates = new List<DateTime>();
                                Debug.WriteLine("NAME {0}, {1}", savedItem.item_name, a.item_name);
                                Debug.WriteLine("SAVED ITEM UID {0}, SAVED TIME STAMP {1}", savedItem.item_uid, savedItem.created_at);
                                Debug.WriteLine("NEW ITEM UID {0}, NEW TIME STAMP {1}", a.item_uid, a.created_at);

                                creationDates.Add(DateTime.Parse(savedItem.created_at));
                                creationDates.Add(DateTime.Parse(a.created_at));
                                creationDates.Sort();

                                if (creationDates[0] != creationDates[1])
                                {
                                    Debug.WriteLine("CREATED FIRST {0}, STRING DATETIME INDEX 0 {1}", creationDates[0], creationDates[0].ToString("yyyy-MM-dd HH:mm:ss"));

                                    if (savedItem.created_at != creationDates[0].ToString("yyyy-MM-dd HH:mm:ss"))
                                    {
                                        savedItem = a;
                                    }
                                }
                                else
                                {
                                    var itemsIdsList = new List<long>();
                                    var savedItemId = savedItem.item_uid.Replace('-', '0');
                                    var newItemId = a.item_uid.Replace('-', '0');

                                    itemsIdsList.Add(long.Parse(savedItemId));
                                    itemsIdsList.Add(long.Parse(newItemId));
                                    itemsIdsList.Sort();

                                    if (savedItemId != itemsIdsList[0].ToString())
                                    {
                                        //savedItem.item_uid = a.item_uid;
                                        savedItem = a;
                                    }
                                }
                                Debug.WriteLine("SELECTED ITEM UID: " + savedItem.item_uid);
                                uniqueItems[key] = savedItem;
                            }
                        }
                    }

                    foreach (string key in uniqueItems.Keys)
                    {
                        listUniqueItems.Add(uniqueItems[key]);
                    }

                    listOfItems = listUniqueItems;

                    vegetablesListView.ItemsSource = SetItemList(listOfItems, "Vegetables");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public ObservableCollection<SingleItem> SetItemList(IList<Items> listOfItems, string type)
        {
            var list = new ObservableCollection<SingleItem>();
            foreach (Items produce in listOfItems)
            {
                if (produce.taxable == null || produce.taxable == "NULL")
                {
                    produce.taxable = "FALSE";
                }

                var itemToInsert = new SingleItem()
                {
                    itemType = produce.item_type,
                    itemImage = produce.item_photo,
                    itemFavoriteImage = "unselectedHeartIcon.png",
                    itemUID = produce.item_uid,
                    itemBusinessUID = produce.itm_business_uid,
                    itemName = produce.item_name,
                    itemPrice = "$ " + produce.item_price.ToString("N2"),
                    itemPriceWithUnit = "$ " + produce.item_price.ToString("N2") + " / " + (string)produce.item_unit.ToString(),
                    itemUnit = (string)produce.item_unit.ToString(),
                    itemDescription = produce.item_desc,
                    itemTaxable = produce.taxable,
                    itemQuantity = 0,
                    itemBusinessPrice = produce.business_price,
                    itemBackgroundColor = Color.FromHex("#FFFFFF"),
                    itemOpacity = 1,
                    isItemVisiable = true,
                    isItemEnable = true,
                    isItemUnavailable = false,
                };

                list.Add(itemToInsert);
            }
            return list;
        }
    }
}
