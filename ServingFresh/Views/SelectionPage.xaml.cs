using System;
using System.Collections.Generic;
using Xamarin.Forms;
using ServingFresh.Config;
using ServingFresh.Effects;
using ServingFresh.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Essentials;
using System.Diagnostics;
using Plugin.LatestVersion;
using Xamarin.Auth;
using ServingFresh.LogIn.Apple;
using Acr.UserDialogs;
using ServingFresh.LogIn.Classes;
using System.Text;
using System.Threading.Tasks;

namespace ServingFresh.Views
{
    public partial class SelectionPage : ContentPage
    {
        class BusinessCard
        {
            public string business_image { get; set; }
            public string business_name { get; set; }
            //public string item_type { get; set; }
            public string business_uid { get; set; }
            //public string business_type { get; set; }
            public IDictionary<string,IList<string>> list_delivery_days { get; set; }
            public Color border_color { get; set; }
        }


        BusinessCard unselectedBusiness(Business b)
        {
            return new BusinessCard()
            {
                business_image = b.business_image,
                business_name = b.business_name,
                //item_type = b.item_type,
                business_uid = b.z_biz_id,
                //business_type = b.business_type,
                list_delivery_days = b.delivery_days,
                border_color = Color.LightGray
            };
        }

        BusinessCard selectedBusiness(Business b)
        {
            return new BusinessCard()
            {
                business_image = b.business_image,
                business_name = b.business_name,
                //item_type = b.item_type,
                business_uid = b.z_biz_id,
                //business_type = b.business_type,
                list_delivery_days = b.delivery_days,
                border_color = Constants.SecondaryColor
            };
        }

        public class AcceptingSchedule
        {
            public IList<string> Friday { get; set; }
            public IList<string> Monday { get; set; }
            public IList<string> Sunday { get; set; }
            public IList<string> Tuesday { get; set; }
            public IList<string> Saturday { get; set; }
            public IList<string> Thursday { get; set; }
            public IList<string> Wednesday { get; set; }
        }

        List<DeliveriesModel> AllDeliveries = new List<DeliveriesModel>();
        List<Business> AllFarms = new List<Business>();
        List<Business> AllFarmersMarkets = new List<Business>();
        List<Business> OpenFarms = new List<Business>();

        ObservableCollection<DeliveriesModel> Deliveries = new ObservableCollection<DeliveriesModel>();
        ObservableCollection<BusinessCard> Farms = new ObservableCollection<BusinessCard>();
        ObservableCollection<BusinessCard> FarmersMarkets = new ObservableCollection<BusinessCard>();

        List<string> types = new List<string>();
        List<string> b_uids = new List<string>();
        string selected_market_id = "";
        string selected_farm_id = "";

        public class DeliveryInfo
        {
            public string delivery_day { get; set; }
            public string delivery_time { get; set; }
        }

        public class Delivery
        {
            public string delivery_date { get; set; }
            public string delivery_shortname { get; set; }
            public string delivery_dayofweek { get; set; }
            public string delivery_time { get; set; }
        }

        public class ScheduleInfo
        {
            public string delivery_date { get; set; }
            public string delivery_shortname { get; set; }
            public string delivery_dayofweek { get; set; }
            public string delivery_time { get; set; }
            public List<string> business_uids { get; set; }
            public DateTime deliveryTimeStamp { get; set; }
            public string orderExpTime { get; set; }
        }

        List<DeliveryInfo> deliveryDays = new List<DeliveryInfo>();
        List<string> deliveryDayList = new List<string>();
        List<DeliveriesModel> deliveryScheduleUnfiltered = new List<DeliveriesModel>();
        List<Delivery> deliveryScheduleFiltered = new List<Delivery>();
        List<ScheduleInfo> schedule = new List<ScheduleInfo>();
        List<string> businessList = new List<string>();
        List<BusinessCard> businesses = new List<BusinessCard>();
        List<BusinessCard> business = new List<BusinessCard>();
        List<ScheduleInfo> copy = new List<ScheduleInfo>();
        List<DateTime> deliverySchedule = new List<DateTime>();
        List<ScheduleInfo> displaySchedule = new List<ScheduleInfo>();
        ServingFreshBusiness data = new ServingFreshBusiness();
        private string deviceId;

        // THIS SELECTION PAGE IS USE IN ALL LOGINS ONLY

