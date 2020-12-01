using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ServingFresh.Models;
using ServingFresh.Views;
using Newtonsoft.Json;
using Xamarin.Forms;
using ServingFresh.Effects;
using ServingFresh.Config;
using System.Diagnostics;

namespace ServingFresh.Views
{
    public partial class ItemsPage : ContentPage
    {
        public class Items
        {
            public string item_uid { get; set; }
            public string created_at { get; set; }
            public string itm_business_uid { get; set; }
            public string item_name { get; set; }
            public object item_status { get; set; }
            public string item_type { get; set; }
            public string item_desc { get; set; }
            public object item_unit { get; set; }
            public double item_price { get; set; }
            public string item_sizes { get; set; }
            public string favorite { get; set; }
            public string item_photo { get; set; }
            public object exp_date { get; set; }
            public string business_delivery_hours { get; set; }
        }

        public class ServingFreshBusinessItems
        {
            public string message { get; set; }
            public int code { get; set; }
            public IList<Items> result { get; set; }
            public string sql { get; set; }
        }

        public class GetItemPost
        {
            public IList<string> type { get; set; }
            public IList<string> ids { get; set; }
        }

        // THIS VARIABLE SHOULD BE THE COMPLETE SOLUTION THAT ZACK MAY USE
        public ObservableCollection<ItemsModel> datagrid = new ObservableCollection<ItemsModel>();
        ServingFreshBusinessItems data = new ServingFreshBusinessItems();

        public class ItemPurchased
        {
            public string pur_business_uid { get; set; }
            public string item_uid { get; set; }
            public string item_name { get; set; }
            public int item_quantity { get; set; }
            public double item_price { get; set; }
        }

        public IDictionary<string, ItemPurchased> order = new Dictionary<string, ItemPurchased>();
        public int totalCount = 0;
        //ServingFreshBusinessItems data = new ServingFreshBusinessItems();
        public List<string> uids = null;

        private static List<string> uidsCopy = null;
        private static List<string> typesCopy = null;
        
