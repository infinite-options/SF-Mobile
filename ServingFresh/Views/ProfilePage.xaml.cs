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

namespace ServingFresh.Views
{
    public partial class ProfilePage : ContentPage
    {
        public class UpdateProfile
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

        public UpdateProfile profile = new UpdateProfile();
        
        private bool isAddressValidated;

        public ProfilePage()
        {
            InitializeComponent();
            InitializeProfile();

            userEmailAddress.Text = (string)Application.Current.Properties["user_email"];
            userEmailAddress.TextColor = Color.Black;
            userFirstName.Text = (string)Application.Current.Properties["user_first_name"];
            userLastName.Text = (string)Application.Current.Properties["user_last_name"];
            if((string)Application.Current.Properties["platform"] != "DIRECT")
            {
                passwordCredentials.HeightRequest = 0;
            }  
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

        void InitializeProfile()
        {
            profile.customer_first_name = userFirstName.Text;
            profile.customer_last_name = userLastName.Text;
            profile.customer_email = userEmailAddress.Text;
            profile.customer_address = userAddress.Text;
            profile.customer_unit = userUnitNumber.Text ;
            profile.customer_city = userCity.Text;
            profile.customer_state = userState.Text;
            profile.customer_zip = userZipcode.Text;
            profile.customer_phone_num = userPhoneNumber.Text;
            profile.customer_lat = Application.Current.Properties["user_latitude"].ToString();
            profile.customer_long = Application.Current.Properties["user_longitude"].ToString();
            profile.customer_uid = Application.Current.Properties["user_id"].ToString();
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
            ValidateAddressClick(sender,e);
        }

        async void ValidateAddressClick(object sender, System.EventArgs e)
        {
            if (userAddress.Text != null)
            {
                profile.customer_address = userAddress.Text.Trim();
            }
            else
            {
                await DisplayAlert("Alert!", "You need to enter your address before signing up.", "OK");
            }
            if (userCity.Text != null)
            {
                profile.customer_city = userCity.Text.Trim();
            }
            else
            {
                await DisplayAlert("Alert!", "You need to enter your city before signing up.", "OK");
            }

            if (userState.Text != null)
            {
                profile.customer_state = userState.Text.Trim();
            }
            else
            {
                await DisplayAlert("Alert!", "You need to enter your state before signing up.", "OK");
            }

            if (userUnitNumber.Text == null)
            {
                profile.customer_unit = "";
            }
            else
            {
                profile.customer_unit = userUnitNumber.Text.Trim();
            }

            if (userZipcode.Text != null)
            {
                profile.customer_zip = userZipcode.Text.Trim();
            }
            else
            {
                await DisplayAlert("Alert!", "You need to enter your zipcode before signing up.", "OK");
            }

            // Setting request for USPS API
            XDocument requestDoc = new XDocument(
                new XElement("AddressValidateRequest",
                new XAttribute("USERID", "400INFIN1745"),
                new XElement("Revision", "1"),
                new XElement("Address",
                new XAttribute("ID", "0"),
                new XElement("Address1", profile.customer_address),
                new XElement("Address2", profile.customer_unit),
                new XElement("City", profile.customer_city),
                new XElement("State", profile.customer_state),
                new XElement("Zip5", profile.customer_zip),
                new XElement("Zip4", "")
                     )
                 )
             );
            var url = "http://production.shippingapis.com/ShippingAPI.dll?API=Verify&XML=" + requestDoc;
            //Console.WriteLine(url);
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
                    if (GetXMLElement(element, "DPVConfirmation").Equals("Y") && GetXMLElement(element, "Zip5").Equals(profile.customer_zip) && GetXMLElement(element, "City").Equals(profile.customer_city.ToUpper())) // Best case
                    {
                        // Get longitude and latitide because we can make a deliver here. Move on to next page.
                        // Console.WriteLine("The address you entered is valid and deliverable by USPS. We are going to get its latitude & longitude");
                        //GetAddressLatitudeLongitude();
                        Geocoder geoCoder = new Geocoder();

                        IEnumerable<Position> approximateLocations = await geoCoder.GetPositionsForAddressAsync(profile.customer_address + "," + profile.customer_city + "," + profile.customer_state);
                        Position position = approximateLocations.FirstOrDefault();

                        latitude = $"{position.Latitude}";
                        longitude = $"{position.Longitude}";

                        profile.customer_lat = latitude;
                        profile.customer_long = longitude;
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
                await DisplayAlert("We couldn't find your address", "Please check for errors.", "OK");
            }
            else
            {
                isAddressValidated = true;
                await DisplayAlert("We validated your address", "Press 'OK' to continue", "OK");
                await Application.Current.SavePropertiesAsync();

                // calling zone
                string userLat = profile.customer_lat;
                string userLong = profile.customer_long;

                var c = new HttpClient();
                var r = await c.GetAsync(Constant.ZoneUrl + userLong + "," + userLat);
                var result = await r.Content.ReadAsStringAsync();

                if (r.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<ServingFreshBusiness>(result);
                    var update = false;
                    if (result.Contains("280") && data.result.Count != 0)
                    {
                        
                        if ((string)Application.Current.Properties["zone"] == data.result[0].zone)
                        {
                            update = await DisplayAlert("Great!", "We have sucessfully updated your information", "OK", "Cancel");
                        }
                        else
                        {
                            update = await DisplayAlert("Great!", "We have sucessfully updated your information, but we will clear your cart", "Save", "Don't Save");
                        }
                    }
                    else
                    {
                        update = await DisplayAlert("Oh No", "We don't currently delivery to your new addrees. Are you sure you want to save this address?", "Save", "Don't Save");
                    }

                    if (update)
                    {
                        var updateClient = new HttpClient();
                        profile.customer_uid = (string)Application.Current.Properties["user_id"];
                        profile.customer_first_name = userFirstName.Text;
                        profile.customer_last_name = userLastName.Text;
                        profile.customer_email = userEmailAddress.Text;
                        profile.customer_phone_num = userPhoneNumber.Text;

                        var p = JsonConvert.SerializeObject(profile);
                        var content = new StringContent(p, Encoding.UTF8, "application/json");

                        System.Diagnostics.Debug.WriteLine("Sign up JSON Object: " + p);

                        //var handler = new HttpClientHandler();
                        //handler.AllowAutoRedirect = true;

                        
                        var RDSResponse = await updateClient.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/update_Profile", content);
                        Debug.WriteLine("Write to database from profile: " + RDSResponse.IsSuccessStatusCode);
                        if (RDSResponse.IsSuccessStatusCode)
                        {
                            // Update user information:
                            Application.Current.Properties["user_first_name"] = userFirstName.Text;     // 1
                            Application.Current.Properties["user_last_name"] = userLastName.Text;       // 2
                            Application.Current.Properties["user_phone_num"] = userPhoneNumber.Text;    // 3
                            Application.Current.Properties["user_address"] = userAddress.Text;          // 4
                            Application.Current.Properties["user_unit"] = userUnitNumber.Text;          // 5
                            Application.Current.Properties["user_city"] = userCity.Text;                // 6
                            Application.Current.Properties["user_state"] = userState.Text;              // 7
                            Application.Current.Properties["user_zip_code"] = userZipcode.Text;         // 8
                            Application.Current.Properties["user_latitude"] = userLat;                  // 9
                            Application.Current.Properties["user_longitude"] = userLong;                // 10
                            await DisplayAlert("Awesome!", "We have save your changes!", "OK");
                            Application.Current.MainPage = new SelectionPage();
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Oops", "Our system is down. We can't say your new information", "OK");
                }

            }
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
            Application.Current.Properties["CardNumber"] = "";
            Application.Current.Properties["CardExpMonth"] = "";
            Application.Current.Properties["CardExpYear"] = "";
            Application.Current.Properties["CardCVV"] = "";
            RemoveAppProperties();
            Application.Current.MainPage = new LogInPage();
        }

        public void RemoveAppProperties()
        {
            // Application properties would only be removed up on unstalling the app
            // It is advace to user essentials preperance instead...
            // This is why you still are able to see the time stamp and user id...
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

        async void UpdatePasswordClick(System.Object sender, System.EventArgs e)
        {
            if(userPassword.Text == userConfirmPassword.Text)
            {
                var updateClient = new HttpClient();
                var changePassword = new UpdatePassword();
                changePassword.customer_email = userEmailAddress.Text;
                changePassword.password = userPassword.Text;
                changePassword.customer_uid = (string)Application.Current.Properties["user_id"];

                var p = JsonConvert.SerializeObject(changePassword);
                var content = new StringContent(p, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine("Sign up JSON Object: " + p);

                var RDSResponse = await updateClient.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/update_email_password", content);
                if (RDSResponse.IsSuccessStatusCode)
                {
                    await DisplayAlert("Awesome!","Your password has been updated","OK");
                }
                else
                {
                    await DisplayAlert("Ooops", "Our system is down. We can't process this request at the moment", "OK");
                }
            }
            else
            {
                await DisplayAlert("Ooops","Your new passwords do not match.","OK");
            }
        }

        async void Switch_Toggled(System.Object sender, Xamarin.Forms.ToggledEventArgs e)
        {
            var notification = (Switch)sender;
            Debug.WriteLine(notification.IsToggled);

            var updateNotification = new UpdateNotification();
            updateNotification.uid = (string)Application.Current.Properties["user_id"];
            updateNotification.guid = (string)Application.Current.Properties["guid"];
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
                await DisplayAlert("Awesome!", "We've updated your notification settings", "OK");
            }
            else
            {
                await DisplayAlert("Ooops", "Our system is down. We can't process this request at the moment", "OK");
            }
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            await DisplayAlert("", "Additional feature coming soon", "Thanks");
        }
    }
}