        //public DeliveriesPage(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs e = null, AppleAccount account = null, string platform = "")
        //{
        //    InitializeComponent();
        //    UserDialogs.Instance.ShowLoading("Please wait while we are processing your request...");
        //    SetHeightWidthOnMap();
        //    SetWidthOnHelpButtonRow();
        //    SetDefaultLocationOnMap();
        //    BackupDisplay.Margin = new Thickness(0, Application.Current.MainPage.Height, 0, 0);
        //    if (platform == "GOOGLE")
        //    {
        //        VerifyUserAccount(accessToken, refreshToken, e, null, "GOOGLE");
        //    }
        //    else if (platform == "FACEBOOK")
        //    {
        //        VerifyUserAccount(accessToken, "", null, null, "FACEBOOK");
        //    }
        //    else if (platform == "APPLE")
        //    {
        //        VerifyUserAccount(account.UserId, "", null, account, "APPLE");
        //    }
        //}

        public SelectionPage(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs googleCredentials = null, AppleAccount appleCredentials = null, string platform = "")
        {
            InitializeComponent();
            _ = VerifyUserCredentials(accessToken, refreshToken, googleCredentials, appleCredentials, platform);
        }

        static async Task WaitAndApologizeAsync()
        {
            await Task.Delay(2000);
        }

