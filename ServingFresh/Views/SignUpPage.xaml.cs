using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using ServingFresh.Config;
using ServingFresh.LogIn.Classes;
using ServingFresh.Models;
using ServingFresh.Notifications;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Diagnostics;

namespace ServingFresh.Views
{
    public partial class SignUpPage : ContentPage
    {
        public SignUpPost directSignUp = new SignUpPost();
        public bool isAddessValidated = false;
        INotifications appleNotification = DependencyService.Get<INotifications>();
        private string deviceId = null;

        public class EmailVerificationObject
        {
            public string email { get; set; }
        }

        public SignUpPage()
        {
            InitializeComponent();
            InitializeAppProperties();
            InitializeSignUpPost();
            InitializeMap();

            if (Device.RuntimePlatform == Device.iOS)
            {
                deviceId = Preferences.Get("guid", null);
                if(deviceId != null) { Debug.WriteLine("This is the iOS GUID from Direct Sign Up: " + deviceId); }
            }
            else
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Direct Sign Up " + deviceId); }
            }

            if(deviceId != null)
            {
                localNotificationButton.IsToggled = true;
            }
            else
            {
                localNotificationButton.IsToggled = false;
            }
        }
        public string userType = null;

        public SignUpPage(string firstName, string lastName, string phone, string email, string address, string unit, string city, string state, string zipcode, string userType, string latitude, string longitude)
        {
            Debug.WriteLine("ARRIVED FROM GUEST TRACK");
            InitializeComponent();
            InitializeAppProperties();
            InitializeSignUpPost();
            InitializeMap();
            SignUpMessageForGuest();

            userFirstName.Text = firstName;
            userLastName.Text = lastName;
            userEmailAddress.Text = email;
            userConfirmEmailAddress.Text = email;
            userPhoneNumber.Text = phone;
            userAddress.Text = address;
            userUnitNumber.Text = unit;
            userCity.Text = city;
            userState.Text = state;
            userZipcode.Text = zipcode;

            this.userType = userType;

            directSignUp.email = userEmailAddress.Text;
            directSignUp.first_name = userFirstName.Text;
            directSignUp.last_name = userLastName.Text;
            directSignUp.phone_number = userPhoneNumber.Text;
            directSignUp.address = userAddress.Text;
            directSignUp.unit = userUnitNumber.Text;
            directSignUp.city = userCity.Text;
            directSignUp.state = userState.Text;
            directSignUp.zip_code = userZipcode.Text;
            directSignUp.latitude = latitude;
            directSignUp.longitude = longitude;
            directSignUp.role = "CUSTOMER";
            directSignUp.mobile_access_token = "FALSE";
            directSignUp.mobile_refresh_token = "FALSE";
            directSignUp.user_access_token = "FALSE";
            directSignUp.user_refresh_token = "FALSE";
            directSignUp.social = "FALSE";
            directSignUp.password = "";
            directSignUp.social_id = "NULL";


            if (Device.RuntimePlatform == Device.iOS)
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Direct Sign Up: " + deviceId); }
            }
            else
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Direct Sign Up " + deviceId); }
            }

            if (deviceId != null)
            {
                localNotificationButton.IsToggled = true;
            }
            else
            {
                localNotificationButton.IsToggled = false;
            }
        }

        public void InitializeSignUpPost()
        {
            var device = "";
            if (Device.RuntimePlatform == Device.Android)
            {
                device = "MOBILE ANDROID";
            }
            else
            {
                device = "MOBILE IOS";
            }
            directSignUp.email = "";
            directSignUp.first_name = "";
            directSignUp.last_name = "";
            directSignUp.phone_number = "";
            directSignUp.address = "";
            directSignUp.unit = "";
            directSignUp.city = "";
            directSignUp.state = "";
            directSignUp.zip_code = "";
            directSignUp.latitude = "0.0";
            directSignUp.longitude = "0.0";
            directSignUp.referral_source = device;
            directSignUp.role = "CUSTOMER";
            directSignUp.mobile_access_token = "FALSE";
            directSignUp.mobile_refresh_token = "FALSE";
            directSignUp.user_access_token = "FALSE";
            directSignUp.user_refresh_token = "FALSE";
            directSignUp.social = "FALSE";
            directSignUp.password = "";
            directSignUp.social_id = "NULL";
        }

        public void InitializeMap()
        {
            map.MapType = MapType.Street;
            Position point = new Position(37.334789, -121.888138);
            var mapSpan = new MapSpan(point, 5, 5);
            map.MoveToRegion(mapSpan);
        }

        public void InitializeAppProperties()
        {
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

            //if (!(bool)Application.Current.Properties["guest"])
            //{
            //    validateAddressButton.IsVisible = false;
            //    validateAddressButton.IsEnabled = false;
            //}
        }

        async void ValidateAddressClick(object sender, System.EventArgs e)
        {
            if(validateAddressButton.Text != "Sign up")
            {
                if (userEmailAddress.Text != null)
                {
                    directSignUp.email = userEmailAddress.Text.ToLower().Trim();
                }
                else
                {
                    await DisplayAlert("Error", "Please enter a valid email address.", "OK");
                }

                if (userConfirmEmailAddress.Text != null)
                {
                    string conformEmail = userConfirmEmailAddress.Text.ToLower().Trim();
                    if (!directSignUp.email.Equals(conformEmail))
                    {
                        await DisplayAlert("Error", "Your email doesn't match", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Please enter a valid email address.", "OK");
                }

                if (userPassword.Text != null)
                {
                    directSignUp.password = userPassword.Text.Trim();
                }
                else
                {
                    await DisplayAlert("Error", "Please enter a password", "OK");
                }

                if (userConfirmPassword.Text != null)
                {
                    string password = userConfirmPassword.Text.Trim();
                    if (!directSignUp.password.Equals(password))
                    {
                        await DisplayAlert("Error", "Your email doesn't match", "OK");
                    }
                }

                if (userFirstName.Text != null)
                {
                    directSignUp.first_name = userFirstName.Text.Trim();
                }
                else
                {
                    await DisplayAlert("Error", "Please your first name.", "OK");
                }

                if (userLastName.Text != null)
                {
                    directSignUp.last_name = userLastName.Text.Trim();
                }
                else
                {
                    await DisplayAlert("Error", "Please your first name.", "OK");
                }

                if (userAddress.Text != null)
                {
                    directSignUp.address = userAddress.Text.Trim();
                }
                else
                {
                    await DisplayAlert("Error", "Please enter your address", "OK");
                }

                if (userUnitNumber.Text != null)
                {
                    directSignUp.unit = userUnitNumber.Text.Trim();
                }

                if (userCity.Text != null)
                {
                    directSignUp.city = userCity.Text.Trim();
                }
                else
                {
                    await DisplayAlert("Error", "Please enter your city", "OK");
                }

                if (userState.Text != null)
                {
                    directSignUp.state = userState.Text.Trim();
                }
                else
                {
                    await DisplayAlert("Error", "Please enter your state", "OK");
                }

                if (userZipcode.Text != null)
                {
                    directSignUp.zip_code = userZipcode.Text.Trim();
                }
                else
                {
                    await DisplayAlert("Error", "Please enter your zipcode", "OK");
                }

                if (userPhoneNumber.Text != null && userPhoneNumber.Text.Length == 10)
                {
                    directSignUp.phone_number = userPhoneNumber.Text.Trim();
                }
                else
                {
                    await DisplayAlert("Error", "Please enter your zipcode", "OK");
                }

                //if (userDeliveryInstructions.Text == null)
                //{
                //    userDeliveryInstructions.Text = "";
                //}
                //else
                //{
              
                //}

                // Setting request for USPS API
                XDocument requestDoc = new XDocument(
                    new XElement("AddressValidateRequest",
                    new XAttribute("USERID", "400INFIN1745"),
                    new XElement("Revision", "1"),
                    new XElement("Address",
                    new XAttribute("ID", "0"),
                    new XElement("Address1", directSignUp.address),
                    new XElement("Address2", directSignUp.unit),
                    new XElement("City", directSignUp.city),
                    new XElement("State", directSignUp.state),
                    new XElement("Zip5", directSignUp.zip_code),
                    new XElement("Zip4", "")
                         )
                     )
                 );
                var url = "http://production.shippingapis.com/ShippingAPI.dll?API=Verify&XML=" + requestDoc;
                Console.WriteLine(url);
                var client = new WebClient();
                var response = client.DownloadString(url);

                var xdoc = XDocument.Parse(response.ToString());
                Console.WriteLine(xdoc);
                string latitude = "0";
                string longitude = "0";
                foreach (XElement element in xdoc.Descendants("Address"))
                {
                    if (GetXMLElement(element, "Error").Equals(""))
                    {
                        if (GetXMLElement(element, "DPVConfirmation").Equals("Y") && GetXMLElement(element, "Zip5").Equals(directSignUp.zip_code) && GetXMLElement(element, "City").Equals(directSignUp.city.ToUpper())) // Best case
                        {
                            // Get longitude and latitide because we can make a deliver here. Move on to next page.
                            // Console.WriteLine("The address you entered is valid and deliverable by USPS. We are going to get its latitude & longitude");
                            //GetAddressLatitudeLongitude();
                            Geocoder geoCoder = new Geocoder();

                            IEnumerable<Position> approximateLocations = await geoCoder.GetPositionsForAddressAsync(directSignUp.address + "," + directSignUp.city + "," + directSignUp.state + "," + directSignUp.zip_code);
                            Position position = approximateLocations.FirstOrDefault();

                            latitude = $"{position.Latitude}";
                            longitude = $"{position.Longitude}";
                            Debug.WriteLine("LATITUDE: " + latitude);
                            Debug.WriteLine("LONGITUDE: " + longitude);
                            directSignUp.latitude = latitude;
                            directSignUp.longitude = longitude;
                            map.MapType = MapType.Street;
                            var mapSpan = new MapSpan(position, 0.001, 0.001);

                            Pin address = new Pin();
                            address.Label = "Delivery Address";
                            address.Type = PinType.SearchResult;
                            address.Position = position;

                            map.MoveToRegion(mapSpan);
                            map.Pins.Add(address);

                            break;
                        }
                        else if (GetXMLElement(element, "DPVConfirmation").Equals("D"))
                        {
                            //await DisplayAlert("Alert!", "Address is missing information like 'Apartment number'.", "Ok");
                            //return;
                        }
                        else
                        {
                            //await DisplayAlert("Alert!", "Seems like your address is invalid.", "Ok");
                            //return;
                        }
                    }
                    else
                    {   // USPS sents an error saying address not found in there records. In other words, this address is not valid because it does not exits.
                        //Console.WriteLine("Seems like your address is invalid.");
                        //await DisplayAlert("Alert!", "Error from USPS. The address you entered was not found.", "Ok");
                        //return;
                    }
                }
                if (latitude == "0" || longitude == "0")
                {
                    await DisplayAlert("We couldn't find your address", "Please check for errors.", "Ok");
                }
                else
                {
                    isAddessValidated = true;
                    await DisplayAlert("We validated your address", "Please click on the Sign up button to create your account!", "OK");
                    await Application.Current.SavePropertiesAsync();
                    validateAddressButton.Text = "Sign up";
                    validateAddressButton.TextColor = Color.White;
                    validateAddressButton.BackgroundColor = Color.FromHex("#FF8500");
                }
            }
            else
            {
                if (userType != null)
                {
                    if (userConfirmPassword.Text == userPassword.Text && userPassword.Text != string.Empty && userConfirmPassword.Text != string.Empty)
                    {
                        directSignUp.password = userPassword.Text;

                        if (userEmailAddress.Text != userConfirmEmailAddress.Text)
                        {
                            return;
                        }
                        else
                        {
                            directSignUp.email = userEmailAddress.Text;
                        }
                        var client = new HttpClient();
                        var directSignUpSerializedObject = JsonConvert.SerializeObject(directSignUp);
                        var content = new StringContent(directSignUpSerializedObject, Encoding.UTF8, "application/json");

                        System.Diagnostics.Debug.WriteLine("Sign up JSON Object: " + directSignUpSerializedObject);

                        //var handler = new HttpClientHandler();
                        //handler.AllowAutoRedirect = true;

                        Debug.WriteLine("Line 361: About to call sign up end point");
                        var RDSResponse = await client.PostAsync(Constant.SignUpUrl, content);
                        Debug.WriteLine("Line 365: Done calling sign up end point");

                        var RDSMessage = await RDSResponse.Content.ReadAsStringAsync();

                        Debug.WriteLine("Status code: " + RDSResponse.IsSuccessStatusCode);
                        Debug.WriteLine("Message from request: " + RDSMessage);

                        if (RDSResponse.IsSuccessStatusCode)
                        {
                            if (RDSMessage.Contains("200"))
                            {
                                var RDSData = JsonConvert.DeserializeObject<SignUpResponse>(RDSMessage);
                                DateTime today = DateTime.Now;
                                DateTime expDate = today.AddDays(Constant.days);

                                Application.Current.Properties["user_id"] = RDSData.result.customer_uid;
                                Application.Current.Properties["time_stamp"] = expDate;
                                Application.Current.Properties["platform"] = "DIRECT";
                                Application.Current.Properties["user_email"] = directSignUp.email;
                                Application.Current.Properties["user_first_name"] = directSignUp.first_name;
                                Application.Current.Properties["user_last_name"] = directSignUp.last_name;
                                Application.Current.Properties["user_phone_num"] = directSignUp.phone_number;
                                Application.Current.Properties["user_address"] = directSignUp.address;
                                Application.Current.Properties["user_unit"] = directSignUp.unit;
                                Application.Current.Properties["user_city"] = directSignUp.city;
                                Application.Current.Properties["user_state"] = directSignUp.state;
                                Application.Current.Properties["user_zip_code"] = directSignUp.zip_code;
                                Application.Current.Properties["user_latitude"] = directSignUp.latitude;
                                Application.Current.Properties["user_longitude"] = directSignUp.longitude;
                                //Application.Current.Properties["user_delivery_instructions"] = userDeliveryInstructions.Text;

                                _ = Application.Current.SavePropertiesAsync();

                                EmailVerificationObject user = new EmailVerificationObject();
                                user.email = directSignUp.email;

                                var userSerializedObject = JsonConvert.SerializeObject(user);
                                var userContent = new StringContent(userSerializedObject, Encoding.UTF8, "application/json");

                                Debug.WriteLine("Email Verification JSON Object to send: " + userSerializedObject);

                                var response = await client.PostAsync(Constant.EmailVerificationUrl, userContent);

                                Debug.WriteLine("Status code: " + response.IsSuccessStatusCode);
                                if (!response.IsSuccessStatusCode)
                                {
                                    await DisplayAlert("Ooops!", "We weren't able to send you a verification link to your email.", "OK");
                                }

                                if (deviceId != null)
                                {
                                    NotificationPost notificationPost = new NotificationPost();

                                    notificationPost.uid = (string)Application.Current.Properties["user_id"];
                                    Application.Current.Properties["guid"] = deviceId.Substring(5);
                                    notificationPost.guid = deviceId.Substring(5);
                                    notificationPost.notification = "TRUE";

                                    var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                    var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                    Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                    var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                    Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);
                                    if (clientResponse.IsSuccessStatusCode)
                                    {
                                        System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                                    }
                                    else
                                    {
                                        await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                                    }
                                }

                                Application.Current.MainPage = new SelectionPage();
                            }
                            else
                            {
                                await DisplayAlert("Alert!", "This email already exists", "OK");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Ooops", "Please enter a password", "OK");
                        return;
                    }


                }
                else
                {
                    if (isAddessValidated)
                    {
                        Debug.WriteLine("LINE 514");
                        var client = new HttpClient();
                        var directSignUpSerializedObject = JsonConvert.SerializeObject(directSignUp);
                        var content = new StringContent(directSignUpSerializedObject, Encoding.UTF8, "application/json");

                        Debug.WriteLine("Sign up JSON Object: " + directSignUpSerializedObject);

                        //var handler = new HttpClientHandler();
                        //handler.AllowAutoRedirect = true;

                        Debug.WriteLine("Line 361: About to call sign up end point");
                        var RDSResponse = await client.PostAsync(Constant.SignUpUrl, content);
                        Debug.WriteLine("Line 365: Done calling sign up end point");

                        var RDSMessage = await RDSResponse.Content.ReadAsStringAsync();

                        Debug.WriteLine("Status code: " + RDSResponse.IsSuccessStatusCode);
                        Debug.WriteLine("Message from request: " + RDSMessage);

                        if (RDSResponse.IsSuccessStatusCode)
                        {
                            if (RDSMessage.Contains("200"))
                            {
                                var RDSData = JsonConvert.DeserializeObject<SignUpResponse>(RDSMessage);
                                DateTime today = DateTime.Now;
                                DateTime expDate = today.AddDays(Constant.days);

                                Application.Current.Properties["user_id"] = RDSData.result.customer_uid;
                                Application.Current.Properties["time_stamp"] = expDate;
                                Application.Current.Properties["platform"] = "DIRECT";
                                Application.Current.Properties["user_email"] = directSignUp.email;
                                Application.Current.Properties["user_first_name"] = directSignUp.first_name;
                                Application.Current.Properties["user_last_name"] = directSignUp.last_name;
                                Application.Current.Properties["user_phone_num"] = directSignUp.phone_number;
                                Application.Current.Properties["user_address"] = directSignUp.address;
                                Application.Current.Properties["user_unit"] = directSignUp.unit;
                                Application.Current.Properties["user_city"] = directSignUp.city;
                                Application.Current.Properties["user_state"] = directSignUp.state;
                                Application.Current.Properties["user_zip_code"] = directSignUp.zip_code;
                                Application.Current.Properties["user_latitude"] = directSignUp.latitude;
                                Application.Current.Properties["user_longitude"] = directSignUp.longitude;
                                //Application.Current.Properties["user_delivery_instructions"] = userDeliveryInstructions.Text;

                                _ = Application.Current.SavePropertiesAsync();

                                EmailVerificationObject user = new EmailVerificationObject();
                                user.email = directSignUp.email;

                                var userSerializedObject = JsonConvert.SerializeObject(user);
                                var userContent = new StringContent(userSerializedObject, Encoding.UTF8, "application/json");

                                Debug.WriteLine("Email Verification JSON Object to send: " + userSerializedObject);

                                var response = await client.PostAsync(Constant.EmailVerificationUrl, userContent);

                                Debug.WriteLine("Status code: " + response.IsSuccessStatusCode);
                                if (!response.IsSuccessStatusCode)
                                {
                                    await DisplayAlert("Ooops!", "We weren't able to send you a verification link to your email.", "OK");
                                }

                                if (deviceId != null)
                                {
                                    NotificationPost notificationPost = new NotificationPost();

                                    notificationPost.uid = (string)Application.Current.Properties["user_id"];
                                    Application.Current.Properties["guid"] = deviceId.Substring(5);
                                    notificationPost.guid = deviceId.Substring(5);
                                    notificationPost.notification = "TRUE";

                                    var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                    var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                    Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                    var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                    Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);
                                    if (clientResponse.IsSuccessStatusCode)
                                    {
                                        System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                                    }
                                    else
                                    {
                                        await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                                    }
                                }

                                Application.Current.MainPage = new SelectionPage();
                            }
                            else
                            {
                                await DisplayAlert("Alert!", "This email already exists", "OK");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Message", "We weren't able to sign you up", "OK");
                    }
                }
            }
        }

        public static string GetXMLElement(XElement element, string name)
        {
            var el = element.Element(name);
            if (el != null)
            {
                return el.Value;
            }
            return "";
        }

        public static string GetXMLAttribute(XElement element, string name)
        {
            var el = element.Attribute(name);
            if (el != null)
            {
                return el.Value;
            }
            return "";
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        async void SignUpMessageForGuest()
        {
            //await DisplayAlert("Congratulations", "Payment was successful. We appreciate your business", "OK");
            var result = await DisplayAlert("", "Your payment was successful. We appreciate your business.\n\nYou are one step away from having an account and accessing additional coupons and promotions.", "Sign Up", "Home Page");
            if (result)
            {
                var sender = new System.Object();
                var e = new System.EventArgs();
                Application.Current.Properties["guest"] = false;
                isAddessValidated = true;
                validateAddressButton.Text = "Sign up";
                validateAddressButton.TextColor = Color.White;
                validateAddressButton.BackgroundColor = Color.FromHex("#FF8500");
                // SignUpNewUser(sender,e);
            }
            else
            {
                Application.Current.MainPage = new LogInPage();
            }
        }


        async void SignUpNewUser(System.Object sender, System.EventArgs e)
        {
           


                if (userType != null)
                {
                    if (userConfirmPassword.Text == userPassword.Text && userPassword.Text != string.Empty && userConfirmPassword.Text != string.Empty)
                    {
                        directSignUp.password = userPassword.Text;

                        if(userEmailAddress.Text != userConfirmEmailAddress.Text)
                        {
                            return;
                        }
                        else
                        {
                            directSignUp.email = userEmailAddress.Text;
                        }
                        var client = new HttpClient();
                        var directSignUpSerializedObject = JsonConvert.SerializeObject(directSignUp);
                        var content = new StringContent(directSignUpSerializedObject, Encoding.UTF8, "application/json");

                        System.Diagnostics.Debug.WriteLine("Sign up JSON Object: " + directSignUpSerializedObject);

                        //var handler = new HttpClientHandler();
                        //handler.AllowAutoRedirect = true;

                        Debug.WriteLine("Line 361: About to call sign up end point");
                        var RDSResponse = await client.PostAsync(Constant.SignUpUrl, content);
                        Debug.WriteLine("Line 365: Done calling sign up end point");

                        var RDSMessage = await RDSResponse.Content.ReadAsStringAsync();

                        Debug.WriteLine("Status code: " + RDSResponse.IsSuccessStatusCode);
                        Debug.WriteLine("Message from request: " + RDSMessage);

                        if (RDSResponse.IsSuccessStatusCode)
                        {
                            if (RDSMessage.Contains("200"))
                            {
                                var RDSData = JsonConvert.DeserializeObject<SignUpResponse>(RDSMessage);
                                DateTime today = DateTime.Now;
                                DateTime expDate = today.AddDays(Constant.days);

                                Application.Current.Properties["user_id"] = RDSData.result.customer_uid;
                                Application.Current.Properties["time_stamp"] = expDate;
                                Application.Current.Properties["platform"] = "DIRECT";
                                Application.Current.Properties["user_email"] = directSignUp.email;
                                Application.Current.Properties["user_first_name"] = directSignUp.first_name;
                                Application.Current.Properties["user_last_name"] = directSignUp.last_name;
                                Application.Current.Properties["user_phone_num"] = directSignUp.phone_number;
                                Application.Current.Properties["user_address"] = directSignUp.address;
                                Application.Current.Properties["user_unit"] = directSignUp.unit;
                                Application.Current.Properties["user_city"] = directSignUp.city;
                                Application.Current.Properties["user_state"] = directSignUp.state;
                                Application.Current.Properties["user_zip_code"] = directSignUp.zip_code;
                                Application.Current.Properties["user_latitude"] = directSignUp.latitude;
                                Application.Current.Properties["user_longitude"] = directSignUp.longitude;
                                //Application.Current.Properties["user_delivery_instructions"] = userDeliveryInstructions.Text;

                                _ = Application.Current.SavePropertiesAsync();

                                EmailVerificationObject user = new EmailVerificationObject();
                                user.email = directSignUp.email;

                                var userSerializedObject = JsonConvert.SerializeObject(user);
                                var userContent = new StringContent(userSerializedObject, Encoding.UTF8, "application/json");

                                Debug.WriteLine("Email Verification JSON Object to send: " + userSerializedObject);

                                var response = await client.PostAsync(Constant.EmailVerificationUrl, userContent);

                                Debug.WriteLine("Status code: " + response.IsSuccessStatusCode);
                                if (!response.IsSuccessStatusCode)
                                {
                                    await DisplayAlert("Ooops!", "We weren't able to send you a verification link to your email.", "OK");
                                }

                                if (deviceId != null)
                                {
                                    NotificationPost notificationPost = new NotificationPost();

                                    notificationPost.uid = (string)Application.Current.Properties["user_id"];
                                    Application.Current.Properties["guid"] = deviceId.Substring(5);
                                    notificationPost.guid = deviceId.Substring(5);
                                    notificationPost.notification = "TRUE";

                                    var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                    var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                    Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                    var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                    Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);
                                    if (clientResponse.IsSuccessStatusCode)
                                    {
                                        System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                                    }
                                    else
                                    {
                                        await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                                    }
                                }

                                Application.Current.MainPage = new SelectionPage();
                            }
                            else
                            {
                                await DisplayAlert("Alert!", "This email already exists", "OK");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Ooops", "Please enter a password", "OK");
                        return;
                    }


                }
                else
                {


                    if (isAddessValidated)
                    {
                        var client = new HttpClient();
                        var directSignUpSerializedObject = JsonConvert.SerializeObject(directSignUp);
                        var content = new StringContent(directSignUpSerializedObject, Encoding.UTF8, "application/json");

                        System.Diagnostics.Debug.WriteLine("Sign up JSON Object: " + directSignUpSerializedObject);

                        //var handler = new HttpClientHandler();
                        //handler.AllowAutoRedirect = true;

                        Debug.WriteLine("Line 361: About to call sign up end point");
                        var RDSResponse = await client.PostAsync(Constant.SignUpUrl, content);
                        Debug.WriteLine("Line 365: Done calling sign up end point");

                        var RDSMessage = await RDSResponse.Content.ReadAsStringAsync();

                        Debug.WriteLine("Status code: " + RDSResponse.IsSuccessStatusCode);
                        Debug.WriteLine("Message from request: " + RDSMessage);

                        if (RDSResponse.IsSuccessStatusCode)
                        {
                            if (RDSMessage.Contains("200"))
                            {
                                var RDSData = JsonConvert.DeserializeObject<SignUpResponse>(RDSMessage);
                                DateTime today = DateTime.Now;
                                DateTime expDate = today.AddDays(Constant.days);

                                Application.Current.Properties["user_id"] = RDSData.result.customer_uid;
                                Application.Current.Properties["time_stamp"] = expDate;
                                Application.Current.Properties["platform"] = "DIRECT";
                                Application.Current.Properties["user_email"] = directSignUp.email;
                                Application.Current.Properties["user_first_name"] = directSignUp.first_name;
                                Application.Current.Properties["user_last_name"] = directSignUp.last_name;
                                Application.Current.Properties["user_phone_num"] = directSignUp.phone_number;
                                Application.Current.Properties["user_address"] = directSignUp.address;
                                Application.Current.Properties["user_unit"] = directSignUp.unit;
                                Application.Current.Properties["user_city"] = directSignUp.city;
                                Application.Current.Properties["user_state"] = directSignUp.state;
                                Application.Current.Properties["user_zip_code"] = directSignUp.zip_code;
                                Application.Current.Properties["user_latitude"] = directSignUp.latitude;
                                Application.Current.Properties["user_longitude"] = directSignUp.longitude;
                                //Application.Current.Properties["user_delivery_instructions"] = userDeliveryInstructions.Text;

                                _ = Application.Current.SavePropertiesAsync();

                                EmailVerificationObject user = new EmailVerificationObject();
                                user.email = directSignUp.email;

                                var userSerializedObject = JsonConvert.SerializeObject(user);
                                var userContent = new StringContent(userSerializedObject, Encoding.UTF8, "application/json");

                                Debug.WriteLine("Email Verification JSON Object to send: " + userSerializedObject);

                                var response = await client.PostAsync(Constant.EmailVerificationUrl, userContent);

                                Debug.WriteLine("Status code: " + response.IsSuccessStatusCode);
                                if (!response.IsSuccessStatusCode)
                                {
                                    await DisplayAlert("Ooops!", "We weren't able to send you a verification link to your email.", "OK");
                                }

                                if (deviceId != null)
                                {
                                    NotificationPost notificationPost = new NotificationPost();

                                    notificationPost.uid = (string)Application.Current.Properties["user_id"];
                                    Application.Current.Properties["guid"] = deviceId.Substring(5);
                                    notificationPost.guid = deviceId.Substring(5);
                                    notificationPost.notification = "TRUE";

                                    var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                    var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                                    Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                    var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                                    Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);
                                    if (clientResponse.IsSuccessStatusCode)
                                    {
                                        System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                                    }
                                    else
                                    {
                                        await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                                    }
                                }

                                Application.Current.MainPage = new SelectionPage();
                            }
                            else
                            {
                                await DisplayAlert("Alert!", "This email already exists", "OK");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Message", "We weren't able to sign you up", "OK");
                    }
                }
            
        }

        void Switch_Toggled(System.Object sender, Xamarin.Forms.ToggledEventArgs e)
        {
            //bool state = false;
            //var notificationsButton = (Switch)sender;
            //state = notificationsButton.IsToggled;
            //System.Diagnostics.Debug.WriteLine(state);

            //if (state)
            //{
            //    var id = Preferences.Get("my_id", string.Empty);
            //    if (string.IsNullOrWhiteSpace(id))
            //    {
            //        id = System.Guid.NewGuid().ToString();
            //        Preferences.Set("my_id", id);
            //        System.Diagnostics.Debug.WriteLine("id: " + id);
            //        System.Diagnostics.Debug.WriteLine("This is the guid generated by your phone first time: " + Preferences.Get("my_id", string.Empty));
            //    }
            //    System.Diagnostics.Debug.WriteLine("This is the guid generated by your phone from Preferences: " + Preferences.Get("my_id", string.Empty));
            //    //Preferences.Clear();
            //}
        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new LogInPage();
        }
    }
}
