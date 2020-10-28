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
    public partial class GuestPage : ContentPage
    {
        public SignUpPost signUpGuest = new SignUpPost();
        public bool isAddressValidated = false;

        public GuestPage()
        {
            InitializeComponent();
            InitializeAppProperties();
            InitializeSignUpPost();
            InitializeMap();
        }

        public void InitializeMap()
        {
            map.MapType = MapType.Street;
            Position point = new Position(37.334789, -121.888138);
            var mapSpan = new MapSpan(point, 5, 5);
            map.MoveToRegion(mapSpan);
        }

        public void InitializeSignUpPost()
        {
            signUpGuest.email = "";
            signUpGuest.first_name = "";
            signUpGuest.last_name = "";
            signUpGuest.phone_number = "";
            signUpGuest.address = "";
            signUpGuest.unit = "";
            signUpGuest.city = "";
            signUpGuest.state = "";
            signUpGuest.zip_code = "";
            signUpGuest.latitude = "0.0";
            signUpGuest.longitude = "0.0";
            signUpGuest.referral_source = "MOBILE";
            signUpGuest.role = "CUSTOMER";
            signUpGuest.mobile_access_token = "FALSE";
            signUpGuest.mobile_refresh_token = "FALSE";
            signUpGuest.user_access_token = "FALSE";
            signUpGuest.user_refresh_token = "FALSE";
            signUpGuest.social = "";
            signUpGuest.password = "";
            signUpGuest.social_id = "NULL";
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

        // IF ANDROID DOESN'T LET YOU IN, THEN COMMENT EVERYTHING INSIDE THIS FUNCTION
        // EXCEPT THE LAST LINE.
        async void ValidateAddressClick(object sender, System.EventArgs e)
        {
            if (userAddress.Text != null)
            {
                signUpGuest.address = userAddress.Text.Trim();
            }
            else
            {
                await DisplayAlert("Alert!", "You need to enter your address before signing up.", "OK");
            }
            if (userCity.Text != null)
            {
                signUpGuest.city = userCity.Text.Trim();
            }
            else
            {
                await DisplayAlert("Alert!", "You need to enter your city before signing up.", "OK");
            }

            if (userState.Text != null)
            {
                signUpGuest.state= userState.Text.Trim();
            }
            else
            {
                await DisplayAlert("Alert!", "You need to enter your state before signing up.", "OK");
            }

            if (userUnitNumber.Text == null)
            {
                signUpGuest.unit = "";
            }
            else
            {
                signUpGuest.unit = userUnitNumber.Text.Trim();
            }

            if (userZipcode.Text != null)
            {
                signUpGuest.zip_code = userZipcode.Text.Trim();
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
                new XElement("Address1", signUpGuest.address),
                new XElement("Address2", signUpGuest.unit),
                new XElement("City", signUpGuest.city),
                new XElement("State", signUpGuest.state),
                new XElement("Zip5", signUpGuest.zip_code),
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
                    if (GetXMLElement(element, "DPVConfirmation").Equals("Y") && GetXMLElement(element, "Zip5").Equals(signUpGuest.zip_code) && GetXMLElement(element, "City").Equals(signUpGuest.city.ToUpper())) // Best case
                    {
                        // Get longitude and latitide because we can make a deliver here. Move on to next page.
                        // Console.WriteLine("The address you entered is valid and deliverable by USPS. We are going to get its latitude & longitude");
                        //GetAddressLatitudeLongitude();
                        Geocoder geoCoder = new Geocoder();

                        IEnumerable<Position> approximateLocations = await geoCoder.GetPositionsForAddressAsync(signUpGuest.address + "," + signUpGuest.city + "," + signUpGuest.state);
                        Position position = approximateLocations.FirstOrDefault();

                        latitude = $"{position.Latitude}";
                        longitude = $"{position.Longitude}";

                        signUpGuest.latitude = latitude;
                        signUpGuest.longitude = longitude;
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
                isAddressValidated = true;
                await DisplayAlert("We validated your address", "You can continue", "OK");
                await Application.Current.SavePropertiesAsync();
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

        void EnterEmailPasswordToSignUpClick(System.Object sender, System.EventArgs e)
        {
            // Console.WriteLine("You are about to transition to the SignUpPage");
            Application.Current.MainPage = new SignUpPage();
        }

        public async void ProceedAsGuestClick(System.Object sender, System.EventArgs e)
        {
            if (isAddressValidated)
            {
                var signUpGuestSerializedObject = JsonConvert.SerializeObject(signUpGuest);
                var content = new StringContent(signUpGuestSerializedObject, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine(signUpGuestSerializedObject);

                var signUpclient = new HttpClient();
                var RDSResponse = await signUpclient.PostAsync(Constant.SignUpUrl, content);
                var RDSMessage = await RDSResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine(RDSMessage);

                if (RDSResponse.IsSuccessStatusCode)
                {
                    if (RDSMessage.Contains(Constant.AutheticatedSuccesful))
                    {
                        var RDSData = JsonConvert.DeserializeObject<SignUpResponse>(RDSMessage);
                        DateTime today = DateTime.Now;
                        DateTime expDate = today.AddDays(Constant.days);

                        Application.Current.Properties["user_id"] = RDSData.result.customer_uid;
                        Application.Current.Properties["time_stamp"] = expDate;
                        Application.Current.Properties["platform"] = "";
                        Application.Current.Properties["user_email"] = signUpGuest.email;
                        Application.Current.Properties["user_first_name"] = signUpGuest.first_name;
                        Application.Current.Properties["user_last_name"] = signUpGuest.last_name;
                        Application.Current.Properties["user_phone_num"] = signUpGuest.phone_number;
                        Application.Current.Properties["user_address"] = signUpGuest.address;
                        Application.Current.Properties["user_unit"] = signUpGuest.unit;
                        Application.Current.Properties["user_city"] = signUpGuest.city;
                        Application.Current.Properties["user_state"] = signUpGuest.state;
                        Application.Current.Properties["user_zip_code"] = signUpGuest.zip_code;
                        Application.Current.Properties["user_latitude"] = signUpGuest.latitude;
                        Application.Current.Properties["user_longitude"] = signUpGuest.longitude;
                        Application.Current.Properties["user_delivery_instructions"] = "";

                        _ = Application.Current.SavePropertiesAsync();
                        Application.Current.MainPage = new HomePage();
                    }
                }
            }
            else
            {
                await DisplayAlert("Alert!", "We weren't able to set your information for your delivery", "OK");
            }
        }
    }
}
