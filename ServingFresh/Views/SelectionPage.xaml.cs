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
using System.IO;
using System.ComponentModel;
using static ServingFresh.Views.SignUpPage;
namespace ServingFresh.Views
{
    public partial class SelectionPage : ContentPage
    {
        class BusinessCard
        {
            public string business_image { get; set; }
            public string business_name { get; set; }
            public string business_uid { get; set; }
            public IDictionary<string,IList<string>> list_delivery_days { get; set; }
            public Color border_color { get; set; }
        }

        BusinessCard unselectedBusiness(Business b)
        {
            return new BusinessCard()
            {
                business_image = b.business_image,
                business_name = b.business_name,
                business_uid = b.z_biz_id,
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
                business_uid = b.z_biz_id,
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

        public static ScheduleInfo selectedDeliveryDate = null;

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

        public class FavoriteResult
        {
            public string favorite_produce { get; set; }
        }

        public class FavoriteGet
        {
            public string message { get; set; }
            public int code { get; set; }
            public IList<FavoriteResult> result { get; set; }
            public string sql { get; set; }
        }

        public class FavoriteItemByUser
        {
            public string customer_uid { get; set; }
        }

        public class ScheduleInfo : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            public string delivery_date { get; set; }
            public string delivery_shortname { get; set; }
            public string delivery_dayofweek { get; set; }
            public string delivery_time { get; set; }
            public List<string> business_uids { get; set; }
            public DateTime deliveryTimeStamp { get; set; }
            public string orderExpTime { get; set; }
            public Xamarin.Forms.Color colorSchedule { get; set; }
            public Xamarin.Forms.Color textColor { get; set; }

            public Xamarin.Forms.Color colorScheduleUpdate {
                 set
                {
                    colorSchedule = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("colorSchedule"));
                }
            }

            public Xamarin.Forms.Color textColorUpdate
            {
                set
                {
                    textColor = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("textColor"));
                }
            }
        }

        public class Filter : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            public string filterName { get; set; }
            public string iconSource { get; set; }
            public bool isSelected { get; set; }
            public string type { get; set; }

            public Filter(string filterName, string iconSource, string type)
            {
                this.filterName = filterName;
                this.iconSource = iconSource;
                this.type = type;
                isSelected = false;
            }

