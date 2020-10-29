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
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ServingFresh.Views
{
    public partial class SocialSignUp : ContentPage
    {
        public SignUpPost socialSignUp = new SignUpPost();
        public bool isAddessValidated = false;

        public SocialSignUp(string socialId, string firstName, string lastName, string emailAddress, string accessToken, string refreshToken, string platform)
        {
            InitializeComponent();
            InitializeSignUpPost();
            InitializeAppProperties();
            InitializeMap();

            userFirstName.Text = firstName;
            userLastName.Text = lastName;
            userEmailAddress.Text = emailAddress;

            socialSignUp.email = emailAddress;
            socialSignUp.first_name = firstName;
            socialSignUp.last_name = lastName;
            socialSignUp.mobile_access_token = accessToken;
            socialSignUp.mobile_refresh_token = refreshToken;
            socialSignUp.social = platform;
            socialSignUp.social_id = socialId;
        }

        public void InitializeSignUpPost()
        {
            socialSignUp.email = "";
            socialSignUp.first_name = "";
            socialSignUp.last_name = "";
            socialSignUp.phone_number = "";
            socialSignUp.address = "";
            socialSignUp.unit = "";
            socialSignUp.city = "";
            socialSignUp.state = "";
            socialSignUp.zip_code = "";
            socialSignUp.latitude = "0.0";
            socialSignUp.longitude = "0.0";
            socialSignUp.referral_source = "MOBILE";
            socialSignUp.role = "CUSTOMER";
            socialSignUp.mobile_access_token = "";
            socialSignUp.mobile_refresh_token = "";
            socialSignUp.user_access_token = "FALSE";
            socialSignUp.user_refresh_token = "FALSE";
            socialSignUp.social = "";
            socialSignUp.password = "";
            socialSignUp.social_id = "";
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
        }

        public void InitializeMap()
        {
            map.MapType = MapType.Street;
            Position point = new Position(37.334789, -121.888138);
            var mapSpan = new MapSpan(point, 5, 5);
            map.MoveToRegion(mapSpan);
        }

        async void ValidateAddressClick(object sender, System.EventArgs e)
        {
            if (userEmailAddress.Text != null)
            {
                socialSignUp.email = userEmailAddress.Text.ToLower().Trim();
            }
            else
            {
                await DisplayAlert("Error", "Please enter a valid email address.", "OK");
            }

            if (userFirstName.Text != null)
            {
                socialSignUp.first_name = userFirstName.Text.Trim();
            }
            else
            {
                await DisplayAlert("Error", "Please your first name.", "OK");
            }

            if (userLastName.Text != null)
            {
                socialSignUp.last_name = userLastName.Text.Trim();
            }
            else
            {
                await DisplayAlert("Error", "Please your first name.", "OK");
            }

            if (userAddress.Text != null)
            {
                socialSignUp.address = userAddress.Text.Trim();
            }
            else
            {
                await DisplayAlert("Error", "Please enter your address", "OK");
            }

            if (userUnitNumber.Text != null)
            {
                socialSignUp.unit = userUnitNumber.Text.Trim();
            }

            if (userCity.Text != null)
            {
                socialSignUp.city = userCity.Text.Trim();
            }
            else
            {
                await DisplayAlert("Error", "Please enter your city", "OK");
            }

            if (userState.Text != null)
            {
                socialSignUp.state = userState.Text.Trim();
            }
            else
            {
                await DisplayAlert("Error", "Please enter your state", "OK");
            }

            if (userZipcode.Text != null)
            {
                socialSignUp.zip_code = userZipcode.Text.Trim();
            }
            else
            {
                await DisplayAlert("Error", "Please enter your zipcode", "OK");
            }

            if (userPhoneNumber.Text != null && userPhoneNumber.Text.Length == 10)
            {
                socialSignUp.phone_number = userPhoneNumber.Text.Trim();
            }
            else
            {
                await DisplayAlert("Error", "Please enter your zipcode", "OK");
            }

            if (userDeliveryInstructions.Text == null)
            {
                userDeliveryInstructions.Text = "";
            }

            // Setting request for USPS API
            XDocument requestDoc = new XDocument(
                new XElement("AddressValidateRequest",
                new XAttribute("USERID", "400INFIN1745"),
                new XElement("Revision", "1"),
                new XElement("Address",
                new XAttribute("ID", "0"),
                new XElement("Address1", socialSignUp.address),
                new XElement("Address2", socialSignUp.unit),
                new XElement("City", socialSignUp.city),
                new XElement("State", socialSignUp.state),
                new XElement("Zip5", socialSignUp.zip_code),
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
                    if (GetXMLElement(element, "DPVConfirmation").Equals("Y") && GetXMLElement(element, "Zip5").Equals(socialSignUp.zip_code) && GetXMLElement(element, "City").Equals(socialSignUp.city.ToUpper())) // Best case
                    {
                        // Get longitude and latitide because we can make a deliver here. Move on to next page.
                        // Console.WriteLine("The address you entered is valid and deliverable by USPS. We are going to get its latitude & longitude");
                        //GetAddressLatitudeLongitude();
                        Geocoder geoCoder = new Geocoder();

                        IEnumerable<Position> approximateLocations = await geoCoder.GetPositionsForAddressAsync(socialSignUp.address + "," + socialSignUp.city + "," + socialSignUp.state);
                        Position position = approximateLocations.FirstOrDefault();

                        latitude = $"{position.Latitude}";
                        longitude = $"{position.Longitude}";

                        socialSignUp.latitude = latitude;
                        socialSignUp.longitude = longitude;
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

        async void SignUpNewUser(System.Object sender, System.EventArgs e)
        {
            if (isAddessValidated)
            {
                var socialSignUpSerializedObject = JsonConvert.SerializeObject(socialSignUp);
                var content = new StringContent(socialSignUpSerializedObject, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine(socialSignUpSerializedObject);

                var signUpclient = new HttpClient();
                var RDSResponse = await signUpclient.PostAsync(Constant.SignUpUrl, content);
                var RDSMessage = await RDSResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine(RDSMessage);
                if (RDSResponse.IsSuccessStatusCode)
                {
                    var RDSData = JsonConvert.DeserializeObject<SignUpResponse>(RDSMessage);
                    DateTime today = DateTime.Now;
                    DateTime expDate = today.AddDays(Constant.days);

                    Application.Current.Properties["user_id"] = RDSData.result.customer_uid;
                    Application.Current.Properties["time_stamp"] = expDate;
                    Application.Current.Properties["platform"] = socialSignUp.social;
                    Application.Current.Properties["user_email"] = socialSignUp.email;
                    Application.Current.Properties["user_first_name"] = socialSignUp.first_name;
                    Application.Current.Properties["user_last_name"] = socialSignUp.last_name;
                    Application.Current.Properties["user_phone_num"] = socialSignUp.phone_number;
                    Application.Current.Properties["user_address"] = socialSignUp.address;
                    Application.Current.Properties["user_unit"] = socialSignUp.unit;
                    Application.Current.Properties["user_city"] = socialSignUp.city;
                    Application.Current.Properties["user_state"] = socialSignUp.state;
                    Application.Current.Properties["user_zip_code"] = socialSignUp.zip_code;
                    Application.Current.Properties["user_latitude"] = socialSignUp.latitude;
                    Application.Current.Properties["user_longitude"] = socialSignUp.longitude;
                    Application.Current.Properties["user_delivery_instructions"] = userDeliveryInstructions.Text;

                    _ = Application.Current.SavePropertiesAsync();
                    Application.Current.MainPage = new SelectionPage();
                }
            }
            else
            {
                await DisplayAlert("Message", "We weren't able to sign you up", "OK");
            }
        }
    }
}
