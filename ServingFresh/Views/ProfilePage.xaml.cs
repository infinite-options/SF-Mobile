using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using ServingFresh.Config;
using ServingFresh.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Switch = Xamarin.Forms.Switch;
using static ServingFresh.Views.PrincipalPage;
using static ServingFresh.Views.SelectionPage;
using static ServingFresh.App;
using System.Threading.Tasks;

namespace ServingFresh.Views
{
    public partial class ProfilePage : ContentPage
    {
        public class Profile
        {
            public string customer_first_name { get; set; }
            public string customer_last_name { get; set; }
            public string customer_phone_num { get; set; }
            public string customer_email { get; set; }
            public string customer_address { get; set; }
            public string customer_unit { get; set; }
            public string customer_city { get; set; }
            public string customer_state { get; set; }
            public string customer_zip { get; set; }
            public string customer_lat { get; set; }
            public string customer_long { get; set; }
            public string customer_uid { get; set; }
            public string guid { get; set; }

            public Profile()
            {
                customer_first_name = "";
                customer_last_name = "";
                customer_phone_num = "";
                customer_email = "";
                customer_address = "";
                customer_unit = "";
                customer_city = "";
                customer_state = "";
                customer_zip = "";
                customer_lat = "";
                customer_long = "";
                customer_uid = "";
                guid = "";
            }
        }

        public class UpdatePassword
        {
            public string customer_email { get; set; }
            public string password { get; set; }
            public string customer_uid { get; set; }
        }

        //{
        //"uid" : "100-000003",
        //"guid" : "22",
        //"notification": "TRUE"
        //}
        public class UpdateNotification
        {
            public string uid { get; set; }
            public string guid { get; set; }
            public string notification { get; set; }
        }

        public Profile profile = new Profile();
        
        private bool isAddressValidated;
        private Address addr = new Address();
        private AddressAutocomplete addressToValidate = null;

        public ProfilePage()
        {
            InitializeComponent();
            //SelectionPage.SetMenu(guestMenuSection, customerMenuSection, historyLabel, profileLabel);
            CartTotal.Text = order.Count.ToString();
            userEmailAddress.Text = user.getUserEmail();
            userEmailAddress.TextColor = Color.Black;
            userFirstName.Text = user.getUserFirstName();
            userLastName.Text = user.getUserLastName();
            if(user.getUserPlatform() == "DIRECT")
            {
                var firstNameInitial = user.getUserFirstName() != null && user.getUserFirstName().Length >= 1 ? user.getUserFirstName().Substring(0, 1) : "";
                var lastNameInitial = user.getUserLastName() != null && user.getUserLastName().Length >= 1 ? user.getUserLastName().Substring(0, 1) : "";

                userInitials.Text = firstNameInitial + lastNameInitial;
                directUserReset.IsVisible = true;
                userInitialsCircle.IsVisible = true;
            }
            else
            {
                //Debug.WriteLine("IMAGE @ PROFILE: " + user.getUserImage());
                socialUserSignedIcon.IsVisible = true;

                if (user.getUserImage() == "")
                {
                    userInitialsCircle.IsVisible = true;
                    var firstNameInitial = user.getUserFirstName() != null && user.getUserFirstName().Length >= 1 ? user.getUserFirstName().Substring(0, 1) : "";
                    var lastNameInitial = user.getUserLastName() != null && user.getUserLastName().Length >= 1 ? user.getUserLastName().Substring(0, 1) : "";

                    userInitials.Text = firstNameInitial + lastNameInitial;
                }
                else
                {
                    userImageCircle.IsVisible = true;
                    imageUser.Source = user.getUserImage();
                }

                if(user.getUserPlatform()== "GOOGLE")
                {
                    signedIcon.Source = "signedGoogleIcon";
                }
                else if(user.getUserPlatform() == "FACEBOOK")
                {
                    signedIcon.Source = "signedFacebookIcon";
                }
                else if (user.getUserPlatform() == "APPLE")
                {
                    signedIcon.Source = "signedAppleIcon";
                }
                
                //passwordCredentials.HeightRequest = 0;
            }

            //if (user.getUserDeviceID() != "")
            //{
            //    notificationButton.IsToggled = true;
            //}
            //else
            //{
            //    notificationButton.IsToggled = false;
            //}
            userAddress.TextChanged -= signUpAddress1Entry_TextChanged;
            userAddress.Text = user.getUserAddress();
            userAddress.TextChanged += signUpAddress1Entry_TextChanged;
            userUnitNumber.Text = user.getUserUnit();
            userCity.Text = user.getUserCity();
            userState.Text = user.getUserState();
            userZipcode.Text = user.getUserZipcode();
            userPhoneNumber.Text = user.getUserPhoneNumber();

            Position position = new Position(Double.Parse(user.getUserLatitude()), Double.Parse(user.getUserLongitude()));
            map.MapType = MapType.Street;
            var mapSpan = new MapSpan(position, 0.001, 0.001);

            Pin address = new Pin();
            address.Label = "Delivery Address";
            address.Type = PinType.SearchResult;
            address.Position = position;

            map.MoveToRegion(mapSpan);
            map.Pins.Add(address);
        }