            public string updateImage
            {
                set
                {
                    iconSource = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("iconSource"));
                }
            }

        }

        GridLength columnWidth;
        ObservableCollection<Filter> filters;
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
        List<Items> data2 = new List<Items>();
        public ObservableCollection<ItemsModel> datagrid = new ObservableCollection<ItemsModel>();

        public static readonly Dictionary<string, ItemPurchased> order = new Dictionary<string, ItemPurchased>();
        public static string zone = "";
        public int totalCount = 0;

        
        // New variables for single lists
        ObservableCollection<SingleItem> vegetablesList = new ObservableCollection<SingleItem>();
        List<Items> vegetableDoubleList = new List<Items>();
        List<Items> fruitDoubleList = new List<Items>();
        List<Items> otherDoubleList = new List<Items>();
        List<Items> dessetDoubleList = new List<Items>();

        public ObservableCollection<ItemsModel> fruitGrid = new ObservableCollection<ItemsModel>();
        public ObservableCollection<ItemsModel> otherGrid = new ObservableCollection<ItemsModel>();
        public ObservableCollection<ItemsModel> dessertGrid = new ObservableCollection<ItemsModel>();

        ObservableCollection<SingleItem> fruitsList = new ObservableCollection<SingleItem>();
        ObservableCollection<SingleItem> othersList = new ObservableCollection<SingleItem>();
        ObservableCollection<SingleItem> dessertsList = new ObservableCollection<SingleItem>();
        public static List<string> favorites = new List<string>();

        public SelectionPage(string accessToken = "", string refreshToken = "", AuthenticatorCompletedEventArgs googleCredentials = null, AppleAccount appleCredentials = null, string platform = "")
        {
            InitializeComponent();
            _ = VerifyUserCredentials(accessToken, refreshToken, googleCredentials, appleCredentials, platform);
            _ = SetFavoritesList();
        }

        public SelectionPage()
        {
            InitializeComponent();
            _ = CheckVersion();
            //_ = SetFavoritesList();
        }

        public SelectionPage(Location current, Dictionary<string, ItemPurchased> previousOrder = null)
        {
            InitializeComponent();


            //Application.Current.Properties["user_latitude"] = current.Latitude.ToString();
            //Application.Current.Properties["user_longitude"] = current.Longitude.ToString();

            GetPreviousOrder(previousOrder);
            GetBusinesses();
            _ = SetFavoritesList();
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
                                        Application.Current.Properties["guest"] = false;
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
                GetBusinesses();

                CartTotal.Text = CheckoutPage.total_qty.ToString();
            }
        }

        void GetPreviousOrder(Dictionary<string, ItemPurchased> previousOrder = null)
        {
            //if(previousOrder != null)
            //{
            //    order = previousOrder;
            //}
        }

        public async void GetBusinesses()
        {
            var userLat = user.getUserLatitude();
            var userLong = user.getUserLongitude();

            Debug.WriteLine("INPUT 1: " + userLat);
            Debug.WriteLine("INPUT 2: " + userLong);

            //if (userLat == "0" && userLong == "0"){ userLong = "-121.8866517"; userLat = "37.2270928";}

            var client = new HttpClient();
            var response = await client.GetAsync(Constant.ProduceByLocation + userLong + "," + userLat);
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("CALL TO ENDPOINT SUCCESSFULL?: " + response.IsSuccessStatusCode);
            Debug.WriteLine("JSON RETURNED: " + result);

            if (response.IsSuccessStatusCode)
            {
                data = JsonConvert.DeserializeObject<ServingFreshBusiness>(result);

                var currentDate = DateTime.Now;
                var tempDateTable = GetTable(currentDate);

                foreach (Business a in data.business_details)
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
                            element.orderExpTime = "Order by " + acceptingDate.ToString("dddd") + ", " + acceptingDate.ToString("htt").ToLower();

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
                            foreach (ScheduleInfo element in displaySchedule)
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

                deliverySchedule.Sort();
                List<ScheduleInfo> sortedSchedule = new List<ScheduleInfo>();

                foreach (DateTime deliveryElement in deliverySchedule)
                {
                    foreach (ScheduleInfo scheduleElement in displaySchedule)
                    {
                        if (deliveryElement == scheduleElement.deliveryTimeStamp)
                        {
                            sortedSchedule.Add(scheduleElement);
                        }
                    }
                }

                displaySchedule = sortedSchedule;

                if (data.business_details.Count != 0)
                {
                    // Parse it
                    zone = data.business_details[0].zone;

                    deliveryDays.Clear();
                    businessList.Clear();

                    foreach (Business business in data.business_details)
                    {
                        DeliveryInfo element = new DeliveryInfo();
                        element.delivery_day = business.z_delivery_day;
                        element.delivery_time = business.z_delivery_time;

                        bool addElement = false;

                        if (deliveryDays.Count != 0)
                        {
                            foreach (DeliveryInfo i in deliveryDays)
                            {
                                if (element.delivery_time == i.delivery_time && element.delivery_day == i.delivery_day)
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
                        IDictionary<string, IList<string>> days = new Dictionary<string, IList<string>>();
                        foreach (Business b in data.business_details)
                        {
                            if (id == b.z_biz_id)
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

                        foreach (Business i in data.business_details)
                        {
                            if (id == i.z_biz_id)
                            {
                                i.delivery_days = days;
                                businesses.Add(unselectedBusiness(i));
                                break;
                            }
                        }
                    }

                    foreach(ScheduleInfo i in displaySchedule)
                    {
                        i.colorSchedule = Color.FromHex("#E0E6E6");
                        i.textColor = Color.FromHex("#136D74");
                    }

                    delivery_list.ItemsSource = displaySchedule;

                    if (displaySchedule.Count != 0)
                    {
                        selectedDeliveryDate = displaySchedule[0];
                        displaySchedule[0].colorScheduleUpdate = Color.FromHex("#FF8500");
                        displaySchedule[0].textColorUpdate = Color.FromHex("#FFFFFF");

                        var day = DateTime.Parse(displaySchedule[0].deliveryTimeStamp.ToString());

                        title.Text = day.ToString("ddd") + ", " + displaySchedule[0].delivery_date.ToString();
                        deliveryTime.Text = displaySchedule[0].delivery_time;
                        orderBy.Text = "(" +  displaySchedule[0].orderExpTime + ")";

                        //GetData(data.result);

                        GetDataForSingleList(data.result);
                        //GetDataForVegetables(vegetableDoubleList);
                        //GetDataForFruits(fruitDoubleList);
                        //GetDataForOthers(otherDoubleList);
                        //GetDataForDesserts(dessetDoubleList);
                        //updateItemsBackgroundColorAndQuantity(vegetablesList, selectedDeliveryDate);


                        updateItemsBackgroundColorAndQuantity(vegetablesList, selectedDeliveryDate);
                        updateItemsBackgroundColorAndQuantity(fruitsList, selectedDeliveryDate);
                        updateItemsBackgroundColorAndQuantity(dessertsList, selectedDeliveryDate);
                        updateItemsBackgroundColorAndQuantity(othersList, selectedDeliveryDate);

                        SetVerticalView(vegetablesList, itemList);
                        //updateItemsBackgroundColorAndQuantity(fruitsList, selectedDeliveryDate);
                        SetVerticalView(fruitsList, fruitsVerticalView);
                        //updateItemsBackgroundColorAndQuantity(dessertsList, selectedDeliveryDate);
                        SetVerticalView(dessertsList, dessertsVerticalView);
                        //updateItemsBackgroundColorAndQuantity(othersList, selectedDeliveryDate);
                        SetVerticalView(othersList, othersVerticalView);
                        UpdateNumberOfItemsInCart();
                    }
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


        private void SetVerticalView(IList<SingleItem> listOfItems, ListView verticalList)
        {
            try
            {
                var gridViewLayout = new ObservableCollection<ItemsModel>();
                if (listOfItems.Count != 0 && listOfItems != null)
                {
                    //var gridViewLayout = new ObservableCollection<ItemsModel>();
                    int n = listOfItems.Count;
                    int j = 0;
                    if (n == 0)
                    {

                        gridViewLayout.Add(new ItemsModel()
                        {
                            height = this.Width / 2 - 10,
                            width = this.Width / 2 - 25,
                            imageSourceLeft = "",
                            quantityLeft = 0,
                            itemNameLeft = "",
                            itemPriceLeft = "$ " + "",
                            itemPriceLeftUnit = "",
                            itemLeftUnit = "",
                            item_businessPriceLeft = 0,
                            isItemLeftVisiable = false,
                            isItemLeftEnable = false,
                            quantityL = 0,
                            item_descLeft = "",
                            itemTaxableLeft = "",
                            colorLeft = Color.FromHex("#FFFFFF"),
                            itemTypeLeft = "",
                            favoriteIconLeft = "unselectedHeartIcon.png",
                            opacityLeft = 0,
                            isItemLeftUnavailable = false,

                            imageSourceRight = "",
                            quantityRight = 0,
                            itemNameRight = "",
                            itemPriceRight = "$ " + "",
                            itemPriceRightUnit = "",
                            itemRightUnit = "",
                            item_businessPriceRight = 0,
                            isItemRightVisiable = false,
                            isItemRightEnable = false,
                            quantityR = 0,
                            item_descRight = "",
                            itemTaxableRight = "",
                            colorRight = Color.FromHex("#FFFFFF"),
                            itemTypeRight = "",
                            favoriteIconRight = "unselectedHeartIcon.png",
                            opacityRight = 0,
                            isItemRightUnavailable = false,
                        });
                    }
                    if (isAmountItemsEven(n))
                    {
                        for (int i = 0; i < n / 2; i++)
                        {
                            gridViewLayout.Add(new ItemsModel()
                            {
                                height = this.Width / 2 - 10,
                                width = this.Width / 2 - 25,
                                imageSourceLeft = listOfItems[j].itemImage,
                                item_uidLeft = listOfItems[j].itemUID,
                                itm_business_uidLeft = listOfItems[j].itemBusinessUID,
                                quantityLeft = listOfItems[j].itemQuantity,
                                itemNameLeft = listOfItems[j].itemName,
                                itemPriceLeft = listOfItems[j].itemPrice,
                                itemPriceLeftUnit = listOfItems[j].itemPriceWithUnit,
                                itemLeftUnit = listOfItems[j].itemUnit,
                                item_businessPriceLeft = listOfItems[j].itemBusinessPrice,
                                isItemLeftVisiable = true,
                                isItemLeftEnable = true,
                                quantityL = listOfItems[j].itemQuantity,
                                item_descLeft = listOfItems[j].itemDescription,
                                itemTaxableLeft = listOfItems[j].itemTaxable,
                                colorLeft = listOfItems[j].itemBackgroundColor,
                                itemTypeLeft = listOfItems[j].itemType,
                                favoriteIconLeft = listOfItems[j].itemFavoriteImage,
                                opacityLeft = listOfItems[j].itemOpacity,
                                isItemLeftUnavailable = listOfItems[j].isItemUnavailable,

                                imageSourceRight = listOfItems[j + 1].itemImage,
                                item_uidRight = listOfItems[j + 1].itemUID,
                                itm_business_uidRight = listOfItems[j + 1].itemBusinessUID,
                                quantityRight = listOfItems[j + 1].itemQuantity,
                                itemNameRight = listOfItems[j + 1].itemName,
                                itemPriceRight = listOfItems[j + 1].itemPrice,
                                itemPriceRightUnit = listOfItems[j + 1].itemPriceWithUnit,
                                itemRightUnit = listOfItems[j + 1].itemUnit,
                                item_businessPriceRight = listOfItems[j + 1].itemBusinessPrice,
                                isItemRightVisiable = true,
                                isItemRightEnable = true,
                                quantityR = listOfItems[j + 1].itemQuantity,
                                item_descRight = listOfItems[j + 1].itemDescription,
                                itemTaxableRight = listOfItems[j + 1].itemTaxable,
                                colorRight = listOfItems[j + 1].itemBackgroundColor,
                                itemTypeRight = listOfItems[j + 1].itemType,
                                favoriteIconRight = listOfItems[j + 1].itemFavoriteImage,
                                opacityRight = listOfItems[j + 1].itemOpacity,
                                isItemRightUnavailable = listOfItems[j + 1].isItemUnavailable,
                            }); ;
                            j = j + 2;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < n / 2; i++)
                        {
                            gridViewLayout.Add(new ItemsModel()
                            {
                                height = this.Width / 2 - 10,
                                width = this.Width / 2 - 25,
                                imageSourceLeft = listOfItems[j].itemImage,
                                item_uidLeft = listOfItems[j].itemUID,
                                itm_business_uidLeft = listOfItems[j].itemBusinessUID,
                                quantityLeft = listOfItems[j].itemQuantity,
                                itemNameLeft = listOfItems[j].itemName,
                                itemPriceLeft = listOfItems[j].itemPrice,
                                itemPriceLeftUnit = listOfItems[j].itemPriceWithUnit,
                                itemLeftUnit = listOfItems[j].itemUnit,
                                item_businessPriceLeft = listOfItems[j].itemBusinessPrice,
                                isItemLeftVisiable = true,
                                isItemLeftEnable = true,
                                quantityL = listOfItems[j].itemQuantity,
                                item_descLeft = listOfItems[j].itemDescription,
                                itemTaxableLeft = listOfItems[j].itemTaxable,
                                colorLeft = listOfItems[j].itemBackgroundColor,
                                itemTypeLeft = listOfItems[j].itemType,
                                favoriteIconLeft = listOfItems[j].itemFavoriteImage,
                                opacityLeft = listOfItems[j].itemOpacity,
                                isItemLeftUnavailable = listOfItems[j].isItemUnavailable,

                                imageSourceRight = listOfItems[j + 1].itemImage,
                                item_uidRight = listOfItems[j + 1].itemUID,
                                itm_business_uidRight = listOfItems[j + 1].itemBusinessUID,
                                quantityRight = listOfItems[j + 1].itemQuantity,
                                itemNameRight = listOfItems[j + 1].itemName,
                                itemPriceRight = listOfItems[j + 1].itemPrice,
                                itemPriceRightUnit = listOfItems[j + 1].itemPriceWithUnit,
                                itemRightUnit = listOfItems[j + 1].itemUnit,
                                item_businessPriceRight = listOfItems[j + 1].itemBusinessPrice,
                                isItemRightVisiable = true,
                                isItemRightEnable = true,
                                quantityR = listOfItems[j + 1].itemQuantity,
                                item_descRight = listOfItems[j + 1].itemDescription,
                                itemTaxableRight = listOfItems[j + 1].itemTaxable,
                                colorRight = listOfItems[j + 1].itemBackgroundColor,
                                itemTypeRight = listOfItems[j + 1].itemType,
                                favoriteIconRight = listOfItems[j + 1].itemFavoriteImage,
                                opacityRight = listOfItems[j + 1].itemOpacity,
                                isItemRightUnavailable = listOfItems[j + 1].isItemUnavailable,
                            });
                            j = j + 2;
                        }
                        gridViewLayout.Add(new ItemsModel()
                        {
                            height = this.Width / 2 - 10,
                            width = this.Width / 2 - 25,
                            imageSourceLeft = listOfItems[j].itemImage,
                            item_uidLeft = listOfItems[j].itemUID,
                            itm_business_uidLeft = listOfItems[j].itemBusinessUID,
                            quantityLeft = listOfItems[j].itemQuantity,
                            itemNameLeft = listOfItems[j].itemName,
                            itemPriceLeft = listOfItems[j].itemPrice,
                            itemPriceLeftUnit = listOfItems[j].itemPriceWithUnit,
                            itemLeftUnit = listOfItems[j].itemUnit,
                            item_businessPriceLeft = listOfItems[j].itemBusinessPrice,
                            isItemLeftVisiable = true,
                            isItemLeftEnable = true,
                            quantityL = listOfItems[j].itemQuantity,
                            item_descLeft = listOfItems[j].itemDescription,
                            itemTaxableLeft = listOfItems[j].itemTaxable,
                            colorLeft = listOfItems[j].itemBackgroundColor,
                            itemTypeLeft = listOfItems[j].itemType,
                            favoriteIconLeft = listOfItems[j].itemFavoriteImage,
                            opacityLeft = listOfItems[j].itemOpacity,
                            isItemLeftUnavailable = listOfItems[j].isItemUnavailable,

                            imageSourceRight = "",
                            quantityRight = 0,
                            itemNameRight = "",
                            itemPriceRight = "$ " + "",
                            isItemRightVisiable = false,
                            isItemRightEnable = false,
                            quantityR = 0,
                            colorRight = Color.FromHex("#FFFFFF"),
                            itemTypeRight = "",
                            favoriteIconRight = "unselectedHeartIcon.png",
                            opacityRight = 0,
                            isItemRightUnavailable = false,
                        });
                    }
                }

                verticalList.ItemsSource = gridViewLayout;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void GetDataForSingleList(IList<Items> listOfItems)
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

                    vegetablesList.Clear();
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
                            itemPrice = "$ " + produce.item_price.ToString(),
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

                        if(produce.item_type == "vegetable")
                        {
                            vegetablesList.Add(itemToInsert);
                            vegetableDoubleList.Add(produce);
                        }
                        else if (produce.item_type == "fruit")
                        {
                            fruitsList.Add(itemToInsert);
                            fruitDoubleList.Add(produce);
                        }
                        else if (produce.item_type == "dessert")
                        {
                            dessertsList.Add(itemToInsert);
                            dessetDoubleList.Add(produce);
                        }
                        else
                        {
                            othersList.Add(itemToInsert);
                            otherDoubleList.Add(produce);
                        }
                    }
                }

                vegetablesListView.ItemsSource = vegetablesList;
                fruitsListView.ItemsSource = fruitsList;
                othersListView.ItemsSource = othersList;
                dessertsListView.ItemsSource = dessertsList;
                //foreach (string key in order.Keys)
                //{

                //    foreach (ItemsModel a in datagrid)
                //    {
                //        if (order[key].item_name == a.itemNameLeft)
                //        {
                //            Debug.WriteLine(order[key].item_name);
                //            a.quantityLeft = order[key].item_quantity;
                //            break;
                //        }
                //        else if (order[key].item_name == a.itemNameRight)
                //        {
                //            Debug.WriteLine(order[key].item_name);
                //            a.quantityRight = order[key].item_quantity;
                //            break;
                //        }
                //    }
                //}
                //itemList.ItemsSource = datagrid;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public bool isAmountItemsEven(int num)
        {
            bool result = false;
            if (num % 2 == 0) { result = true; }
            return result;
        }

        void SubtractItem(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;
            var itemModelObject = (SingleItem)button.CommandParameter;
            var itemSelected = new ItemPurchased();

            if (itemModelObject != null)
            {
                if (itemModelObject.updateItemQuantity != 0)
                {
                    itemModelObject.updateItemQuantity -= 1;

                    if (order != null)
                    {
                        if (order.ContainsKey(itemModelObject.itemName))
                        {
                            var itemToUpdate = order[itemModelObject.itemName];
                            itemToUpdate.item_quantity = itemModelObject.updateItemQuantity;
                            order[itemModelObject.itemName] = itemToUpdate;
                        }
                        else
                        {
                            itemSelected.pur_business_uid = itemModelObject.itemBusinessUID;
                            itemSelected.item_uid = itemModelObject.itemUID;
                            itemSelected.item_name = itemModelObject.itemName;
                            itemSelected.item_quantity = itemModelObject.updateItemQuantity;
                            itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPrice.Substring(1).Trim());
                            itemSelected.img = itemModelObject.itemImage;
                            itemSelected.unit = itemModelObject.itemUnit;
                            itemSelected.description = itemModelObject.itemDescription;
                            itemSelected.business_price = itemModelObject.itemBusinessPrice;
                            itemSelected.taxable = itemModelObject.itemTaxable;
                            itemSelected.isItemAvailable = true;
                            order.Add(itemModelObject.itemName, itemSelected);
                        }
                    }

                    if (itemModelObject.updateItemQuantity == 0)
                    {
                        itemModelObject.updateItemBackgroundColor = Color.FromHex("#FFFFFF");
                        order.Remove(itemModelObject.itemName);
                    }
                    UpdateNumberOfItemsInCart();
                }
                else
                {
                    itemModelObject.updateItemBackgroundColor = Color.FromHex("#FFFFFF");
                }
            }

        }

        void AddItem(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;
            var itemModelObject = (SingleItem)button.CommandParameter;
            var itemSelected = new ItemPurchased();
            if (itemModelObject != null)
            {
                itemModelObject.updateItemBackgroundColor = Color.FromHex("#ffce99");
                itemModelObject.updateItemQuantity += 1;

                if (order != null)
                {
                    if (order.ContainsKey(itemModelObject.itemName))
                    {
                        var itemToUpdate = order[itemModelObject.itemName];
                        itemToUpdate.item_quantity = itemModelObject.updateItemQuantity;
                        order[itemModelObject.itemName] = itemToUpdate;
                    }
                    else
                    {
                        itemSelected.pur_business_uid = itemModelObject.itemBusinessUID;
                        itemSelected.item_uid = itemModelObject.itemUID;
                        itemSelected.item_name = itemModelObject.itemName;
                        itemSelected.item_quantity = itemModelObject.updateItemQuantity;
                        itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPrice.Substring(1).Trim());
                        itemSelected.img = itemModelObject.itemImage;
                        itemSelected.unit = itemModelObject.itemUnit;
                        itemSelected.description = itemModelObject.itemDescription;
                        itemSelected.business_price = itemModelObject.itemBusinessPrice;
                        itemSelected.taxable = itemModelObject.itemTaxable;
                        itemSelected.isItemAvailable = true;
                        order.Add(itemModelObject.itemName, itemSelected);
                    }
                }
                UpdateNumberOfItemsInCart();
            }
        }

        void updateItemsBackgroundColorAndQuantity(ObservableCollection<SingleItem> sourceList, ScheduleInfo defaultSchedule)
        {
            foreach (SingleItem i in sourceList)
            {
                if (!defaultSchedule.business_uids.Contains(i.itemBusinessUID))
                {
                    i.updateItemOpacity = 0.5;
                    i.updateIsItemEnable = false;
                    i.updateIsItemUnavailable = true;
                }
                else
                {
                    i.updateItemOpacity = 1;
                    i.updateIsItemEnable = true;
                    i.updateIsItemUnavailable = false;
                }

                if (order != null)
                {
                    if (order.ContainsKey(i.itemName))
                    {
                        i.updateItemBackgroundColor = Color.FromHex("#ffce99");
                        i.updateItemQuantity = order[i.itemName].item_quantity;
                    }
                }

                if(favorites != null && favorites.Count != 0)
                {
                    if (favorites.Contains(i.itemName))
                    {
                        i.updateItemFavoriteImage = "selectedFavoritesIcon.png";
                    }
                }
            }
        }

        void SubtractItemLeft(System.Object sender, System.EventArgs e)
        {

            var button = (Button)sender;
            var itemModelObject = (ItemsModel)button.CommandParameter;
            ItemPurchased itemSelected = new ItemPurchased();
            if (itemModelObject != null)
            {
                if (itemModelObject.quantityL != 0)
                {
                    itemModelObject.quantityL -= 1;
                    totalCount -= 1;
                    //CartTotal.Text = totalCount.ToString();
                    
                    if (order != null)
                    {
                        if (order.ContainsKey(itemModelObject.itemNameLeft))
                        {
                            var itemToUpdate = order[itemModelObject.itemNameLeft];
                            itemToUpdate.item_quantity = itemModelObject.quantityL;
                            order[itemModelObject.itemNameLeft] = itemToUpdate;
                        }
                        else
                        {
                            itemSelected.pur_business_uid = itemModelObject.itm_business_uidLeft;
                            itemSelected.item_uid = itemModelObject.item_uidLeft;
                            itemSelected.item_name = itemModelObject.itemNameLeft;
                            itemSelected.item_quantity = itemModelObject.quantityL;
                            itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPriceLeft.Substring(1).Trim());
                            itemSelected.img = itemModelObject.imageSourceLeft;
                            itemSelected.unit = itemModelObject.itemLeftUnit;
                            itemSelected.description = itemModelObject.item_descLeft;
                            itemSelected.business_price = itemModelObject.item_businessPriceLeft;
                            itemSelected.taxable = itemModelObject.itemTaxableLeft;
                            itemSelected.isItemAvailable = true;
                            order.Add(itemModelObject.itemNameLeft, itemSelected);
                        }
                    }

                    if(itemModelObject.quantityL == 0)
                    {
                        itemModelObject.colorLeft = Color.FromHex("#FFFFFF");
                        itemModelObject.colorLeftUpdate = Color.FromHex("#FFFFFF");
                        order.Remove(itemModelObject.itemNameLeft);
                    }
                    UpdateNumberOfItemsInCart();
                }
                else
                {
                    itemModelObject.colorLeft = Color.FromHex("#FFFFFF");
                    itemModelObject.colorLeftUpdate = Color.FromHex("#FFFFFF");
                }
            }

        }

        void AddItemLeft(System.Object sender, System.EventArgs e)
        {

            var button = (Button)sender;
            var itemModelObject = (ItemsModel)button.CommandParameter;
            ItemPurchased itemSelected = new ItemPurchased();
            if (itemModelObject != null)
            {
                itemModelObject.colorLeft = Color.FromHex("#ffce99");
                itemModelObject.colorLeftUpdate = Color.FromHex("#ffce99");
                itemModelObject.quantityL += 1;
                totalCount += 1;
                //CartTotal.Text = totalCount.ToString();
                
                if (order != null)
                {
                    if (order.ContainsKey(itemModelObject.itemNameLeft))
                    {
                        var itemToUpdate = order[itemModelObject.itemNameLeft];
                        itemToUpdate.item_quantity = itemModelObject.quantityL;
                        order[itemModelObject.itemNameLeft] = itemToUpdate;
                    }
                    else
                    {
                        itemSelected.pur_business_uid = itemModelObject.itm_business_uidLeft;
                        itemSelected.item_uid = itemModelObject.item_uidLeft;
                        itemSelected.item_name = itemModelObject.itemNameLeft;
                        itemSelected.item_quantity = itemModelObject.quantityL;
                        itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPriceLeft.Substring(1).Trim());
                        itemSelected.img = itemModelObject.imageSourceLeft;
                        itemSelected.unit = itemModelObject.itemLeftUnit;
                        itemSelected.description = itemModelObject.item_descLeft;
                        itemSelected.business_price = itemModelObject.item_businessPriceLeft;
                        itemSelected.taxable = itemModelObject.itemTaxableLeft;
                        itemSelected.isItemAvailable = true;
                        order.Add(itemModelObject.itemNameLeft, itemSelected);
                    }
                }
                UpdateNumberOfItemsInCart();
            }

        }

        void SubtractItemRight(System.Object sender, System.EventArgs e)
        {

            var button = (Button)sender;
            var itemModelObject = (ItemsModel)button.CommandParameter;
            ItemPurchased itemSelected = new ItemPurchased();
            if (itemModelObject != null)
            {
                if (itemModelObject.quantityR != 0)
                {
                    itemModelObject.quantityR -= 1;
                    totalCount -= 1;
                    //CartTotal.Text = totalCount.ToString();
                    
                    if (order.ContainsKey(itemModelObject.itemNameRight))
                    {
                        var itemToUpdate = order[itemModelObject.itemNameRight];
                        itemToUpdate.item_quantity = itemModelObject.quantityR;
                        order[itemModelObject.itemNameRight] = itemToUpdate;
                    }
                    else
                    {
                        itemSelected.pur_business_uid = itemModelObject.itm_business_uidRight;
                        itemSelected.item_uid = itemModelObject.item_uidRight;
                        itemSelected.item_name = itemModelObject.itemNameRight;
                        itemSelected.item_quantity = itemModelObject.quantityR;
                        itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPriceRight.Substring(1).Trim());
                        itemSelected.img = itemModelObject.imageSourceRight;
                        itemSelected.unit = itemModelObject.itemRightUnit;
                        itemSelected.description = itemModelObject.item_descRight;
                        itemSelected.business_price = itemModelObject.item_businessPriceRight;
                        itemSelected.taxable = itemModelObject.itemTaxableRight;
                        itemSelected.isItemAvailable = true;
                        order.Add(itemModelObject.itemNameRight, itemSelected);
                    }

                    if(itemModelObject.quantityR == 0)
                    {
                        itemModelObject.colorRight = Color.FromHex("#FFFFFF");
                        itemModelObject.colorRightUpdate = Color.FromHex("#FFFFFF");
                        order.Remove(itemModelObject.itemNameRight);
                    }
                    UpdateNumberOfItemsInCart();
                }
                else
                {
                    itemModelObject.colorRight = Color.FromHex("#FFFFFF");
                    itemModelObject.colorRightUpdate = Color.FromHex("#FFFFFF");
                }
            }

        }

        void AddItemRight(System.Object sender, System.EventArgs e)
        {

            var button = (Button)sender;
            var itemModelObject = (ItemsModel)button.CommandParameter;
            ItemPurchased itemSelected = new ItemPurchased();
            if (itemModelObject != null)
            {
                itemModelObject.colorRight = Color.FromHex("#ffce99");
                itemModelObject.colorRightUpdate = Color.FromHex("#ffce99");
                itemModelObject.quantityR += 1;
                totalCount += 1;
                //CartTotal.Text = totalCount.ToString();
                
                if (order.ContainsKey(itemModelObject.itemNameRight))
                {
                    var itemToUpdate = order[itemModelObject.itemNameRight];
                    itemToUpdate.item_quantity = itemModelObject.quantityR;
                    order[itemModelObject.itemNameRight] = itemToUpdate;
                }
                else
                {
                    itemSelected.pur_business_uid = itemModelObject.itm_business_uidRight;
                    itemSelected.item_uid = itemModelObject.item_uidRight;
                    itemSelected.item_name = itemModelObject.itemNameRight;
                    itemSelected.item_quantity = itemModelObject.quantityR;
                    itemSelected.item_price = Convert.ToDouble(itemModelObject.itemPriceRight.Substring(1).Trim());
                    itemSelected.img = itemModelObject.imageSourceRight;
                    itemSelected.unit = itemModelObject.itemRightUnit;
                    itemSelected.description = itemModelObject.item_descRight;
                    itemSelected.business_price = itemModelObject.item_businessPriceRight;
                    itemSelected.taxable = itemModelObject.itemTaxableRight;
                    itemSelected.isItemAvailable = true;
                    order.Add(itemModelObject.itemNameRight, itemSelected);
                }
                UpdateNumberOfItemsInCart();
            }
        }

        private Dictionary<string, ItemPurchased> purchase = new Dictionary<string, ItemPurchased>();

        void CheckOutClickBusinessPage(System.Object sender, System.EventArgs e)
        {

            foreach (string item in order.Keys)
            {
                if (!selectedDeliveryDate.business_uids.Contains(order[item].pur_business_uid))
                {
                    order[item].isItemAvailable = false;
                }
                else
                {
                    order[item].isItemAvailable = true;
                }
            }

            Application.Current.MainPage = new CheckoutPage();
        }

        void UpdateNumberOfItemsInCart()
        {
            var totalItemsToDelivery = 0;
            foreach (string item in order.Keys)
            {
                if (order[item].item_quantity != 0)
                {
                    if (selectedDeliveryDate.business_uids.Contains(order[item].pur_business_uid))
                    {
                        totalItemsToDelivery += order[item].item_quantity;
                    }
                }
            }
            CartTotal.Text = totalItemsToDelivery.ToString();
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

            foreach (Business a in data.business_details)
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

        void Open_Farm(Object sender, EventArgs e)
        {
            var sl = (StackLayout)sender;
            var tgr = (TapGestureRecognizer)sl.GestureRecognizers[0];
            var dm = (ScheduleInfo)tgr.CommandParameter;
            string weekday = dm.delivery_dayofweek;

            selectedDeliveryDate = dm;
            var day = DateTime.Parse(selectedDeliveryDate.deliveryTimeStamp.ToString());
            title.Text = day.ToString("ddd") + ", " + selectedDeliveryDate.delivery_date.ToString();
            deliveryTime.Text = selectedDeliveryDate.delivery_time;
            orderBy.Text = "(" + selectedDeliveryDate.orderExpTime + ")";

            Debug.WriteLine(weekday);
            foreach(string b_uid in dm.business_uids)
            {
                Debug.WriteLine(b_uid);
            }

            foreach(ScheduleInfo i in displaySchedule)
            {
                i.colorScheduleUpdate = Color.FromHex("#E0E6E6");
                i.textColorUpdate = Color.FromHex("#136D74");
            }
            dm.colorScheduleUpdate = Color.FromHex("#FF8500");
            dm.textColorUpdate = Color.FromHex("#FFFFFF");

            Application.Current.Properties["delivery_date"] = dm.delivery_date;
            Application.Current.Properties["delivery_time"] = dm.delivery_time;
            Application.Current.Properties["deliveryDate"] = dm.deliveryTimeStamp;

            updateItemsBackgroundColorAndQuantity(vegetablesList, selectedDeliveryDate);
            updateItemsBackgroundColorAndQuantity(fruitsList, selectedDeliveryDate);
            updateItemsBackgroundColorAndQuantity(dessertsList, selectedDeliveryDate);
            updateItemsBackgroundColorAndQuantity(othersList, selectedDeliveryDate);
            SetVerticalView(vegetablesList, itemList);
            SetVerticalView(fruitsList, fruitsVerticalView);
            SetVerticalView(dessertsList, dessertsVerticalView);
            SetVerticalView(othersList, othersVerticalView);
            UpdateNumberOfItemsInCart();
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

                //farm_list.ItemsSource = business;
                delivery_list.ItemsSource = businessDisplaySchedule;
            }
            else
            {
                //farm_list.ItemsSource = businesses;
                delivery_list.ItemsSource = displaySchedule;
            }

        }

        void DeliveryDaysClick(System.Object sender, System.EventArgs e)
        {
            // SHOULDN'T MOVE SINCE YOU ARE IN THIS PAGE
        }

        void OrdersClick(System.Object sender, System.EventArgs e)
        {
            CheckOutClickBusinessPage(sender,e);
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

        void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            var selectedImage = (ImageButton)sender;
            var pickedElement = (ItemsModel)selectedImage.CommandParameter;

            pickedElement.favoriteIconLeftUpdate = "selectedFavoritesIcon.png";
        }

        void FavoriteClick(System.Object sender, System.EventArgs e)
        {
            var selectedImage = (ImageButton)sender;
            var pickedElement = (SingleItem)selectedImage.CommandParameter;
            if (pickedElement.isItemEnable)
            {
                if (pickedElement.updateItemFavoriteImage != "selectedFavoritesIcon.png")
                {
                    pickedElement.updateItemFavoriteImage = "selectedFavoritesIcon.png";
                    AddItemToFavorites(pickedElement.itemName);
                }
                else
                {
                    pickedElement.updateItemFavoriteImage = "unselectedHeartIcon.png";
                    RemoveItemFromFavorites(pickedElement.itemName);
                }
            }
        }

        void AddItemToFavorites(string itemName)
        {
            if (!favorites.Contains(itemName))
            {
                favorites.Add(itemName);
            }
        }

        void RemoveItemFromFavorites(string itemName)
        {
            if (favorites.Contains(itemName))
            {
                favorites.Remove(itemName);
            }
        }

        public static async Task<bool> SetFavoritesList()
        {
            var taskResponse = false;
            if (!(bool)Application.Current.Properties["guest"])
            {
                var client = new System.Net.Http.HttpClient();
                var favoriteItemByUser = new FavoriteItemByUser()
                {
                    customer_uid = (string)Application.Current.Properties["user_id"],
                };

                var serializedObject = JsonConvert.SerializeObject(favoriteItemByUser);
                var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
                var endpointResponse = await client.PostAsync(Constant.GetUserFavorites, content);

                Debug.WriteLine("JSON OBJECT (POST) TO GET USER FAVORITES: " + serializedObject);

                if (endpointResponse.IsSuccessStatusCode)
                {
                    var data = await endpointResponse.Content.ReadAsStringAsync();
                    var parseData = JsonConvert.DeserializeObject<FavoriteGet>(data);

                    if (parseData.result.Count != 0)
                    {
                        favorites = JsonConvert.DeserializeObject<List<string>>(parseData.result[0].favorite_produce);
                        foreach (string itemName in favorites)
                        {
                            Debug.WriteLine("FAVORITE ITEMS " + itemName);
                        }
                    }

                    taskResponse = true;
                }
            }

            return taskResponse;
        }

        public static List<string> GetFavoritesList()
        {
            return favorites;
        }

        public static void RemoveAllFavoriteItems()
        {
            favorites.Clear();
        }

        void GetFavoritesClick(System.Object sender, System.EventArgs e)
        {
            var client = (ImageButton)sender;

            if(client.Source.ToString().Contains("unselectedHeartIcon.png"))
            {
                client.Source = "selectedFavoritesIcon.png";

                vegetablesListView.ItemsSource =  FavoritesFrom(vegetablesList);
                fruitsListView.ItemsSource = FavoritesFrom(fruitsList);
                othersListView.ItemsSource = FavoritesFrom(othersList);
                dessertsListView.ItemsSource = FavoritesFrom(dessertsList);
            }
            else
            {
                client.Source = "unselectedHeartIcon.png";
                vegetablesListView.ItemsSource = vegetablesList;
                fruitsListView.ItemsSource = fruitsList;
                othersListView.ItemsSource = othersList;
                dessertsListView.ItemsSource = dessertsList;
            }
        }

        ObservableCollection<SingleItem> FavoritesFrom(ObservableCollection<SingleItem> source)
        {
            var list = new ObservableCollection<SingleItem>();
            foreach (SingleItem element in source)
            {
                if (favorites.Contains(element.itemName))
                {
                    //var item = element;
                    //item.itemFavoriteImage = "selectedFavoritesIcon.png";
                    list.Add(element);
                }
            }
            return list;
        }

        void ShowHideMenu(System.Object sender, System.EventArgs e)
        {
            var height = new GridLength(0);
            if (menuFrame.Height.Equals(height))
            {
                menuFrame.Height = this.Height - 180;
            }
            else
            {
                menuFrame.Height = 0;
            }
        }

        void SeeAllItemsHorizontallyVertically(System.Object sender, System.EventArgs e)
        {
            var stacklayout = (StackLayout)sender;
            var label = (Label)stacklayout.Children[0];
            var image = (Image)stacklayout.Children[1];

            if(stacklayout.ClassId == "vegetablesView")
            {
                SwitchLayoutViews(vegetablesList, vegetablesListView, itemList, label, image, "vegetables");
            }
            else if (stacklayout.ClassId == "fruitsView")
            {
                SwitchLayoutViews(fruitsList, fruitsListView, fruitsVerticalView, label, image, "fruits");
            }
            else if (stacklayout.ClassId == "othersView")
            {
                SwitchLayoutViews(othersList, othersListView, othersVerticalView, label, image, "others");
            }
            else if (stacklayout.ClassId == "dessertsView")
            {
                SwitchLayoutViews(dessertsList, dessertsListView, dessertsVerticalView, label, image, "desserts");
            }
        }

        void SwitchLayoutViews(ObservableCollection<SingleItem> source ,CollectionView horizontalView, ListView verticalView, Label viewLabel, Image viewIcon, string category)
        {
            if (viewIcon.Source.ToString() == "File: triangleIconFilled.png")
            {
                viewLabel.Text = "Switch to scroll view";
                viewIcon.Rotation = 180;
                viewIcon.Source = "triangleIconEmpty.png";
                SetImageColor(viewIcon);
                SetVerticalView(source, verticalView);
                horizontalView.HeightRequest = 0;
                var rowHeight = source.Count;
                if(rowHeight % 2 != 0) { rowHeight++; }
                verticalView.HeightRequest = 190 * rowHeight / 2;
            }
            else
            {
                viewLabel.Text = "See all " + category;
                viewIcon.Rotation = 0;
                viewIcon.Source = "triangleIconFilled.png";
                SetImageColor(viewIcon);
                updateItemsBackgroundColorAndQuantity(source, selectedDeliveryDate);
                horizontalView.HeightRequest = 190;
                verticalView.HeightRequest = 0;
                
            }
        }

        void SetImageColor(Image image)
        {
            image.Effects[0] = new TintImageEffect() { TintColor = Color.FromHex("#FF8500") };
        }

        public static void NavigateToStore(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SelectionPage();
        }

        public static void NavigateToCart(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage();
        }

        public static void NavigateToHistory(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new HistoryPage();
        }

        public static void NavigateToRefunds(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new RefundPage();
        }

        public static void NavigateToInfo(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new InfoPage();
        }

        public static void NavigateToProfile(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new ProfilePage();
        }

        public static void NavigateToSignIn(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new LogInPage();
        }

        public static void NavigateToSignUp(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SignUpPage();
        }

        public static void NavigateToLogOut(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new PrincipalPage();
        }
    }
}