        public async Task VerifyUserCredentials(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs googleAccount = null, AppleAccount appleCredentials = null, string platform= "")
        {
            try
            {
                //var progress = UserDialogs.Instance.Loading("Loading...");
                var client = new HttpClient();
                var socialLogInPost = new SocialLogInPost();

                var googleData = new GoogleResponse();
                var facebookData = new FacebookResponse();

                if (platform == "GOOGLE")
                {
                    var request = new OAuth2Request("GET", new Uri(Constant.GoogleUserInfoUrl), null, googleAccount.Account);
                    var GoogleResponse = await request.GetResponseAsync();
                    var googelUserData = GoogleResponse.GetResponseText();

                    googleData = JsonConvert.DeserializeObject<GoogleResponse>(googelUserData);

                    socialLogInPost.email = googleData.email;
                    socialLogInPost.social_id = googleData.id;
                }
                else if (platform == "FACEBOOK")
                {
                    var facebookResponse = client.GetStringAsync(Constant.FacebookUserInfoUrl + accessToken);
                    var facebookUserData = facebookResponse.Result;

                    facebookData = JsonConvert.DeserializeObject<FacebookResponse>(facebookUserData);

                    socialLogInPost.email = facebookData.email;
                    socialLogInPost.social_id = facebookData.id;
                }
                else if (platform == "APPLE")
                {
                    socialLogInPost.email = appleCredentials.Email;
                    socialLogInPost.social_id = appleCredentials.UserId;
                }

                socialLogInPost.password = "";
                socialLogInPost.signup_platform = platform;

                var socialLogInPostSerialized = JsonConvert.SerializeObject(socialLogInPost);
                var postContent = new StringContent(socialLogInPostSerialized, Encoding.UTF8, "application/json");

                var test = UserDialogs.Instance.Loading("Loading...");
                var RDSResponse = await client.PostAsync(Constant.LogInUrl, postContent);
                var responseContent = await RDSResponse.Content.ReadAsStringAsync();
                var authetication = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);
                if (RDSResponse.IsSuccessStatusCode)
                {
                    if (responseContent != null)
                    {
                        if (authetication.code.ToString() == Constant.EmailNotFound)
                        {
                            test.Hide();
                            if(platform == "GOOGLE")
                            {
                                Application.Current.MainPage = new SocialSignUp(googleData.id, googleData.given_name, googleData.family_name, googleData.email, accessToken, refreshToken, "GOOGLE");
                            }
                            else if (platform == "FACEBOOK")
                            {
                                Application.Current.MainPage = new SocialSignUp(facebookData.id, facebookData.name, "", facebookData.email, accessToken, accessToken, "FACEBOOK");
                            }
                            else if (platform == "APPLE")
                            {
                                Application.Current.MainPage = new SocialSignUp(appleCredentials.UserId, appleCredentials.Name, "", appleCredentials.Email, appleCredentials.Token, appleCredentials.Token, "APPLE");
                            }
                        }
                        if (authetication.code.ToString() == Constant.AutheticatedSuccesful)
                        {
                            try
                            {
                                var data = JsonConvert.DeserializeObject<SuccessfulSocialLogIn>(responseContent);
                                Application.Current.Properties["user_id"] = data.result[0].customer_uid;

                                UpdateTokensPost updateTokesPost = new UpdateTokensPost();
                                updateTokesPost.uid = data.result[0].customer_uid;
                                if (platform == "GOOGLE")
                                {
                                    updateTokesPost.mobile_access_token = accessToken;
                                    updateTokesPost.mobile_refresh_token = refreshToken;
                                }
                                else if (platform == "FACEBOOK")
                                {
                                    updateTokesPost.mobile_access_token = accessToken;
                                    updateTokesPost.mobile_refresh_token = accessToken;
                                }else if (platform == "APPLE")
                                {
                                    updateTokesPost.mobile_access_token = appleCredentials.Token;
                                    updateTokesPost.mobile_refresh_token = appleCredentials.Token;
                                }

                                var updateTokesPostSerializedObject = JsonConvert.SerializeObject(updateTokesPost);
                                var updateTokesContent = new StringContent(updateTokesPostSerializedObject, Encoding.UTF8, "application/json");
                                var updateTokesResponse = await client.PostAsync(Constant.UpdateTokensUrl, updateTokesContent);
                                var updateTokenResponseContent = await updateTokesResponse.Content.ReadAsStringAsync();

                                if (updateTokesResponse.IsSuccessStatusCode)
                                {
                                    var user = new RequestUserInfo();
                                    user.uid = data.result[0].customer_uid;

                                    var requestSelializedObject = JsonConvert.SerializeObject(user);
                                    var requestContent = new StringContent(requestSelializedObject, Encoding.UTF8, "application/json");

                                    var clientRequest = await client.PostAsync(Constant.GetUserInfoUrl, requestContent);

                                    if (clientRequest.IsSuccessStatusCode)
                                    {
                                        var userSfJSON = await clientRequest.Content.ReadAsStringAsync();
                                        var userProfile = JsonConvert.DeserializeObject<UserInfo>(userSfJSON);

                                        DateTime today = DateTime.Now;
                                        DateTime expDate = today.AddDays(Constant.days);

                                        Application.Current.Properties["user_id"] = data.result[0].customer_uid;
                                        Application.Current.Properties["time_stamp"] = expDate;
                                        Application.Current.Properties["platform"] = platform;
                                        Application.Current.Properties["user_email"] = userProfile.result[0].customer_email;
                                        Application.Current.Properties["user_first_name"] = userProfile.result[0].customer_first_name;
                                        Application.Current.Properties["user_last_name"] = userProfile.result[0].customer_last_name;
                                        Application.Current.Properties["user_phone_num"] = userProfile.result[0].customer_phone_num;
                                        Application.Current.Properties["user_address"] = userProfile.result[0].customer_address;
                                        Application.Current.Properties["user_unit"] = userProfile.result[0].customer_unit;
                                        Application.Current.Properties["user_city"] = userProfile.result[0].customer_city;
                                        Application.Current.Properties["user_state"] = userProfile.result[0].customer_state;
                                        Application.Current.Properties["user_zip_code"] = userProfile.result[0].customer_zip;
                                        Application.Current.Properties["user_latitude"] = userProfile.result[0].customer_lat;
                                        Application.Current.Properties["user_longitude"] = userProfile.result[0].customer_long;

                                        _ = Application.Current.SavePropertiesAsync();
                                        await CheckVersion();

                                        if (Device.RuntimePlatform == Device.iOS)
                                        {
                                            deviceId = Preferences.Get("guid", null);
                                            if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Log in: " + deviceId); }
                                        }
                                        else
                                        {
                                            deviceId = Preferences.Get("guid", null);
                                            if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Log in " + deviceId); }
                                        }

                                        if (deviceId != null)
                                        {
                                            NotificationPost notificationPost = new NotificationPost();

                                            notificationPost.uid = (string)Application.Current.Properties["user_id"];
                                            notificationPost.guid = deviceId.Substring(5);
                                            Application.Current.Properties["guid"] = deviceId.Substring(5);
                                            notificationPost.notification = "TRUE";

                                            var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                                            Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                                            var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

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
                                        test.Hide();
                                        //Application.Current.MainPage = new SelectionPage();
                                    }
                                    else
                                    {
                                        test.Hide();
                                        await DisplayAlert("Alert!", "Our internal system was not able to retrieve your user information. We are working to solve this issue.", "OK");
                                    }
                                }
                                else
                                {
                                    test.Hide();
                                    await DisplayAlert("Oops", "We are facing some problems with our internal system. We weren't able to update your credentials", "OK");
                                }
                                test.Hide();
                            }
                            catch (Exception second)
                            {
                                Debug.WriteLine(second.Message);
                            }
                        }
                        if (authetication.code.ToString() == Constant.ErrorPlatform)
                        {
                            var RDSCode = JsonConvert.DeserializeObject<RDSLogInMessage>(responseContent);
                            test.Hide();
                            Application.Current.MainPage = new LogInPage("Message", RDSCode.message);
                        }

                        if (authetication.code.ToString() == Constant.ErrorUserDirectLogIn)
                        {
                            test.Hide();
                            Application.Current.MainPage = new LogInPage("Oops!", "You have an existing Serving Fresh account. Please use direct login");
                        }
                    }
                }
                test.Hide();
            }
            catch (Exception first)
            {
                Debug.WriteLine(first.Message);
            }
        }