        void SaveChangesClick(System.Object sender, System.EventArgs e)
        {
            if (!(String.IsNullOrEmpty(userPassword.Text) && String.IsNullOrEmpty(userConfirmPassword.Text)))
            {
                UpdatePasswordClick(sender, e);
            }

            ValidateAddressClick(sender,e);
        }


        async void signUpAddress1Entry_TextChanged(System.Object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrEmpty(userAddress.Text))
            {
                if (addressToValidate != null)
                {
                    if (addressToValidate.Street != userAddress.Text)
                    {
                        SignUpAddressList.ItemsSource = await addr.GetPlacesPredictionsAsync(userAddress.Text);
                        signUpAddress1Entry_Focused(sender, eventArgs);
                    }
                }
                else
                {
                    SignUpAddressList.ItemsSource = await addr.GetPlacesPredictionsAsync(userAddress.Text);
                    signUpAddress1Entry_Focused(sender, eventArgs);
                }
            }
            else
            {
                signUpAddress1Entry_Unfocused(sender, eventArgs);
                addressToValidate = null;
            }
        }

        void signUpAddress1Entry_Focused(System.Object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrEmpty(userAddress.Text))
            {
                addr.addressEntryFocused(SignUpAddressList, signUpAddressFrame);
            }

        }

        void signUpAddress1Entry_Unfocused(System.Object sender, EventArgs eventArgs)
        {
            addr.addressEntryUnfocused(SignUpAddressList, signUpAddressFrame);
        }

        async void SignUpAddressList_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            userAddress.TextChanged -= signUpAddress1Entry_TextChanged;
            addressToValidate = addr.addressSelected(SignUpAddressList, userAddress, signUpAddressFrame);
            string zipcode = await addr.getZipcode(addressToValidate.PredictionID);
            if (zipcode != null)
            {
                addressToValidate.ZipCode = zipcode;
            }
            addr.addressSelectedFillEntries(addressToValidate, userAddress, userUnitNumber, userCity, userState, userZipcode);
            addr.addressEntryUnfocused(SignUpAddressList, signUpAddressFrame);
            userAddress.TextChanged += signUpAddress1Entry_TextChanged;
        }

        async void ValidateAddressClick(object sender, System.EventArgs e)
        {
            try
            {
                var client1 = new SignUp();
                if (userFirstName.Text != "" && userLastName.Text != "" && userAddress.Text != "" && userCity.Text != "" && userState.Text != "" && userPhoneNumber.Text != "" && userZipcode.Text != "")
                {
                    //try to validate address if address doesn't return true ask to enter unit number and try again
                    var client = new AddressValidation();
                    var addressStatus = client.ValidateAddressString(userAddress.Text, userUnitNumber.Text, userCity.Text, userCity.Text, userZipcode.Text);

                    if (addressStatus != null)
                    {
                        if (addressStatus == "Y" || addressStatus == "S")
                        {

                            var location = await client.ConvertAddressToGeoCoordiantes(userAddress.Text, userCity.Text, userZipcode.Text);
                            if (location != null)
                            {
                                var zoneStatus = await client.isLocationInZones(zone, location.Latitude.ToString(), location.Longitude.ToString());

                                if (zoneStatus == "INSIDE CURRENT ZONE" || zoneStatus == "INSIDE DIFFERENT ZONE")
                                {
                                    if (zone == "INSIDE CURRENT ZONE")
                                    {
                                        client.SetPinOnMap(map, location, userAddress.Text);
                                        var updateStatus = await UpdateUserProfile(user, userFirstName.Text, userLastName.Text, userPhoneNumber.Text, userAddress.Text, userUnitNumber.Text, userCity.Text, userState.Text, userZipcode.Text, location.Latitude.ToString(), location.Longitude.ToString());
                                        if (updateStatus)
                                        {
                                            //user.setUserUSPSType(addressStatus);
                                            //user.setUserFirstName(userFirstName.Text);
                                            //user.setUserLastName(userLastName.Text);
                                            //user.setUserPhoneNumber(userPhoneNumber.Text);
                                            //user.setUserAddress(userAddress.Text);
                                            //user.setUserUnit(userUnitNumber.Text == null ? "" : userUnitNumber.Text);
                                            //user.setUserCity(userCity.Text);
                                            //user.setUserState(userState.Text);
                                            //user.setUserZipcode(userZipcode.Text);
                                            //user.setUserLatitude(location.Latitude.ToString());
                                            //user.setUserLongitude(location.Longitude.ToString());
                                            if (messageList != null)
                                            {
                                                if (messageList.ContainsKey("701-000048"))
                                                {
                                                    await DisplayAlert(messageList["701-000048"].title, messageList["701-000048"].message, messageList["701-000048"].responses);
                                                }
                                                else
                                                {
                                                    await DisplayAlert("We have updated your profile successfully!", "", "OK");
                                                }
                                            }
                                            else
                                            {
                                                await DisplayAlert("We have updated your profile successfully!", "", "OK");
                                            }
                                            
                                        }
                                        else
                                        {
                                            if (messageList != null)
                                            {
                                                if (messageList.ContainsKey("701-000049"))
                                                {
                                                    await DisplayAlert(messageList["701-000049"].title, messageList["701-000049"].message, messageList["701-000049"].responses);
                                                }
                                                else
                                                {
                                                    await DisplayAlert("We were not able to update your profile", "", "OK");
                                                }
                                            }
                                            else
                                            {
                                                await DisplayAlert("We were not able to update your profile", "", "OK");
                                            }
                                            
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        client.SetPinOnMap(map, location, userAddress.Text);
                                        bool proceed = false;
                                        if (messageList != null)
                                        {
                                            if (messageList.ContainsKey("701-000050"))
                                            {
                                                string[] arrayResponses = messageList["701-000050"].responses.Split(',');
                                                proceed = await DisplayAlert(messageList["701-000050"].title, messageList["701-000050"].message, 0 < arrayResponses.Length? arrayResponses[0]: "Save changes", 1 < arrayResponses.Length ? arrayResponses[1] : "Don't save changes");
                                            }
                                            else
                                            {
                                                proceed = await DisplayAlert("Great! You have updated your address", "Since, you address is located in a different delivery area we have to reset your shoping cart", "Save changes", "Don't save changes");
                                            }
                                        }
                                        else
                                        {
                                            proceed = await DisplayAlert("Great! You have updated your address", "Since, you address is located in a different delivery area we have to reset your shoping cart", "Save changes", "Don't save changes");
                                        }
                                        
                                        if (proceed)
                                        {
                                            var updateStatus = await UpdateUserProfile(user, userFirstName.Text, userLastName.Text, userPhoneNumber.Text, userAddress.Text, userUnitNumber.Text, userCity.Text, userState.Text, userZipcode.Text, location.Latitude.ToString(), location.Longitude.ToString());
                                            if (updateStatus)
                                            {
                                                if (messageList != null)
                                                {
                                                    if (messageList.ContainsKey("701-000048"))
                                                    {
                                                        await DisplayAlert(messageList["701-000048"].title, messageList["701-000048"].message, messageList["701-000048"].responses);
                                                    }
                                                    else
                                                    {
                                                        await DisplayAlert("We have updated your profile successfully!", "", "OK");
                                                    }
                                                }
                                                else
                                                {
                                                    await DisplayAlert("We have updated your profile successfully!", "", "OK");
                                                }
                                                
                                            }
                                            else
                                            {
                                                if (messageList != null)
                                                {
                                                    if (messageList.ContainsKey("701-000049"))
                                                    {
                                                        await DisplayAlert(messageList["701-000049"].title, messageList["701-000049"].message, messageList["701-000049"].responses);
                                                    }
                                                    else
                                                    {
                                                        await DisplayAlert("We were not able to update your profile", "", "OK");
                                                    }
                                                }
                                                else
                                                {
                                                    await DisplayAlert("We were not able to update your profile", "", "OK");
                                                }
                                               
                                            }
                                        }
                                        return;
                                    }
                                }
                                else
                                {
                                    bool proceed = false;
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000051"))
                                        {
                                            string[] arrayResponses = messageList["701-000051"].responses.Split(',');
                                            proceed = await DisplayAlert(messageList["701-000051"].title, messageList["701-000051"].message, 0 < arrayResponses.Length ? arrayResponses[0] : "Save changes", 1 < arrayResponses.Length ? arrayResponses[1] : "Don't save changes");
                                        }
                                        else
                                        {
                                            proceed = await DisplayAlert("Your address is outside our supported areas", "We can still update your profile, but there will not be any available business for your account", "Save changes", "Don't save changes");
                                        }
                                    }
                                    else
                                    {
                                        proceed = await DisplayAlert("Your address is outside our supported areas", "We can still update your profile, but there will not be any available business for your account", "Save changes", "Don't save changes");
                                    }

                                    if (proceed)
                                    {
                                        var updateStatus = await UpdateUserProfile(user, userFirstName.Text, userLastName.Text, userPhoneNumber.Text, userAddress.Text, userUnitNumber.Text, userCity.Text, userState.Text, userZipcode.Text, location.Latitude.ToString(), location.Longitude.ToString());
                                        if (updateStatus)
                                        {
                                            if (messageList != null)
                                            {
                                                if (messageList.ContainsKey("701-000048"))
                                                {
                                                    await DisplayAlert(messageList["701-000048"].title, messageList["701-000048"].message, messageList["701-000048"].responses);
                                                }
                                                else
                                                {
                                                    await DisplayAlert("We have updated your profile successfully!", "", "OK");
                                                }
                                            }
                                            else
                                            {
                                                await DisplayAlert("We have updated your profile successfully!", "", "OK");
                                            }
                                            
                                        }
                                        else
                                        {
                                            if (messageList != null)
                                            {
                                                if (messageList.ContainsKey("701-000049"))
                                                {
                                                    await DisplayAlert(messageList["701-000049"].title, messageList["701-000049"].message, messageList["701-000049"].responses);
                                                }
                                                else
                                                {
                                                    await DisplayAlert("We were not able to update your profile", "", "OK");
                                                }
                                            }
                                            else
                                            {
                                                await DisplayAlert("We were not able to update your profile", "", "OK");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (messageList != null)
                                {
                                    if (messageList.ContainsKey("701-000010"))
                                    {
                                        await DisplayAlert(messageList["701-000010"].title, messageList["701-000010"].message, messageList["701-000010"].responses);
                                    }
                                    else
                                    {
                                        await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                                }
                                
                                return;
                            }

                        }
                        else if (addressStatus == "D")
                        {
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000002"))
                                {
                                    await DisplayAlert(messageList["701-000002"].title, messageList["701-000002"].message, messageList["701-000002"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Oops", "Please enter your address unit number", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Oops", "Please enter your address unit number", "OK");
                            }
                           
                            return;
                        }
                    }
                }
                else
                {

                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000003"))
                        {
                            await DisplayAlert(messageList["701-000003"].title, messageList["701-000003"].message, messageList["701-000003"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                    }
                    
                    return;
                }
            }catch(Exception errorValidateAddress)
            {
                var client = new Diagnostic();
                client.parseException(errorValidateAddress.ToString(), user);
            }
        }


        public async Task<bool> UpdateUserProfile(User user, string firstName, string lastName, string phoneNumber, string address, string unit, string city, string state, string zipcode, string latitude, string longitude)
        {
            try
            {
                bool result = false;
                var client = new HttpClient();
                var profile = new Profile();

                profile.customer_uid = user.getUserID();
                profile.customer_email = user.getUserEmail();
                profile.customer_first_name = firstName;
                profile.customer_last_name = lastName;
                profile.customer_phone_num = phoneNumber;
                profile.customer_address = address;
                profile.customer_unit = unit == null ? "" : unit;
                profile.customer_city = city;
                profile.customer_state = state;
                profile.customer_zip = zipcode;
                profile.customer_lat = latitude;
                profile.customer_long = longitude;
                profile.guid = user.getUserDeviceID();

                var serializeProfile = JsonConvert.SerializeObject(profile);
                var content = new StringContent(serializeProfile, Encoding.UTF8, "application/json");
                Debug.WriteLine("PROFILE: " + serializeProfile);
                var endpointCall = await client.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/update_Profile", content);
                Debug.WriteLine("ENDPOINT @ PROFILE WAS: " + endpointCall.IsSuccessStatusCode);
                if (endpointCall.IsSuccessStatusCode)
                {

                    user.setUserFirstName(firstName);
                    user.setUserLastName(lastName);
                    user.setUserPhoneNumber(phoneNumber);
                    user.setUserAddress(address);
                    user.setUserUnit(unit);
                    user.setUserCity(city);
                    user.setUserState(state);
                    user.setUserZipcode(zipcode);
                    user.setUserLatitude(latitude);
                    user.setUserLongitude(longitude);
                    result = true;
                }
                return result;
            }catch(Exception errorUpdateUserProfile)
            {
                var client = new Diagnostic();
                client.parseException(errorUpdateUserProfile.ToString(), user);
                return false;
            }
        }



        async void UpdatePasswordClick(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (userPassword.Text == userConfirmPassword.Text)
                {
                    var updateClient = new HttpClient();
                    var changePassword = new UpdatePassword();
                    changePassword.customer_email = user.getUserEmail();
                    changePassword.password = userPassword.Text;
                    changePassword.customer_uid = user.getUserID();

                    var p = JsonConvert.SerializeObject(changePassword);
                    var content = new StringContent(p, Encoding.UTF8, "application/json");

                    System.Diagnostics.Debug.WriteLine("Sign up JSON Object: " + p);

                    var RDSResponse = await updateClient.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/update_email_password", content);
                    if (RDSResponse.IsSuccessStatusCode)
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000052"))
                            {
                                await DisplayAlert(messageList["701-000052"].title, messageList["701-000052"].message, messageList["701-000052"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Awesome!", "Your password has been updated", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Awesome!", "Your password has been updated", "OK");
                        }
                        
                    }
                    else
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000053"))
                            {
                                await DisplayAlert(messageList["701-000053"].title, messageList["701-000053"].message, messageList["701-000053"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Ooops", "Our system is down. We can't process this request at the moment", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Ooops", "Our system is down. We can't process this request at the moment", "OK");
                        }
                        
                    }
                }
                else
                {
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000054"))
                        {
                            await DisplayAlert(messageList["701-000054"].title, messageList["701-000054"].message, messageList["701-000054"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Ooops", "Your new passwords do not match.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Ooops", "Your new passwords do not match.", "OK");
                    }
                    
                }
            }
            catch (Exception errorUpdatePassword)
            {
                var client = new Diagnostic();
                client.parseException(errorUpdatePassword.ToString(), user);
            }
        }

        async void Switch_Toggled(System.Object sender, Xamarin.Forms.ToggledEventArgs e)
        {
            try
            {
                var notification = (Switch)sender;
                Debug.WriteLine(notification.IsToggled);
                var updateNotification = new UpdateNotification();
                updateNotification.uid = user.getUserID();
                updateNotification.guid = user.getUserDeviceID();
                updateNotification.notification = notification.IsToggled.ToString().ToUpper();


                var updateClient = new HttpClient();


                var p = JsonConvert.SerializeObject(updateNotification);
                var content = new StringContent(p, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine("Sign up JSON Object: " + p);

                var RDSResponse = await updateClient.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/update_guid_notification/customer,update", content);
                var r = await RDSResponse.Content.ReadAsStringAsync();
                Debug.WriteLine("Response: " + r);
                if (RDSResponse.IsSuccessStatusCode)
                {
                    //await DisplayAlert("Awesome!", "We've updated your notification settings", "OK");
                }
                else
                {
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000053"))
                        {
                            await DisplayAlert(messageList["701-000053"].title, messageList["701-000053"].message, messageList["701-000053"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Ooops", "Our system is down. We can't process this request at the moment", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Ooops", "Our system is down. We can't process this request at the moment", "OK");
                    }
                   
                }
                
 
            }
            catch (Exception notificationUpdateStatus)
            {
                
            }
        }


        void ShowMenuFromProfile(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new MenuPage(), true);
        }

        void NavigateToCartFromProfile(System.Object sender, System.EventArgs e)
        {
            NavigateToCart(sender, e);
        }

        void NavigateToMainPageFromProfile(System.Object sender, System.EventArgs e)
        {
            NavigateToMain(sender, e);
        }

        void NavigateToStoreFromProfile(System.Object sender, System.EventArgs e)
        {
            NavigateToStore(sender, e);
        }

        void NavigateToHistoryFromProfile(System.Object sender, System.EventArgs e)
        {
            NavigateToHistory(sender, e);
        }

        void NavigateToProfileFromProfile(System.Object sender, System.EventArgs e)
        {
            NavigateToProfile(sender, e);
        }

        void NavigateToInfoFromProfile(System.Object sender, System.EventArgs e)
        {
            NavigateToInfo(sender, e);
        }

        void NavigateToSignInFromProfile(System.Object sender, System.EventArgs e)
        {
            NavigateToSignIn(sender, e);
        }

        void NavigateToSignUpFromProfile(System.Object sender, System.EventArgs e)
        {
            NavigateToSignUp(sender, e);
        }

        void NavigateToRefundsFromProfile(System.Object sender, System.EventArgs e)
        {
            NavigateToRefunds(sender, e);
        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            if (passwordCredentials.IsVisible)
            {
                passwordCredentials.IsVisible = false;
            }
            else
            {
                passwordCredentials.IsVisible = true;
            }
        }
    }
}
