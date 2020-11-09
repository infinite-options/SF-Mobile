using System;
using System.Collections.Generic;
using Xamarin.Forms;
using ServingFresh.Config;
using ServingFresh.Effects;
using ServingFresh.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
using Newtonsoft.Json;

namespace ServingFresh.Views
{
    public partial class SelectionPage : ContentPage
    {

        class BusinessCard
        {
            public string business_image { get; set; }
            public string business_name { get; set; }
            public string item_type { get; set; }
            public string business_uid { get; set; }
            public string business_type { get; set; }
            public Color border_color { get; set; }
        }
        BusinessCard unselectedBusiness(Business b)
        {
            return new BusinessCard()
            {
                business_image = b.business_image,
                business_name = b.business_name,
                item_type = b.item_type,
                business_uid = b.business_uid,
                business_type = b.business_type,
                border_color = Color.LightGray
            };
        }
        BusinessCard selectedBusiness(Business b)
        {
            return new BusinessCard()
            {
                business_image = b.business_image,
                business_name = b.business_name,
                item_type = b.item_type,
                business_uid = b.business_uid,
                business_type = b.business_type,
                border_color = Constants.SecondaryColor
            };
        }

        List<DeliveriesModel> AllDeliveries = new List<DeliveriesModel>();
        List<Business> AllFarms = new List<Business>();
        List<Business> AllFarmersMarkets = new List<Business>();
        ObservableCollection<DeliveriesModel> Deliveries = new ObservableCollection<DeliveriesModel>();
        ObservableCollection<BusinessCard> Farms = new ObservableCollection<BusinessCard>();
        ObservableCollection<BusinessCard> FarmersMarkets = new ObservableCollection<BusinessCard>();
        List<string> types = new List<string>();
        List<string> b_uids = new List<string>();
        string selected_market_id = "";
        string selected_farm_id = "";

        public SelectionPage()
        {
            InitializeComponent();
            Init();
            GetBusinesses();
            GetDays();
            CartTotal.Text = CheckoutPage.total_qty.ToString();
        }

        void Init()
        {
            BackgroundColor = Constants.PrimaryColor;
            delivery_list.ItemsSource = Deliveries;
            market_list.ItemsSource = FarmersMarkets;
            farm_list.ItemsSource = Farms;
        }

        void GetDays()
        {
            AllDeliveries.Clear();
            var date = DateTime.Now.AddHours(7);
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
                AllDeliveries.Add(new DeliveriesModel()
                {
                    delivery_dayofweek = date.DayOfWeek.ToString(),
                    delivery_shortname = date.DayOfWeek.ToString().Substring(0, 3).ToUpper(),
                    delivery_date = monthNames[date.Month] + " " + date.Day
                });
                date = date.AddDays(1);
            }
            Deliveries.Clear();
            foreach (DeliveriesModel dm in AllDeliveries) Deliveries.Add(dm);
        }

        void ResetDays()
        {
            List<string> business_uids = new List<string>();
            if (selected_farm_id != "")
            {
                business_uids.Add(selected_farm_id);
            }
            else
            {
                foreach (BusinessCard bc in Farms)
                {
                    business_uids.Add(bc.business_uid);
                }
            }
            Deliveries.Clear();
            foreach (DeliveriesModel dm in AllDeliveries)
            {
                if (anyBusinessesOpen(business_uids, dm.delivery_dayofweek))
                    Deliveries.Add(dm);
            }
        }

        void ResetFarms()
        {
            if (selected_farm_id != "") return;
            Farms.Clear();
            if (selected_market_id != "")
            {
                foreach (Business b in AllFarms)
                {
                    if (b.business_association != null)
                    {
                        var association = JsonConvert.DeserializeObject<List<string>>(b.business_association);
                        if (association.Contains(selected_market_id))
                        {
                            bool matchesItemType = true;
                            foreach (string type in types)
                            {
                                if (!b.item_type.Contains(type)) matchesItemType = false;
                            }
                            if (matchesItemType) Farms.Add(unselectedBusiness(b));
                        }
                    }
                }
            }
            else
            {
                foreach (Business b in AllFarms)
                {
                    bool matchesItemType = true;
                    foreach (string type in types)
                    {
                        if (!b.item_type.Contains(type)) matchesItemType = false;
                    }
                    if (matchesItemType) Farms.Add(unselectedBusiness(b));
                }
            }
        }

