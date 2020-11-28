﻿using System;
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
        }

        List<DeliveryInfo> deliveryDays = new List<DeliveryInfo>();
        List<string> deliveryDayList = new List<string>();

        List<DeliveriesModel> deliveryScheduleUnfiltered = new List<DeliveriesModel>();
        List<Delivery> deliveryScheduleFiltered = new List<Delivery>();
        List<ScheduleInfo> schedule = new List<ScheduleInfo>();

        List<string> businessList = new List<string>();
        List<BusinessCard> businesses = new List<BusinessCard>();
        List<BusinessCard> business = new List<BusinessCard>();
        public ServingFreshBusiness data = new ServingFreshBusiness();

        public SelectionPage()
        {
            InitializeComponent();
            Init();
            GetBusinesses();
            CartTotal.Text = CheckoutPage.total_qty.ToString();
        }

        void Init()
        {
            BackgroundColor = Constants.PrimaryColor;
            //delivery_list.ItemsSource = Deliveries;
            //market_list.ItemsSource = FarmersMarkets;
            //farm_list.ItemsSource = Farms;
        }

        void GetDays()
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
                schedule.Add(new ScheduleInfo()
                {
                    delivery_date = k.delivery_date,
                    delivery_shortname = k.delivery_shortname,
                    delivery_dayofweek = k.delivery_dayofweek,
                    delivery_time = k.delivery_time,
                    business_uids = ids,
                });
            }
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
 
        async void GetBusinesses()
        {
            string userLat = (string)Application.Current.Properties["user_latitude"];
            string userLong = (string)Application.Current.Properties["user_longitude"];
            if (userLat == "0" && userLong == "0")
            {
                userLong = "-121.924799";
                userLat = "37.364027";
            }

            var client = new HttpClient();
            var response = await client.GetAsync(Constant.ZoneUrl + userLong + "," + userLat);
            var result = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(result);

            if (response.IsSuccessStatusCode)
            {

                data = JsonConvert.DeserializeObject<ServingFreshBusiness>(result);
                if (result.Contains("280"))
                {
                    // Parse it
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

                    GetDays();
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
                await DisplayAlert("Oops!", "Our system is down. We are working to fix this issue.", "OK");
                return;
            }
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
                farm_list.ItemsSource = business;
                List<ScheduleInfo> bussinesSchedule = new List<ScheduleInfo>();
                foreach(string day in bc.list_delivery_days.Keys)
                {

                    List<string> times = (List<string>)bc.list_delivery_days[day];
                    foreach (string t in times)
                    {
                        foreach (ScheduleInfo i in schedule)
                        {
                            if(day.ToUpper() == i.delivery_dayofweek.ToUpper() && t == i.delivery_time)
                            {
                                bussinesSchedule.Add(i);
                            }
                        }
                    }
                }
                delivery_list.ItemsSource = bussinesSchedule;
            }
            else
            {
                farm_list.ItemsSource = businesses;
                delivery_list.ItemsSource = schedule;
            }
                //    if (bc.business_type == "Farmers Market")
                //    {
                //        selected_market_id = bc.business_uid;
                //        FarmersMarkets.Clear();
                //        foreach (Business b in AllFarmersMarkets)
                //        {
                //            if (b.business_uid == bc.business_uid)
                //            {
                //                FarmersMarkets.Add(selectedBusiness(b));
                //            }
                //        }
                //        ResetFarms();
                //    }
                //    else
                //    {
                //        selected_farm_id = bc.business_uid;
                //        Farms.Clear();
                //        foreach (Business b in AllFarms)
                //        {
                //            if (b.business_uid == bc.business_uid) Farms.Add(selectedBusiness(b));
                //        }
                //        ResetFarmersMarkets();
                //    }
                //}
                //else
                //{
                //    if (bc.business_type == "Farmers Market")
                //    {
                //        selected_market_id = "";
                //    }
                //    else
                //    {
                //        selected_farm_id = "";
                //    }
                //    ResetFarms();
                //    ResetFarmersMarkets();
                //}
                //ResetDays();
                //Update_Item_Types();
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
            Application.Current.MainPage = new InfoPage();
        }

        void ProfileClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new ProfilePage();
        }
    }
}