        public ItemsPage(List<string> types, List<string> b_uids, string day)
        {
            InitializeComponent();
            try
            {
                //SetInitialFilters(types);
                uids = b_uids;
                uidsCopy = uids;
                typesCopy = types;
                _ = GetData(types, b_uids);
                titlePage.Text = day;
                itemList.ItemsSource = datagrid;
                CartTotal.Text = totalCount.ToString();
            }catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public ItemsPage(IDictionary<string, ItemPurchased> orderCopy ,string day)
        {
            Debug.WriteLine("You are coming from the checkout page");
            InitializeComponent();
            try
            {
                //SetInitialFilters(types);
                
                
                _ = GetData(typesCopy, uidsCopy);
                titlePage.Text = day;
                itemList.ItemsSource = datagrid;
                CartTotal.Text = orderCopy.Count.ToString();
                order = orderCopy;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            // This are the types and business uids
            foreach (string a in typesCopy)
            {
                Debug.WriteLine(a);
            }
            foreach (string b in uidsCopy)
            {
                Debug.WriteLine(b);
            }

        }

        void SetInitialFilters(List<string> types)
        {
            if (types.Count != 4)
            {
                foreach (string type in types)
                {
                    if (type == "fruit")
                    {
                        var tint = (TintImageEffect)fruit.Effects[0];
                        tint.TintColor = Constants.SecondaryColor;
                    }
                    if (type == "vegetable")
                    {
                        var tint = (TintImageEffect)vegetable.Effects[0];
                        tint.TintColor = Constants.SecondaryColor;
                    }
                    if (type == "dessert")
                    {
                        var tint = (TintImageEffect)dessert.Effects[0];
                        tint.TintColor = Constants.SecondaryColor;
                    }
                    if (type == "other")
                    {
                        var tint = (TintImageEffect)other.Effects[0];
                        tint.TintColor = Constants.SecondaryColor;
                    }
                }
            }
        }

        private async Task GetData(List<string> types, List<string> b_uids)
        {
            try
            {


                GetItemPost post = new GetItemPost();
                post.type = types;
                post.ids = b_uids;

                var client = new HttpClient();
                var getItemsString = JsonConvert.SerializeObject(post);
                var getItemsStringMessage = new StringContent(getItemsString, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage();
                request.Method = HttpMethod.Post;
                request.Content = getItemsStringMessage;

                var httpResponse = await client.PostAsync(Constant.GetItemsUrl, getItemsStringMessage);
                var r = await httpResponse.Content.ReadAsStreamAsync();
                StreamReader sr = new StreamReader(r);
                JsonReader reader = new JsonTextReader(sr);

                if (httpResponse.IsSuccessStatusCode)
                {
                    JsonSerializer serializer = new JsonSerializer();
                    data = serializer.Deserialize<ServingFreshBusinessItems>(reader);

                    this.datagrid.Clear();
                    int n = data.result.Count;
                    int j = 0;
                    if (n == 0)
                    {
                        this.datagrid.Add(new ItemsModel()
                        {
                            height = this.Width / 2 + 25,
                            width = this.Width / 2 - 25,
                            imageSourceLeft = "",
                            quantityLeft = 0,
                            itemNameLeft = "",
                            itemPriceLeft = "$ " + "",
                            isItemLeftVisiable = false,
                            isItemLeftEnable = false,
                            quantityL = 0,

                            imageSourceRight = "",
                            quantityRight = 0,
                            itemNameRight = "",
                            itemPriceRight = "$ " + "",
                            isItemRightVisiable = false,
                            isItemRightEnable = false,
                            quantityR = 0
                        });
                    }
                    if (isAmountItemsEven(n))
                    {
                        for (int i = 0; i < n / 2; i++)
                        {
                            this.datagrid.Add(new ItemsModel()
                            {
                                height = this.Width / 2 + 25,
                                width = this.Width / 2 - 25,
                                imageSourceLeft = data.result[j].item_photo,
                                item_uidLeft = data.result[j].item_uid,
                                itm_business_uidLeft = data.result[j].itm_business_uid,
                                quantityLeft = 0,
                                itemNameLeft = data.result[j].item_name,
                                itemPriceLeft = "$ " + data.result[j].item_price.ToString(),
                                isItemLeftVisiable = true,
                                isItemLeftEnable = true,
                                quantityL = 0,

                                imageSourceRight = data.result[j + 1].item_photo,
                                item_uidRight = data.result[j + 1].item_uid,
                                itm_business_uidRight = data.result[j + 1].itm_business_uid,
                                quantityRight = 0,
                                itemNameRight = data.result[j + 1].item_name,
                                itemPriceRight = "$ " + data.result[j + 1].item_price.ToString(),
                                isItemRightVisiable = true,
                                isItemRightEnable = true,
                                quantityR = 0
                            });
                            j = j + 2;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < n / 2; i++)
                        {
                            this.datagrid.Add(new ItemsModel()
                            {
                                height = this.Width / 2 + 25,
                                width = this.Width / 2 - 25,
                                imageSourceLeft = data.result[j].item_photo,
                                item_uidLeft = data.result[j].item_uid,
                                itm_business_uidLeft = data.result[j].itm_business_uid,
                                quantityLeft = 0,
                                itemNameLeft = data.result[j].item_name,
                                itemPriceLeft = "$ " + data.result[j].item_price.ToString(),
                                isItemLeftVisiable = true,
                                isItemLeftEnable = true,
                                quantityL = 0,

                                imageSourceRight = data.result[j + 1].item_photo,
                                item_uidRight = data.result[j + 1].item_uid,
                                itm_business_uidRight = data.result[j + 1].itm_business_uid,
                                quantityRight = 0,
                                itemNameRight = data.result[j + 1].item_name,
                                itemPriceRight = "$ " + data.result[j + 1].item_price.ToString(),
                                isItemRightVisiable = true,
                                isItemRightEnable = true,
                                quantityR = 0
                            });
                            j = j + 2;
                        }
                        this.datagrid.Add(new ItemsModel()
                        {
                            height = this.Width / 2 + 25,
                            width = this.Width / 2 - 25,
                            imageSourceLeft = data.result[j].item_photo,
                            item_uidLeft = data.result[j].item_uid,
                            itm_business_uidLeft = data.result[j].itm_business_uid,
                            quantityLeft = 0,
                            itemNameLeft = data.result[j].item_name,
                            itemPriceLeft = "$ " + data.result[j].item_price.ToString(),
                            isItemLeftVisiable = true,
                            isItemLeftEnable = true,
                            quantityL = 0,

                            imageSourceRight = "",
                            quantityRight = 0,
                            itemNameRight = "",
                            itemPriceRight = "$ " + "",
                            isItemRightVisiable = false,
                            isItemRightEnable = false,
                            quantityR = 0
                        });
                    }
                }
            }catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public bool isAmountItemsEven(int num)
        {
            bool result = false;
            if (num % 2 == 0) { result = true; }
            return result;
        }

        public ItemsModel itemModelEmpty()
        {
            ItemsModel e = new ItemsModel();

            e.imageSourceLeft = "";
            e.quantityLeft = 0;
            e.itemNameLeft = "";
            e.itemPriceLeft = "";
            e.isItemLeftVisiable = false;
            e.isItemLeftEnable = false;

            e.imageSourceRight = "";
            e.quantityRight = 0;
            e.itemNameRight = "";
            e.itemPriceRight = "";
            e.isItemRightVisiable = false;
            e.isItemRightEnable = false;

            return e;
        }

        public ItemsModel itemModelEven(string imgLeft, string imgRight, string nameLeft, string nameRight, string priceLeft, string priceRight)
        {
            ItemsModel e = new ItemsModel();

            e.imageSourceLeft = imgLeft;
            e.quantityLeft = 0;
            e.itemNameLeft = nameLeft;
            e.itemPriceLeft = "$ " + priceLeft;
            e.isItemLeftVisiable = true;
            e.isItemLeftEnable = true;

            e.imageSourceRight = imgRight;
            e.quantityRight = 0;
            e.itemNameRight = nameRight;
            e.itemPriceRight = "$ " + priceRight;
            e.isItemRightVisiable = true;
            e.isItemRightEnable = true;

            return e;
        }

        public ItemsModel itemModelOdd(string imgLeft, string imgRight, string nameLeft, string nameRight, string priceLeft, string priceRight)
        {
            ItemsModel e = new ItemsModel();

            e.imageSourceLeft = imgLeft;
            e.quantityLeft = 0;
            e.itemNameLeft = nameLeft;
            e.itemPriceLeft = "$ " + priceLeft;
            e.isItemLeftVisiable = true;
            e.isItemLeftEnable = true;

            e.imageSourceRight = imgRight;
            e.quantityRight = 0;
            e.itemNameRight = nameRight;
            e.itemPriceRight = "$ " + priceRight;
            e.isItemRightVisiable = false;
            e.isItemRightEnable = false;

            return e;
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
                    CartTotal.Text = totalCount.ToString();
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
                            order.Add(itemModelObject.itemNameLeft, itemSelected);
                        }
                    }
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
                itemModelObject.quantityL += 1;
                totalCount += 1;
                CartTotal.Text = totalCount.ToString();
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
                        order.Add(itemModelObject.itemNameLeft, itemSelected);
                    }
                }
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
                    CartTotal.Text = totalCount.ToString();
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
                        order.Add(itemModelObject.itemNameRight, itemSelected);
                    }
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
                itemModelObject.quantityR += 1;
                totalCount += 1;
                CartTotal.Text = totalCount.ToString();
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
                    order.Add(itemModelObject.itemNameRight, itemSelected);
                }
            }
        }

        void Change_Color(Object sender, EventArgs e)
        {
            List<string> t = new List<string>();
            if (sender is ImageButton imgbtn)
            {
                if (imgbtn.Effects[0] is TintImageEffect tint)
                {
                    imgbtn.Effects.RemoveAt(0);
                    if (tint.TintColor.Equals(Color.White))
                    {
                        tint.TintColor = Constants.SecondaryColor;
                        var im = (ImageButton)sender;
                        im.ClassId = "T";
                    }
                    else
                    {
                        tint.TintColor = Color.White;
                        var im = (ImageButton)sender;
                        im.ClassId = "F";
                    }
                    imgbtn.Effects.Insert(0, tint);
                }
            }

            if(fruit.ClassId == "T")
            {
                t.Add("fruit");
            }
            if(vegetable.ClassId == "T")
            {
                t.Add("vegetable");
            }
            if(dessert.ClassId == "T")
            {
                t.Add("dessert");
            }
            if(other.ClassId == "T")
            {
                t.Add("other");
            }

            if(fruit.ClassId == "F" && vegetable.ClassId == "F" && dessert.ClassId == "F" && other.ClassId == "F")
            {
                t.Add("fruit");
                t.Add("vegetable");
                t.Add("dessert");
                t.Add("other");
               
            }

            foreach(string ty in t)
            {
                Debug.WriteLine(ty);
            }
            Debug.WriteLine("");
            Debug.WriteLine("");
            _ = GetData(t, uids);
        }

        void CheckOutClickBusinessPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage(order, titlePage.Text);
        }

        void DeliveryDaysClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SelectionPage();
        }

        void OrdersClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new CheckoutPage(order, titlePage.Text);
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