        void ResetFarmersMarkets()
        {
            if (selected_market_id != "") return;
            FarmersMarkets.Clear();
            if (selected_farm_id != "")
            {
                foreach (Business b in AllFarms)
                {
                    if (b.business_uid == selected_farm_id)
                    {
                        foreach (Business market in AllFarmersMarkets)
                        {
                            if (b.business_association != null &&
                                b.business_association.Contains(market.business_uid))
                            {
                                FarmersMarkets.Add(unselectedBusiness(market));
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (Business b in AllFarmersMarkets)
                {
                    FarmersMarkets.Add(unselectedBusiness(b));
                }
            }
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
            // post and champ market.
            var client = new HttpClient();
            var response = await client.GetAsync(Constant.ZoneUrl + userLong + "," + userLat);
            string result = response.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<ServingFreshBusiness>(result);
            AllFarmersMarkets.Clear();
            AllFarms.Clear();
            if (data.result != null)
            {
                foreach (Business b in data.result)
                {
                    if (b.business_type == "Farm") AllFarms.Add(b);
                    else if (b.business_type == "Farmers Market") AllFarmersMarkets.Add(b);
                }
            }
            ResetFarms();
            ResetFarmersMarkets();
            ResetDays();
        }

        bool isBusinessOpen(string business_uid, string weekday)
        {
            foreach (Business b in AllFarms)
            {
                if (b.business_uid == business_uid)
                {
                    var hours = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(b.business_delivery_hours);
                    return hours[weekday][0] != hours[weekday][1];
                }
            }
            return false;
        }
        bool anyBusinessesOpen(List<string> business_uids, string weekday)
        {
            bool anyOpen = false;
            foreach (string s in business_uids)
            {
                if (isBusinessOpen(s, weekday))
                {
                    anyOpen = true;
                }
            }
            return anyOpen;
        }

        void Open_Checkout(Object sender, EventArgs e)
        {

            Application.Current.MainPage = new CheckoutPage(null);
        }

        void Open_Farm(Object sender, EventArgs e)
        {
            var sl = (StackLayout)sender;
            var tgr = (TapGestureRecognizer)sl.GestureRecognizers[0];
            var dm = (DeliveriesModel)tgr.CommandParameter;
            string weekday = dm.delivery_dayofweek;
            if (types.Count == 0)
            {
                types.Add("fruit");
                types.Add("vegetable");
                types.Add("dessert");
                types.Add("other");
            }
            b_uids.Clear();
            if (selected_farm_id != "") b_uids.Add(selected_farm_id);
            else
            {
                foreach (BusinessCard bc in Farms)
                {
                    if (isBusinessOpen(bc.business_uid, weekday)) b_uids.Add(bc.business_uid);
                }
            }

            foreach (string type in types)
            {
                System.Diagnostics.Debug.WriteLine(type);
            }

            // Delivery Times of all business who are avaiable this weeek day
            string startTime = "";
            string endTime = "";
            if(b_uids.Count != 0)
            {
                foreach (string ids in b_uids)
                {
                    foreach (Business b in AllFarms)
                    {
                        if (b.business_uid == ids)
                        {
                            var deliveryTime = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(b.business_delivery_hours);
                            startTime = deliveryTime[weekday][0];
                            endTime = deliveryTime[weekday][1];
                            System.Diagnostics.Debug.WriteLine("Start Delivery Time (string): " + deliveryTime[weekday][0]);
                            System.Diagnostics.Debug.WriteLine("End Delivery Time (string): " + deliveryTime[weekday][1]);
                            break;
                        }
                    }
                    break;
                }

                DateTime sTime = DateTime.Parse(startTime);
                DateTime eTime = DateTime.Parse(endTime);

                string deliveryMonth = dm.delivery_date;
                string deliveryDay = dm.delivery_shortname;
                string deliveryStartTime = sTime.ToString("hh:mm tt");
                string deliveryEndTime = eTime.ToString("hh:mm tt");
                string deliveryDate = deliveryDay + ", " + deliveryMonth + ", " + DateTime.Now.Year;

                // System.Diagnostics.Debug.WriteLine("Delivery Date: " + deliveryDate);
                // System.Diagnostics.Debug.WriteLine("Start Delivery Time (string): " + deliveryStartTime);
                // System.Diagnostics.Debug.WriteLine("End Delivery Time (string): " + deliveryEndTime);

                Application.Current.Properties["delivery_date"] = deliveryDate;
                Application.Current.Properties["delivery_time"] = "bwt: "+ deliveryStartTime + " - " + deliveryEndTime;
                Application.Current.Properties["delivery_start_time"] = sTime.ToString("hh:mm:ss");
                System.Diagnostics.Debug.WriteLine(Application.Current.Properties["delivery_date"]);
                System.Diagnostics.Debug.WriteLine(Application.Current.Properties["delivery_time"]);
            }
            else
            {
                Application.Current.Properties["delivery_date"] = "";
                Application.Current.Properties["delivery_time"] = "";
            }

            ItemsPage businessItemPage = new ItemsPage(types, b_uids, weekday);
            Application.Current.MainPage = businessItemPage;
        }

        void Change_Color(Object sender, EventArgs e)
        {
            var imgbtn = (ImageButton)sender;
            var sl = (StackLayout)imgbtn.Parent;
            var l = (Label)sl.Children[1];
            var type = "";
            if (l.Text == "Fruits") type = "fruit";
            else if (l.Text == "Vegetables") type = "vegetable";
            else if (l.Text == "Desserts") type = "dessert";
            else if (l.Text == "Other") type = "other";
            var tint = (TintImageEffect)imgbtn.Effects[0];
            if (tint.TintColor == Constants.PrimaryColor)
            {
                tint.TintColor = Constants.SecondaryColor;
                types.Add(type);
            }
            else if (tint.TintColor == Constants.SecondaryColor)
            {
                tint.TintColor = Constants.PrimaryColor;
                types.Remove(type);
            }
            imgbtn.Effects.Clear();
            imgbtn.Effects.Add(tint);
            ResetFarmersMarkets();
            ResetFarms();
            ResetDays();
        }

        void Update_Item_Types()
        {
            bool hasFruit = false;
            bool hasVegetable = false;
            bool hasDessert = false;
            bool hasOther = false;
            foreach (BusinessCard bc in Farms)
            {
                if (!hasFruit && bc.item_type.Contains("fruit")) hasFruit = true;
                if (!hasVegetable && bc.item_type.Contains("vegetable")) hasVegetable = true;
                if (!hasDessert && bc.item_type.Contains("dessert")) hasDessert = true;
                if (!hasOther && bc.item_type.Contains("other")) hasOther = true;
            }
            var tint = (TintImageEffect)FruitIcon.Effects[0];
            if (hasFruit) tint.TintColor = types.Contains("fruit") ? Constants.SecondaryColor : Constants.PrimaryColor;
            else tint.TintColor = Color.LightGray;
            FruitIcon.Effects.Clear();
            FruitIcon.Effects.Add(tint);
            tint = (TintImageEffect)VegetableIcon.Effects[0];
            if (hasVegetable) tint.TintColor = types.Contains("vegetable") ? Constants.SecondaryColor : Constants.PrimaryColor;
            else tint.TintColor = Color.LightGray;
            VegetableIcon.Effects.Clear();
            VegetableIcon.Effects.Add(tint);
            tint = (TintImageEffect)DessertIcon.Effects[0];
            if (hasDessert) tint.TintColor = types.Contains("dessert") ? Constants.SecondaryColor : Constants.PrimaryColor;
            else tint.TintColor = Color.LightGray;
            DessertIcon.Effects.Clear();
            DessertIcon.Effects.Add(tint);
            tint = (TintImageEffect)OtherIcon.Effects[0];
            if (hasOther) tint.TintColor = types.Contains("other") ? Constants.SecondaryColor : Constants.PrimaryColor;
            else tint.TintColor = Color.LightGray;
            OtherIcon.Effects.Clear();
            OtherIcon.Effects.Add(tint);
        }

        void Change_Border_Color(Object sender, EventArgs e)
        {
            var f = (Frame)sender;
            var tgr = (TapGestureRecognizer)f.GestureRecognizers[0];
            var bc = (BusinessCard)tgr.CommandParameter;
            if (bc.border_color == Color.LightGray)
            {
                if (bc.business_type == "Farmers Market")
                {
                    selected_market_id = bc.business_uid;
                    FarmersMarkets.Clear();
                    foreach (Business b in AllFarmersMarkets)
                    {
                        if (b.business_uid == bc.business_uid)
                        {
                            FarmersMarkets.Add(selectedBusiness(b));
                        }
                    }
                    ResetFarms();
                }
                else
                {
                    selected_farm_id = bc.business_uid;
                    Farms.Clear();
                    foreach (Business b in AllFarms)
                    {
                        if (b.business_uid == bc.business_uid) Farms.Add(selectedBusiness(b));
                    }
                    ResetFarmersMarkets();
                }
            }
            else
            {
                if (bc.business_type == "Farmers Market")
                {
                    selected_market_id = "";
                }
                else
                {
                    selected_farm_id = "";
                }
                ResetFarms();
                ResetFarmersMarkets();
            }
            ResetDays();
            Update_Item_Types();
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