        public SelectionPage()
        {
            InitializeComponent();
            _ = CheckVersion();
        }

        public async Task CheckVersion()
        {
            var isLatest = await CrossLatestVersion.Current.IsUsingLatestVersion();
            
            if (!isLatest)
            {
                await DisplayAlert("Serving Fresh\nhas gotten even better!", "Please visit the App Store to get the latest version.", "OK");
                await CrossLatestVersion.Current.OpenAppInStore();
            }
            else
            {
                _ = GetBusinesses();
                Application.Current.Properties["zone"] = "";
                Application.Current.Properties["day"] = "";
                Application.Current.Properties["deliveryDate"] = "";
                CartTotal.Text = CheckoutPage.total_qty.ToString();
            }
        }
 
        public async Task GetBusinesses()
        {
            //var progress = UserDialogs.Instance.Loading("Loading...");
            var userLat = (string)Application.Current.Properties["user_latitude"];
            var userLong = (string)Application.Current.Properties["user_longitude"];

            if (userLat == "0" && userLong == "0"){ userLong = "-121.8866517"; userLat = "37.2270928";}

            var client = new HttpClient();
            var response = await client.GetAsync(Constant.ZoneUrl + userLong + "," + userLat);
            var result = await response.Content.ReadAsStringAsync();
            //Debug.WriteLine("ZONE URL: "Constant.ZoneUrl + userLong + "," + userLat);
            //Debug.WriteLine("LIST OF FARMS: " + result);

            if (response.IsSuccessStatusCode)
            {

                data = JsonConvert.DeserializeObject<ServingFreshBusiness>(result);

                var currentDate = DateTime.Now;
                var tempDateTable = GetTable(currentDate);

                //Debug.WriteLine("TEMP TABLE FOR LOOK UPS");
                //foreach(DateTime t in tempDateTable)
                //{
                //    Debug.WriteLine(t);
                //}

                foreach (Business a in data.result)
                {
                    var acceptingDate = LastAcceptingOrdersDate(tempDateTable, a.z_accepting_day, a.z_accepting_time);
                    var deliveryDate = new DateTime();
                    //Debug.WriteLine("CURRENT DATE: " + currentDate);
                    //Debug.WriteLine("LAST ACCEPTING DATE: " + acceptingDate);

                    if (currentDate < acceptingDate)
                    {
                        //Debug.WriteLine("ONTIME");

                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);
                        if (deliveryDate < acceptingDate)
                        {
                            deliveryDate = deliveryDate.AddDays(7);
                        }
                        if (!deliverySchedule.Contains(deliveryDate))
                        {
                            var element = new ScheduleInfo();

                            element.business_uids = new List<string>();
                            element.business_uids.Add(a.z_biz_id);
                            element.delivery_date = deliveryDate.ToString("MMM dd");
                            element.delivery_dayofweek = deliveryDate.DayOfWeek.ToString();
                            element.delivery_shortname = deliveryDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                            element.delivery_time = a.z_delivery_time;
                            element.deliveryTimeStamp = deliveryDate;
                            element.orderExpTime = "Order by " + acceptingDate.ToString("ddd") + " " + acceptingDate.ToString("htt").ToLower();

                            //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            //Debug.WriteLine("business_uids list: ");

                            //foreach(string ID in element.business_uids)
                            //{
                            //    Debug.WriteLine(ID);
                            //}

                            deliverySchedule.Add(deliveryDate);
                            displaySchedule.Add(element);
                        }
                        else
                        {
                            foreach(ScheduleInfo element in displaySchedule)
                            {
                                if(element.deliveryTimeStamp == deliveryDate)
                                {
                                    var e = element;

                                    e.business_uids.Add(a.z_biz_id);
                                    element.business_uids = e.business_uids;

                                    //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    //Debug.WriteLine("business_uids list: ");

                                    //foreach (string ID in element.business_uids)
                                    //{
                                    //    Debug.WriteLine(ID);
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("NON ONTIME! -> ROLL OVER TO NEXT DELIVERY DATE");

                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);

                        if (!deliverySchedule.Contains(deliveryDate.AddDays(7)))
                        {
                            var nextDeliveryDate = deliveryDate.AddDays(7);
                            var element = new ScheduleInfo();

                            element.business_uids = new List<string>();
                            element.business_uids.Add(a.z_biz_id);
                            element.delivery_date = nextDeliveryDate.ToString("MMM dd");
                            element.delivery_dayofweek = nextDeliveryDate.DayOfWeek.ToString();
                            element.delivery_shortname = nextDeliveryDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                            element.delivery_time = a.z_delivery_time;
                            element.deliveryTimeStamp = nextDeliveryDate;
                            element.orderExpTime = "Order by " + acceptingDate.AddDays(7).ToString("ddd") + " " + acceptingDate.AddDays(7).ToString("htt").ToLower();

                            //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            //Debug.Write("business_uids list: ");

                            //foreach (string ID in element.business_uids)
                            //{
                            //    Debug.Write(ID + ", ");
                            //}

                            deliverySchedule.Add(nextDeliveryDate);
                            displaySchedule.Add(element);
                        }
                        else
                        {
                            var nextDeliveryDate = deliveryDate.AddDays(7);

                            foreach (ScheduleInfo element in displaySchedule)
                            {
                                if (element.deliveryTimeStamp == nextDeliveryDate)
                                {
                                    var e = element;

                                    e.business_uids.Add(a.z_biz_id);
                                    element.business_uids = e.business_uids;

                                    //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    //Debug.Write("business_uids list: ");

                                    //foreach (string ID in element.business_uids)
                                    //{
                                    //    Debug.Write(ID + ", ");
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                }

                // DISPLAY SCHEDULE ELEMENTS;
                //Debug.WriteLine("");
                //Debug.WriteLine("");
                //Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS NOT SORTED");
                //Debug.WriteLine("");

                //foreach (ScheduleInfo element in displaySchedule)
                //{
                //    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                //    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                //    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                //    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                //    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                //    Debug.Write("business_uids list: ");

                //    foreach (string ID in element.business_uids)
                //    {
                //        Debug.Write(ID + ", ");
                //    }

                //    Debug.WriteLine("");
                //    Debug.WriteLine("");
                //}

                deliverySchedule.Sort();
                List<ScheduleInfo> sortedSchedule = new List<ScheduleInfo>();

                foreach(DateTime deliveryElement in deliverySchedule)
                {
                    foreach(ScheduleInfo scheduleElement in displaySchedule)
                    {
                        if(deliveryElement == scheduleElement.deliveryTimeStamp)
                        {
                            sortedSchedule.Add(scheduleElement);
                        }
                    }
                }

                displaySchedule = sortedSchedule;

                //Debug.WriteLine("");
                //Debug.WriteLine("");
                //Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS SORTED");
                //Debug.WriteLine("");

                //foreach (ScheduleInfo element in displaySchedule)
                //{
                //    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                //    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                //    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                //    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                //    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                //    Debug.Write("business_uids list: ");

                //    foreach (string ID in element.business_uids)
                //    {
                //        Debug.Write(ID + ", ");
                //    }

                //    Debug.WriteLine("");
                //    Debug.WriteLine("");
                //}

                if (result.Contains("280") && data.result.Count != 0)
                {
                    // Parse it
                    Application.Current.Properties["zone"] = data.result[0].zone;
                    //Debug.WriteLine("Zone to save: " + Application.Current.Properties["zone"]);
                    //Debug.WriteLine("Parsing Data");
                    deliveryDays.Clear();
                    businessList.Clear();

                    foreach (Business business in data.result)
                    {
                        DeliveryInfo element = new DeliveryInfo();
                        element.delivery_day = business.z_delivery_day;
                        element.delivery_time = business.z_delivery_time;

                        bool addElement = false;

                        if (deliveryDays.Count != 0)
                        {
                            foreach(DeliveryInfo i in deliveryDays)
                            {
                                if(element.delivery_time == i.delivery_time && element.delivery_day == i.delivery_day)
                                {
                                    addElement = true;
                                    break;
                                }
                            }
                            if (!addElement)
                            {
                                deliveryDays.Add(element);
                            }
                        }
                        else
                        {
                            deliveryDays.Add(element);
                        }

                        if (!businessList.Contains(business.z_biz_id))
                        {
                            businessList.Add(business.z_biz_id);
                        }
                    }

                    foreach (string id in businessList)
                    {
                        //Debug.WriteLine(id + " :");
                        IDictionary<string,IList<string>> days = new Dictionary<string,IList<string>>();
                        foreach(Business b in data.result)
                        {
                            if(id == b.z_biz_id)
                            {
                                if (!days.ContainsKey(b.z_delivery_day.ToUpper()))
                                {
                                    IList<string> times = new List<string>();
                                    times.Add(b.z_delivery_time);
                                    days.Add(b.z_delivery_day.ToUpper(), times);
                                }
                                else
                                {
                                    List<string> times = (List<string>)days[b.z_delivery_day.ToUpper()];
                                    times.Add(b.z_delivery_time);
                                    days[b.z_delivery_day.ToUpper()] = times;
                                }
                               
                            }
                        }

                        foreach(Business i in data.result)
                        {
                            if(id == i.z_biz_id)
                            {
                                i.delivery_days = days;
                                businesses.Add(unselectedBusiness(i));
                                break;
                            }
                        }
                    }

                    //foreach(DeliveryInfo i in deliveryDays)
                    //{
                    //    Debug.WriteLine(i.delivery_day);
                    //    Debug.WriteLine(i.delivery_time);
                    //}

                    //GetDays();
                    delivery_list.ItemsSource = displaySchedule;
                    farm_list.ItemsSource = businesses;
                }
                else
                {
                    await DisplayAlert("Oops", "We don't have a business that can delivery to your location at the moment", "OK");
                    return;
                }
            }
            else
            {
                //await DisplayAlert("Oops!", "Our system is down. We are working to fix this issue.", "OK");
                return;
            }
        }

        public List<DateTime> GetBusinessSchedule(ServingFreshBusiness data, string businessID)
        {
            var currentDate = DateTime.Now;
            var tempDateTable = GetTable(currentDate);
            var businesDeliverySchedule = new List<DateTime>();
            var businesDisplaySchedule = new List<ScheduleInfo>();

            //Debug.WriteLine("TEMP TABLE FOR LOOK UPS");
            //foreach (DateTime t in tempDateTable)
            //{
            //    Debug.WriteLine(t);
            //}

            foreach (Business a in data.result)
            {
                if(businessID == a.z_biz_id)
                {
                    var acceptingDate = LastAcceptingOrdersDate(tempDateTable, a.z_accepting_day, a.z_accepting_time);
                    var deliveryDate = new DateTime();
                    //Debug.WriteLine("CURRENT DATE: " + currentDate);
                    //Debug.WriteLine("LAST ACCEPTING DATE: " + acceptingDate);

                    if (currentDate < acceptingDate)
                    {
                        //Debug.WriteLine("ONTIME");
                        
                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);
                        if(deliveryDate < acceptingDate)
                        {
                            deliveryDate = deliveryDate.AddDays(7);
                        }

                        if (!businesDeliverySchedule.Contains(deliveryDate))
                        {
                            var element = new ScheduleInfo();

                            element.business_uids = new List<string>();
                            element.business_uids.Add(a.z_biz_id);
                            element.delivery_date = deliveryDate.ToString("MMM dd");
                            element.delivery_dayofweek = deliveryDate.DayOfWeek.ToString();
                            element.delivery_shortname = deliveryDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                            element.delivery_time = a.z_delivery_time;
                            element.deliveryTimeStamp = deliveryDate;
                            element.orderExpTime = "Order by " + acceptingDate.ToString("ddd") + " " + acceptingDate.ToString("htt").ToLower();

                            //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            //Debug.WriteLine("business_uids list: ");

                            //foreach (string ID in element.business_uids)
                            //{
                            //    Debug.WriteLine(ID);
                            //}

                            businesDeliverySchedule.Add(deliveryDate);
                            businesDisplaySchedule.Add(element);
                        }
                        else
                        {
                            foreach (ScheduleInfo element in businesDisplaySchedule)
                            {
                                if (element.deliveryTimeStamp == deliveryDate)
                                {
                                    var e = element;

                                    e.business_uids.Add(a.z_biz_id);
                                    element.business_uids = e.business_uids;

                                    //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    //Debug.WriteLine("business_uids list: ");

                                    //foreach (string ID in element.business_uids)
                                    //{
                                    //    Debug.WriteLine(ID);
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("NON ONTIME! -> ROLL OVER TO NEXT DELIVERY DATE");

                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);

                        if (!businesDeliverySchedule.Contains(deliveryDate.AddDays(7)))
                        {
                            var nextDeliveryDate = deliveryDate.AddDays(7);
                            var element = new ScheduleInfo();

                            element.business_uids = new List<string>();
                            element.business_uids.Add(a.z_biz_id);
                            element.delivery_date = nextDeliveryDate.ToString("MMM dd");
                            element.delivery_dayofweek = nextDeliveryDate.DayOfWeek.ToString();
                            element.delivery_shortname = nextDeliveryDate.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                            element.delivery_time = a.z_delivery_time;
                            element.deliveryTimeStamp = nextDeliveryDate;
                            element.orderExpTime = "Order by " + acceptingDate.AddDays(7).ToString("ddd") + " " + acceptingDate.AddDays(7).ToString("htt").ToLower();

                            //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            //Debug.Write("business_uids list: ");

                            //foreach (string ID in element.business_uids)
                            //{
                            //    Debug.Write(ID + ", ");
                            //}

                            businesDeliverySchedule.Add(nextDeliveryDate);
                            businesDisplaySchedule.Add(element);
                        }
                        else
                        {
                            var nextDeliveryDate = deliveryDate.AddDays(7);

                            foreach (ScheduleInfo element in businesDisplaySchedule)
                            {
                                if (element.deliveryTimeStamp == nextDeliveryDate)
                                {
                                    var e = element;

                                    e.business_uids.Add(a.z_biz_id);
                                    element.business_uids = e.business_uids;

                                    //Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    //Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    //Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    //Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    //Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    //Debug.Write("business_uids list: ");

                                    //foreach (string ID in element.business_uids)
                                    //{
                                    //    Debug.Write(ID + ", ");
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // DISPLAY SCHEDULE ELEMENTS;
            //Debug.WriteLine("");
            //Debug.WriteLine("");
            //Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS NOT SORTED");
            //Debug.WriteLine("");

            //foreach (ScheduleInfo element in businesDisplaySchedule)
            //{
            //    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
            //    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
            //    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
            //    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
            //    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
            //    Debug.Write("business_uids list: ");

            //    foreach (string ID in element.business_uids)
            //    {
            //        Debug.Write(ID + ", ");
            //    }

            //    Debug.WriteLine("");
            //    Debug.WriteLine("");
            //}

            businesDeliverySchedule.Sort();
            List<ScheduleInfo> sortedSchedule = new List<ScheduleInfo>();

            foreach (DateTime deliveryElement in deliverySchedule)
            {
                foreach (ScheduleInfo scheduleElement in businesDisplaySchedule)
                {
                    if (deliveryElement == scheduleElement.deliveryTimeStamp)
                    {
                        sortedSchedule.Add(scheduleElement);
                    }
                }
            }

            businesDisplaySchedule = sortedSchedule;

            //Debug.WriteLine("");
            //Debug.WriteLine("");
            //Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS SORTED");
            //Debug.WriteLine("");

            //foreach (ScheduleInfo element in businesDisplaySchedule)
            //{
            //    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
            //    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
            //    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
            //    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
            //    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
            //    Debug.Write("business_uids list: ");

            //    foreach (string ID in element.business_uids)
            //    {
            //        Debug.Write(ID + ", ");
            //    }

            //    Debug.WriteLine("");
            //    Debug.WriteLine("");
            //}
            return businesDeliverySchedule;
        }

        public List<DateTime> GetTable(DateTime today)
        {
            var table = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                table.Add(today);
                today = today.AddDays(1);
            }
            return table;
        }

        public DateTime LastAcceptingOrdersDate(List<DateTime> table, string acceptingDay, string acceptingTime)
        {
            var date = DateTime.Parse(acceptingTime);

            foreach(DateTime element in table)
            {
                if (element.DayOfWeek.ToString().ToUpper() == acceptingDay)
                {
                    break;
                }
                date = date.AddDays(1);
            }

            return date;
        }

        public DateTime BussinesDeliveryDate(DateTime lastAcceptingOrdersDate, string day, string time)
        {
            string startTime = "";

            foreach (char a in time.ToCharArray())
            {
                if (a != '-')
                {
                    startTime += a;
                }
                else
                {
                    break;
                }
            }

            var deliveryDate = DateTime.Parse(startTime.Trim());
            //Debug.WriteLine("DELIVERYY DATE IN BUSINESS" + deliveryDate);
            //Debug.WriteLine("LAST ACCEPTING DATE IN BUSINESS" + lastAcceptingOrdersDate);
            if (deliveryDate < lastAcceptingOrdersDate)
            {
                deliveryDate = deliveryDate.AddDays(7);
            }
            //Debug.WriteLine("DEFAULT DELIVERY DATE: " + deliveryDate);

            for (int i = 0; i < 7; i++)
            {
                if (deliveryDate.DayOfWeek.ToString().ToUpper() == day.ToUpper())
                {
                    break;
                }
                deliveryDate = deliveryDate.AddDays(1);
            }

            //Debug.WriteLine("DELIVERY DATE: " + deliveryDate);
            return deliveryDate;
        }


        public DateTime lastAcceptingOrdersDate(string day, string hour)
        {
            var acceptingDate = DateTime.Parse(hour);

            //Debug.WriteLine("DEFAULT ACCEPTING ORDERS DATE: " + acceptingDate);
            //Debug.WriteLine("LAST ACCEPTING DAY: " + day.ToUpper());

            for (int i = 0; i < 7; i++)
            {
                if (acceptingDate.DayOfWeek.ToString().ToUpper() == day.ToUpper())
                {
                    break;
                }
                acceptingDate = acceptingDate.AddDays(1);
            }

            //Debug.WriteLine("LAST ACCEPTING ORDERS DATE: " + acceptingDate);
            return acceptingDate;
        }

        public DateTime bussinesDeliveryDate(string day, string time)
        {
            string startTime = "";

            foreach (char a in time.ToCharArray())
            {
                if (a != '-')
                {
                    startTime += a;
                }
                else
                {
                    break;
                }
            }

            var deliveryDate = DateTime.Parse(startTime.Trim());
            //Debug.WriteLine("DEFAULT DELIVERY DATE: " + deliveryDate);

            for (int i = 0; i < 7; i++)
            {
                if (deliveryDate.DayOfWeek.ToString().ToUpper() == day.ToUpper())
                {
                    break;
                }
                deliveryDate = deliveryDate.AddDays(1);
            }

            //Debug.WriteLine("DELIVERY DATE: " + deliveryDate);
            return deliveryDate;
        }


        void Open_Checkout(Object sender, EventArgs e)
        {

            Application.Current.MainPage = new CheckoutPage(null);
        }

        void Open_Farm(Object sender, EventArgs e)
        {
            var sl = (StackLayout)sender;
            var tgr = (TapGestureRecognizer)sl.GestureRecognizers[0];
            var dm = (ScheduleInfo)tgr.CommandParameter;
            string weekday = dm.delivery_dayofweek;

            Debug.WriteLine(weekday);
            foreach(string b_uid in dm.business_uids)
            {
                Debug.WriteLine(b_uid);
            }

            if (types.Count == 0)
            {
                types.Add("fruit");
                types.Add("vegetable");
                types.Add("dessert");
                types.Add("other");
            }

            Application.Current.Properties["delivery_date"] = dm.delivery_date;
            Application.Current.Properties["delivery_time"] = dm.delivery_time;
            Application.Current.Properties["deliveryDate"] = dm.deliveryTimeStamp;

            ItemsPage businessItemPage = new ItemsPage(types, dm.business_uids, weekday);
            Application.Current.MainPage = businessItemPage;
        }



        void Change_Border_Color(Object sender, EventArgs e)
        {
            
            var f = (Frame)sender;
            var tgr = (TapGestureRecognizer)f.GestureRecognizers[0];
            var bc = (BusinessCard)tgr.CommandParameter;
            if (bc.border_color == Color.LightGray)
            {
                business.Clear();
                business.Add(selectedBusiness(new Business()
                {
                    business_image = bc.business_image,
                    business_name = bc.business_name,
                    z_biz_id = bc.business_uid
                }));
                var businessDeliveryDates = GetBusinessSchedule(data, bc.business_uid);
                List<ScheduleInfo> businessDisplaySchedule = new List<ScheduleInfo>();

                foreach(DateTime deliveryElement in businessDeliveryDates)
                {
                    foreach(ScheduleInfo scheduleElement in displaySchedule)
                    {
                        if(deliveryElement == scheduleElement.deliveryTimeStamp)
                        {
                            businessDisplaySchedule.Add(scheduleElement);
                            break;
                        }
                    }
                }

                //Debug.WriteLine("");
                //Debug.WriteLine("");
                //Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS SORTED");
                //Debug.WriteLine("");

                //foreach (ScheduleInfo element in businessDisplaySchedule)
                //{
                //    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                //    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                //    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                //    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                //    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                //    Debug.Write("business_uids list: ");

                //    foreach (string ID in element.business_uids)
                //    {
                //        Debug.Write(ID + ", ");
                //    }

                //    Debug.WriteLine("");
                //    Debug.WriteLine("");
                //}

                farm_list.ItemsSource = business;
                delivery_list.ItemsSource = businessDisplaySchedule;
            }
            else
            {
                farm_list.ItemsSource = businesses;
                delivery_list.ItemsSource = displaySchedule;
            }

        }

        void CheckOutClickDeliveryDaysPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage(null);
        }

        void DeliveryDaysClick(System.Object sender, System.EventArgs e)
        {
            // SHOULDN'T MOVE SINCE YOU ARE IN THIS PAGE
        }

        void OrdersClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage();
        }

        void InfoClick(System.Object sender, System.EventArgs e)
        {
            if (!(bool)Application.Current.Properties["guest"])
            {
                Application.Current.MainPage = new InfoPage();
            }
            
        }

        void ProfileClick(System.Object sender, System.EventArgs e)
        {
            if (!(bool)Application.Current.Properties["guest"])
            {
                Application.Current.MainPage = new ProfilePage();
            }
        }
    }
}
