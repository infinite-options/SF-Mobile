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
        public List<ScheduleInfo> schedule = new List<ScheduleInfo>();

        List<string> businessList = new List<string>();
        List<BusinessCard> businesses = new List<BusinessCard>();
        List<BusinessCard> business = new List<BusinessCard>();
        public ServingFreshBusiness data = new ServingFreshBusiness();

        public  List<ScheduleInfo> copy = new List<ScheduleInfo>();
        public bool versionUpdate = false;

        List<DateTime> deliverySchedule = new List<DateTime>();
        


        public SelectionPage()
        {
            InitializeComponent();
            Init();
            CheckVersion();
            
        }

        public void Init()
        {
            BackgroundColor = Constants.PrimaryColor;
            //delivery_list.ItemsSource = Deliveries;
            //market_list.ItemsSource = FarmersMarkets;
            //farm_list.ItemsSource = Farms;
        }

        public async void CheckVersion()
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
                Application.Current.Properties["day"] = "";
                Application.Current.Properties["deliveryDate"] = "";
                CartTotal.Text = CheckoutPage.total_qty.ToString();
            }
        }

        public void GetDays()
        {
            deliveryScheduleUnfiltered.Clear();
            deliveryScheduleFiltered.Clear();
            var date = DateTime.Now;
            var monthNames = new List<string>();
            monthNames.Add("");
            monthNames.Add("Jan");
            monthNames.Add("Feb");
            monthNames.Add("Mar");
            monthNames.Add("Apr");
            monthNames.Add("May");
            monthNames.Add("Jun");
            monthNames.Add("Jul");
            monthNames.Add("Aug");
            monthNames.Add("Sep");
            monthNames.Add("Oct");
            monthNames.Add("Nov");
            monthNames.Add("Dec");
            for (int i = 0; i < 7; i++)
            {
                deliveryScheduleUnfiltered.Add(new DeliveriesModel()
                {
                    delivery_dayofweek = date.DayOfWeek.ToString(),
                    delivery_shortname = date.DayOfWeek.ToString().Substring(0, 3).ToUpper(),
                    delivery_date = monthNames[date.Month] + " " + date.Day
                });
                date = date.AddDays(1);
            }

            foreach(DeliveriesModel i in deliveryScheduleUnfiltered)
            {
                foreach(DeliveryInfo j in deliveryDays)
                {
                    if(i.delivery_dayofweek.ToUpper() == j.delivery_day.ToUpper())
                    {
                        deliveryScheduleFiltered.Add(new Delivery() {
                            delivery_dayofweek = i.delivery_dayofweek,
                            delivery_shortname = i.delivery_shortname,
                            delivery_date = i.delivery_date,
                            delivery_time = j.delivery_time
                        });
                    }
                }
            }

            foreach(Delivery k in deliveryScheduleFiltered)
            {
                List<string> ids = new List<string>();
                foreach(Business b in data.result)
                {
                    if(k.delivery_dayofweek.ToUpper() == b.z_delivery_day.ToUpper() && k.delivery_time == b.z_delivery_time)
                    {
                        ids.Add(b.z_biz_id);
                    }
                }
                string deliveryStartTime = "";

                foreach(char a in k.delivery_time.ToCharArray())
                {
                    if(a != '-')
                    {
                        deliveryStartTime += a;
                    }
                    else { break; }
                }
                schedule.Add(new ScheduleInfo()
                {
                    delivery_date = k.delivery_date,
                    delivery_shortname = k.delivery_shortname,
                    delivery_dayofweek = k.delivery_dayofweek,
                    delivery_time = k.delivery_time,
                    business_uids = ids,
                    
                });
            }
            List<ScheduleInfo> copySchedule = new List<ScheduleInfo>();
            List<ScheduleInfo> temp = new List<ScheduleInfo>();
            foreach (ScheduleInfo a in schedule)
            {
                copySchedule.Add(a);
            }
            
            deliverySchedule.Sort();
            foreach(DateTime a in deliverySchedule)
            {
                temp.Add(new ScheduleInfo()
                {
                    delivery_date = a.ToString("MMM") + " " + a.Day.ToString(),
                    delivery_shortname = a.DayOfWeek.ToString().Substring(0, 3).ToUpper(),
                    delivery_dayofweek = a.DayOfWeek.ToString(),
                    delivery_time = "",
                    business_uids = schedule[0].business_uids,
                    
                }); ;

                ////delivery_dayofweek = date.DayOfWeek.ToString(),
                ////delivery_shortname = date.DayOfWeek.ToString().Substring(0, 3).ToUpper(),
                ////delivery_date = monthNames[date.Month] + " " + date.Day
                //Debug.WriteLine(a);
                //for (int i = 0; i < copySchedule.Count; i++)
                //{
                //    if(copySchedule[i].status == "NON-ACTIVE")
                //    {
                //        if (a.DayOfWeek.ToString().ToUpper() == copySchedule[i].delivery_dayofweek.ToUpper())
                //        {
                //            var d = copySchedule[i];
                //            d.delivery_date = a.ToString("MMM") + " " + a.Day.ToString();
                //            temp.Add(d);
                //            copySchedule[i].status = "ACTIVE";
                //        }
                //    }
                //}
            }

            //foreach (ScheduleInfo a in temp)
            //{
            //    foreach (ScheduleInfo b in schedule)
            //    {
            //        if (a.delivery_dayofweek == b.delivery_dayofweek && a.startTime.Trim() == b.startTime.ToUpper().Trim())
            //        {
            //            a.delivery_time = b.delivery_time;
            //            a.business_uids = b.business_uids;
            //        }
            //    }
            //}

            schedule = temp;
            //Debug.WriteLine(a);
            //var d = new ScheduleInfo();
            //d.delivery_shortname = a.DayOfWeek.ToString().Substring(0, 3).ToUpper();
            //d.delivery_date = a.ToString("MMM") + " " + a.Day.ToString();
            //temp.Add(d);


            //if (schedule.Count != 0)
            //{
            //    Debug.WriteLine("DELIVERY CHECK POINT");
            //    var firstElement = schedule[0];
            //    var dateE1 = firstElement.delivery_date;
            //    var timeE1 = "";

            //    foreach (char a in firstElement.delivery_time.ToCharArray())
            //    {
            //        if (a != '-')
            //        {
            //            timeE1 += a;
            //        }
            //    }
            //    timeE1.Trim().ToUpper();
            //    Debug.WriteLine("DELIVERY DATE                      : " + dateE1);
            //    Debug.WriteLine("DELIVERY TIME                      : " + timeE1);
            //    var timeStampDate = DateTime.Parse(dateE1);
            //    var timeStampTime = DateTime.Parse(timeE1);

            //    Debug.WriteLine("DELIVERY TIME AS DATETIME TYPE DATE: " + timeStampDate);
            //    Debug.WriteLine("DELIVERY TIME AS DATETIME TYPE TIME: " + timeStampTime);
            //    var timeStampString = timeStampDate.ToString("MM/dd/yyyy") + " " + timeStampTime.ToString("HH:mm:ss");
            //    var timeStampE1 = DateTime.Parse(timeStampString);
            //    Debug.WriteLine("DELIVERY TIME AS DATETIME TYPE     : " + timeStampE1);
            //    var targetDate = timeStampE1.AddDays(-1).ToString("MM/dd/yyyy");
            //    var targetTime = DateTime.Parse("01:00 PM").ToString("HH:mm:ss");
            //    var targetTimeString = targetDate + " " + targetTime;
            //    var targetTimeStamp = DateTime.Parse(targetTimeString);
            //    Debug.WriteLine("TARGET TIME AS DATETIME TYPE       : " + targetTimeStamp);

            //    var currentTime = DateTime.Now;
            //    Debug.WriteLine("CURRENT TIME AS DATETIME TYPE      : " + currentTime);
            //    if (currentTime < targetTimeStamp)
            //    {

            //    }
            //    else
            //    {
            //        if (schedule.Count == 1)
            //        {
            //            firstElement.delivery_date = timeStampE1.AddDays(7).ToString("MMM dd");
            //            schedule[0] = firstElement;
            //        }
            //        else
            //        {
            //            var lastElement = firstElement;
            //            schedule.RemoveAt(0);
            //            lastElement.delivery_date = timeStampE1.AddDays(7).ToString("MMM dd");
            //            schedule.Add(lastElement);
            //        }
            //    }
            //}
            //delivery_list.ItemsSource = schedule;
            delivery_list.ItemsSource = schedule;
        }

        void ResetDays()
        {
            //List<string> business_uids = new List<string>();
            //if (selected_farm_id != "")
            //{
            //    business_uids.Add(selected_farm_id);
            //}
            //else
            //{
            //    foreach (BusinessCard bc in Farms)
            //    {
            //        business_uids.Add(bc.business_uid);
            //    }
            //}
            //Deliveries.Clear();
            //foreach (DeliveriesModel dm in AllDeliveries)
            //{
            //    if (anyBusinessesOpen(business_uids, dm.delivery_dayofweek))
            //        Deliveries.Add(dm);
            //}
        }

        public List<ScheduleInfo> displaySchedule = new List<ScheduleInfo>();
 
        public async void GetBusinesses()
        {
            string userLat = (string)Application.Current.Properties["user_latitude"];
            string userLong = (string)Application.Current.Properties["user_longitude"];
            if (userLat == "0" && userLong == "0")
            {
                userLong = "-121.8866517";
                userLat = "37.2270928";
            }

            var client = new HttpClient();
            var response = await client.GetAsync(Constant.ZoneUrl + userLong + "," + userLat);
            Debug.WriteLine(Constant.ZoneUrl + userLong + "," + userLat);
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine("List of farms: " + result);

            Application.Current.Properties["zone"] = "";
                
            if (response.IsSuccessStatusCode)
            {

                data = JsonConvert.DeserializeObject<ServingFreshBusiness>(result);
                //var filterData = new ServingFreshBusiness();
                //filterData.result = new List<Business>();

                var currentDate = DateTime.Now;
                var tempDateTable = GetTable(currentDate);

                Debug.WriteLine("TEMP TABLE FOR LOOK UPS");
                foreach(DateTime t in tempDateTable)
                {
                    Debug.WriteLine(t);
                }

                foreach (Business a in data.result)
                {
                    var acceptingDate = LastAcceptingOrdersDate(tempDateTable, a.z_accepting_day, a.z_accepting_time);
                    var deliveryDate = new DateTime();
                    Debug.WriteLine("CURRENT DATE: " + currentDate);
                    Debug.WriteLine("LAST ACCEPTING DATE: " + acceptingDate);

                    if (currentDate < acceptingDate)
                    {
                        Debug.WriteLine("ONTIME");
                        
                        //deliveryDate = BussinesDeliveryDate(acceptingDate, a.z_delivery_day, a.z_accepting_time);
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
                            //element.orderExpTime = "Order by " + acceptingDate.ToString("MMM dd");
                            Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            Debug.WriteLine("business_uids list: ");

                            foreach(string ID in element.business_uids)
                            {
                                Debug.WriteLine(ID);
                            }

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

                                    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    Debug.WriteLine("business_uids list: ");

                                    foreach (string ID in element.business_uids)
                                    {
                                        Debug.WriteLine(ID);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("NON ONTIME! -> ROLL OVER TO NEXT DELIVERY DATE");

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
                            //element.orderExpTime = "Order by " + acceptingDate.AddDays(7).ToString("MMM dd");
                            Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            Debug.Write("business_uids list: ");

                            foreach (string ID in element.business_uids)
                            {
                                Debug.Write(ID + ", ");
                            }

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

                                    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    Debug.Write("business_uids list: ");

                                    foreach (string ID in element.business_uids)
                                    {
                                        Debug.Write(ID + ", ");
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }

                // DISPLAY SCHEDULE ELEMENTS;
                Debug.WriteLine("");
                Debug.WriteLine("");
                Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS NOT SORTED");
                Debug.WriteLine("");

                foreach (ScheduleInfo element in displaySchedule)
                {
                    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                    Debug.Write("business_uids list: ");

                    foreach (string ID in element.business_uids)
                    {
                        Debug.Write(ID + ", ");
                    }

                    Debug.WriteLine("");
                    Debug.WriteLine("");
                }

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

                Debug.WriteLine("");
                Debug.WriteLine("");
                Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS SORTED");
                Debug.WriteLine("");

                foreach (ScheduleInfo element in displaySchedule)
                {
                    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                    Debug.Write("business_uids list: ");

                    foreach (string ID in element.business_uids)
                    {
                        Debug.Write(ID + ", ");
                    }

                    Debug.WriteLine("");
                    Debug.WriteLine("");
                }


                // ALGORITHM WHEN USING BUSINESS ACCEPTING HOURS
                // =============================================
                //foreach (Business a in data.result)
                //{
                //    var hours = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(a.business_accepting_hours);
                //    foreach(string key in hours.Keys)
                //    {
                //        var day = DateTime.Now.DayOfWeek;
                //        if(key == day.ToString())
                //        {
                //            if(hours[key][0] != "" && hours[key][1] != "")
                //            {
                //                // BUSINESS IS OPENED, BUT HAVE TO CHECK IF WE ARE ONTIME
                //                var startT = DateTime.Parse(hours[key][0]);
                //                var endT = DateTime.Parse(hours[key][1]);
                //                var currentT = DateTime.Now;

                //                if(startT <= currentT && currentT <= endT)
                //                {

                //                    Debug.WriteLine("BUSINESS IS ACCEPTING ORDERS");
                //                    Debug.WriteLine("START: " + startT);
                //                    Debug.WriteLine("END: " + endT);
                //                    Debug.WriteLine("CURRENT: " + currentT);
                //                    filterData.result.Add(a);
                //                }

                //            }
                //        }
                //    }
                //}

                //foreach (Business a in filterData.result)
                //{
                //    Debug.WriteLine(a.business_name);
                //}

                //data.result = filterData.result;


                if (result.Contains("280") && data.result.Count != 0)
                {
                    // Parse it
                    Application.Current.Properties["zone"] = data.result[0].zone;
                    Debug.WriteLine("Zone to save: " + Application.Current.Properties["zone"]);
                    Debug.WriteLine("Parsing Data");
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
                        Debug.WriteLine(id + " :");
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

                    foreach(DeliveryInfo i in deliveryDays)
                    {
                        Debug.WriteLine(i.delivery_day);
                        Debug.WriteLine(i.delivery_time);
                    }

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

            Debug.WriteLine("TEMP TABLE FOR LOOK UPS");
            foreach (DateTime t in tempDateTable)
            {
                Debug.WriteLine(t);
            }

            foreach (Business a in data.result)
            {
                if(businessID == a.z_biz_id)
                {
                    var acceptingDate = LastAcceptingOrdersDate(tempDateTable, a.z_accepting_day, a.z_accepting_time);
                    var deliveryDate = new DateTime();
                    Debug.WriteLine("CURRENT DATE: " + currentDate);
                    Debug.WriteLine("LAST ACCEPTING DATE: " + acceptingDate);

                    if (currentDate < acceptingDate)
                    {
                        Debug.WriteLine("ONTIME");
                        
                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);
                        if(deliveryDate < acceptingDate)
                        {
                            deliveryDate = deliveryDate.AddDays(7);
                        }
                        //deliveryDate = BussinesDeliveryDate(acceptingDate, a.z_delivery_day, a.z_accepting_time);
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
                            //element.orderExpTime = "Order by " + acceptingDate.ToString("MMM dd");
                            Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            Debug.WriteLine("business_uids list: ");

                            foreach (string ID in element.business_uids)
                            {
                                Debug.WriteLine(ID);
                            }

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

                                    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    Debug.WriteLine("business_uids list: ");

                                    foreach (string ID in element.business_uids)
                                    {
                                        Debug.WriteLine(ID);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("NON ONTIME! -> ROLL OVER TO NEXT DELIVERY DATE");

                        deliveryDate = bussinesDeliveryDate(a.z_delivery_day, a.z_delivery_time);
                        //deliveryDate = bussinesDeliveryDate

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
                            //element.orderExpTime = "Order by " + acceptingDate.AddDays(7).ToString("MMM dd");
                            Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                            Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                            Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                            Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                            Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                            Debug.Write("business_uids list: ");

                            foreach (string ID in element.business_uids)
                            {
                                Debug.Write(ID + ", ");
                            }

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

                                    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                                    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                                    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                                    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                                    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                                    Debug.Write("business_uids list: ");

                                    foreach (string ID in element.business_uids)
                                    {
                                        Debug.Write(ID + ", ");
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // DISPLAY SCHEDULE ELEMENTS;
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS NOT SORTED");
            Debug.WriteLine("");

            foreach (ScheduleInfo element in businesDisplaySchedule)
            {
                Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                Debug.Write("business_uids list: ");

                foreach (string ID in element.business_uids)
                {
                    Debug.Write(ID + ", ");
                }

                Debug.WriteLine("");
                Debug.WriteLine("");
            }

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

            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS SORTED");
            Debug.WriteLine("");

            foreach (ScheduleInfo element in businesDisplaySchedule)
            {
                Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                Debug.Write("business_uids list: ");

                foreach (string ID in element.business_uids)
                {
                    Debug.Write(ID + ", ");
                }

                Debug.WriteLine("");
                Debug.WriteLine("");
            }
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
            Debug.WriteLine("DELIVERYY DATE IN BUSINESS" + deliveryDate);
            Debug.WriteLine("LAST ACCEPTING DATE IN BUSINESS" + lastAcceptingOrdersDate);
            if (deliveryDate < lastAcceptingOrdersDate)
            {
                deliveryDate = deliveryDate.AddDays(7);
            }
            Debug.WriteLine("DEFAULT DELIVERY DATE: " + deliveryDate);

            for (int i = 0; i < 7; i++)
            {
                if (deliveryDate.DayOfWeek.ToString().ToUpper() == day.ToUpper())
                {
                    break;
                }
                deliveryDate = deliveryDate.AddDays(1);
            }

            Debug.WriteLine("DELIVERY DATE: " + deliveryDate);
            return deliveryDate;
        }


        public DateTime lastAcceptingOrdersDate(string day, string hour)
        {
            var acceptingDate = DateTime.Parse(hour);

            Debug.WriteLine("DEFAULT ACCEPTING ORDERS DATE: " + acceptingDate);
            Debug.WriteLine("LAST ACCEPTING DAY: " + day.ToUpper());

            for (int i = 0; i < 7; i++)
            {
                if (acceptingDate.DayOfWeek.ToString().ToUpper() == day.ToUpper())
                {
                    break;
                }
                acceptingDate = acceptingDate.AddDays(1);
            }

            Debug.WriteLine("LAST ACCEPTING ORDERS DATE: " + acceptingDate);
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
            Debug.WriteLine("DEFAULT DELIVERY DATE: " + deliveryDate);

            for (int i = 0; i < 7; i++)
            {
                if (deliveryDate.DayOfWeek.ToString().ToUpper() == day.ToUpper())
                {
                    break;
                }
                deliveryDate = deliveryDate.AddDays(1);
            }

            Debug.WriteLine("DELIVERY DATE: " + deliveryDate);
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

        void Change_Color(Object sender, EventArgs e)
        {
            //try
            //{
            //    var imgbtn = (ImageButton)sender;
            //    var sl = (StackLayout)imgbtn.Parent;
            //    var l = (Label)sl.Children[1];
            //    var type = "";
            //    if (l.Text == "Fruits") type = "fruit";
            //    else if (l.Text == "Vegetables") type = "vegetable";
            //    else if (l.Text == "Desserts") type = "dessert";
            //    else if (l.Text == "Other") type = "other";
            //    var tint = (TintImageEffect)imgbtn.Effects[0];
            //    if (tint.TintColor == Constants.PrimaryColor)
            //    {
            //        tint.TintColor = Constants.SecondaryColor;
            //        types.Add(type);
            //    }
            //    else if (tint.TintColor == Constants.SecondaryColor)
            //    {
            //        tint.TintColor = Constants.PrimaryColor;
            //        types.Remove(type);
            //    }
            //    imgbtn.Effects.Clear();
            //    imgbtn.Effects.Add(tint);
            //    ResetFarmersMarkets();
            //    ResetFarms();
            //    ResetDays();
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
           
        }

        void Update_Item_Types()
        {
            //bool hasFruit = false;
            //bool hasVegetable = false;
            //bool hasDessert = false;
            //bool hasOther = false;
            //foreach (BusinessCard bc in Farms)
            //{
            //    if (!hasFruit && bc.item_type.Contains("fruit")) hasFruit = true;
            //    if (!hasVegetable && bc.item_type.Contains("vegetable")) hasVegetable = true;
            //    if (!hasDessert && bc.item_type.Contains("dessert")) hasDessert = true;
            //    if (!hasOther && bc.item_type.Contains("other")) hasOther = true;
            //}
            //var tint = (TintImageEffect)FruitIcon.Effects[0];
            //if (hasFruit) tint.TintColor = types.Contains("fruit") ? Constants.SecondaryColor : Constants.PrimaryColor;
            //else tint.TintColor = Color.LightGray;
            //FruitIcon.Effects.Clear();
            //FruitIcon.Effects.Add(tint);
            //tint = (TintImageEffect)VegetableIcon.Effects[0];
            //if (hasVegetable) tint.TintColor = types.Contains("vegetable") ? Constants.SecondaryColor : Constants.PrimaryColor;
            //else tint.TintColor = Color.LightGray;
            //VegetableIcon.Effects.Clear();
            //VegetableIcon.Effects.Add(tint);
            //tint = (TintImageEffect)DessertIcon.Effects[0];
            //if (hasDessert) tint.TintColor = types.Contains("dessert") ? Constants.SecondaryColor : Constants.PrimaryColor;
            //else tint.TintColor = Color.LightGray;
            //DessertIcon.Effects.Clear();
            //DessertIcon.Effects.Add(tint);
            //tint = (TintImageEffect)OtherIcon.Effects[0];
            //if (hasOther) tint.TintColor = types.Contains("other") ? Constants.SecondaryColor : Constants.PrimaryColor;
            //else tint.TintColor = Color.LightGray;
            //OtherIcon.Effects.Clear();
            //OtherIcon.Effects.Add(tint);
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

                Debug.WriteLine("");
                Debug.WriteLine("");
                Debug.WriteLine("DISPLAY SCHEDULE ELEMENTS SORTED");
                Debug.WriteLine("");

                foreach (ScheduleInfo element in businessDisplaySchedule)
                {
                    Debug.WriteLine("element.delivery_date: " + element.delivery_date);
                    Debug.WriteLine("element.delivery_dayofweek: " + element.delivery_dayofweek);
                    Debug.WriteLine("element.delivery_shortname: " + element.delivery_shortname);
                    Debug.WriteLine("element.delivery_time: " + element.delivery_time);
                    Debug.WriteLine("element.deliveryTimeStamp: " + element.deliveryTimeStamp);
                    Debug.Write("business_uids list: ");

                    foreach (string ID in element.business_uids)
                    {
                        Debug.Write(ID + ", ");
                    }

                    Debug.WriteLine("");
                    Debug.WriteLine("");
                }

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
