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
            catch (Exception errorGetCurrentLocation)
            {
                var client = new Diagnostic();
                client.parseException(errorGetCurrentLocation.ToString(), user);
            }

        }

        async void FindLocalProduceBaseOnLocation(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (addressToValidate != null)
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

                                        user.setUserAddress(addressToValidate.Street);
                                        user.setUserCity(addressToValidate.City);
                                        user.setUserUnit(addressToValidate.Unit == null ? "" : addressToValidate.Unit);
                                        user.setUserState(addressToValidate.State);
                                        user.setUserZipcode(addressToValidate.ZipCode);
                                        user.setUserUSPSType(addressStatus);
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
                                var unit = await DisplayPromptAsync("It looks like your address is missing its unit number", "Please enter your address unit number in the space below", "OK", "Cancel");
                                if (unit != null)
                                {
                                    addressToValidate.Unit = unit;
                                }
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
            catch (Exception errorFindLocalProduceBaseOnLocation)
            {
                var client = new Diagnostic();
                client.parseException(errorFindLocalProduceBaseOnLocation.ToString(), user);
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
            scrollView.ScrollToAsync(0, 5, animate);
            Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(),true);
            //logInRow.Height = this.Height - 200;
            //logInFrame.Margin = new Thickness(5, -this.Height + 400, 5, 0);
        }

        void NavigateToSignUp(System.Object sender, System.EventArgs e)
        {
            bool animate = false;
            scrollView.ScrollToAsync(0, 5, animate);
            Application.Current.MainPage.Navigation.PushModalAsync(new AddressPage(), true);
            //addressRow.Height = this.Height - 200;
            //addressFrameSignUp.Margin = new Thickness(5, -this.Height + 400, 5, 0);
        }

        async void OnAddressChanged(object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrEmpty(AddressEntry.Text))
            {
                if (addressToValidate != null)
                {
                    if (addressToValidate.Street != AddressEntry.Text)
                    {
                        addressList.ItemsSource = await addr.GetPlacesPredictionsAsync(AddressEntry.Text);
                        addressEntryFocused(sender, eventArgs);
                    }
                }
                else
                {
                    addressList.ItemsSource = await addr.GetPlacesPredictionsAsync(AddressEntry.Text);
                    addressEntryFocused(sender, eventArgs);
                }
            }
            else
            {
                addressEntryUnfocused(sender, eventArgs);
                addressToValidate = null;
            }
        }

        void addressEntryFocused(object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrEmpty(AddressEntry.Text))
            {
                addr.addressEntryFocused(addressList, addressFrame);
            }
        }

        void addressEntryUnfocused(object sender, EventArgs eventArgs)
        {
            addr.addressEntryUnfocused(addressList, addressFrame);
        }

        async void addressSelected(System.Object sender, SelectedItemChangedEventArgs e)
        {
            AddressEntry.TextChanged -= OnAddressChanged;
            addressToValidate = addr.addressSelected(addressList, AddressEntry, addressFrame);
            string zipcode = await addr.getZipcode(addressToValidate.PredictionID);
            if( zipcode != null)
            {
                addressToValidate.ZipCode = zipcode;
            }
            AddressEntry.Text += zipcode;
            AddressEntry.TextChanged += OnAddressChanged;
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

        public async void GetBusinesses()
        {
            try
            {
                var userLat = "37.227124";
                var userLong = "-121.886943";

                //Debug.WriteLine("INPUT 1: " + userLat);
                //Debug.WriteLine("INPUT 2: " + userLong);

                //if (userLat == "0" && userLong == "0"){ userLong = "-121.8866517"; userLat = "37.2270928";}

                var client = new HttpClient();
                var response = await client.GetAsync(Constant.ProduceByLocation + userLong + "," + userLat);
                var result = await response.Content.ReadAsStringAsync();

                //Debug.WriteLine("URL: " + Constant.ProduceByLocation + userLong + "," + userLat);

                //Debug.WriteLine("CALL TO ENDPOINT SUCCESSFULL?: " + response.IsSuccessStatusCode);
                //Debug.WriteLine("JSON RETURNED: " + result);

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
            }catch(Exception errorGetBusiness)
            {
                var client = new Diagnostic();
                client.parseException(errorGetBusiness.ToString(), user);
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
                                //Debug.WriteLine("NAME {0}, {1}", savedItem.item_name, a.item_name);
                                //Debug.WriteLine("SAVED ITEM UID {0}, SAVED TIME STAMP {1}", savedItem.item_uid, savedItem.created_at);
                                //Debug.WriteLine("NEW ITEM UID {0}, NEW TIME STAMP {1}", a.item_uid, a.created_at);

                                creationDates.Add(DateTime.Parse(savedItem.created_at));
                                creationDates.Add(DateTime.Parse(a.created_at));
                                creationDates.Sort();

                                if (creationDates[0] != creationDates[1])
                                {
                                    //Debug.WriteLine("CREATED FIRST {0}, STRING DATETIME INDEX 0 {1}", creationDates[0], creationDates[0].ToString("yyyy-MM-dd HH:mm:ss"));

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
                                //Debug.WriteLine("SELECTED ITEM UID: " + savedItem.item_uid);
                                uniqueItems[key] = savedItem;
                            }
                        }
                    }
                    string[] arrayTypes = new[] { "vegetable", "fruit" };

                    foreach(string t in arrayTypes)
                    {
                        foreach (string key in uniqueItems.Keys)
                        {
                            if(t == uniqueItems[key].item_type)
                            {
                                listUniqueItems.Add(uniqueItems[key]);
                            }
                        }
                    }
                    

                    listOfItems = listUniqueItems;

                    vegetablesListView.ItemsSource = SetItemList(listOfItems, "Vegetables");
                }

            }
            catch (Exception errorGetDataForSingleList)
            {
                var client = new Diagnostic();
                client.parseException(errorGetDataForSingleList.ToString(), user);
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

        async void BecomeAnAmbassador(System.Object sender, System.EventArgs e)
        {
            string action = await DisplayActionSheet("Love Serving Fresh?\n\nBecome an Ambassador\n\nGive 20, Get 20\n\nRefer a friend and both you and your friend get $10 off on your next two orders.", "Cancel", null, "Login", "Sign Up");
            if (action == "Login")
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(new LogInPage(94, "1"), true);
            }
            else if (action == "Sign Up")
            {
                NavigateToSignUp(sender,e);
            }
        }

        void NavigateToGiftCardPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new GiftCardPage();
        }
    }
}
