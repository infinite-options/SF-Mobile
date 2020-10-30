using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using ServingFresh.Config;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using ServingFresh.Models;
using static ServingFresh.Views.ItemsPage;
using Application = Xamarin.Forms.Application;
using Stripe;

namespace ServingFresh.Views
{
    public class ItemObject : INotifyPropertyChanged                                            // INotifyPropertyChanged - don't have to render entire page only the items you changed
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string item_uid { get; set; }
        public string business_uid { get; set; }
        public string name { get; set; }
        public int qty { get; set; }
        public double price { get; set; }
        public string total_price { get { return "$ " + (qty * price).ToString("N2"); } }
        public void increase_qty()
        {
            qty++;
            PropertyChanged(this, new PropertyChangedEventArgs("qty"));
            PropertyChanged(this, new PropertyChangedEventArgs("total_price"));
        }
        public void decrease_qty()
        {
            if (qty == 0) return;
            qty--;
            PropertyChanged(this, new PropertyChangedEventArgs("qty"));
            PropertyChanged(this, new PropertyChangedEventArgs("total_price"));
        }
    }

    public class PurchaseDataObject
    {
        public string pur_customer_uid { get; set; }
        public string pur_business_uid { get; set; }
        public ObservableCollection<PurchasedItem> items { get; set; }
        public string order_instructions { get; set; }
        public string delivery_instructions { get; set; }
        public string order_type { get; set; }
        public string delivery_first_name { get; set; }
        public string delivery_last_name { get; set; }
        public string delivery_phone_num { get; set; }
        public string delivery_email { get; set; }
        public string delivery_address { get; set; }
        public string delivery_unit { get; set; }
        public string delivery_city { get; set; }
        public string delivery_state { get; set; }
        public string delivery_zip { get; set; }
        public string delivery_latitude { get; set; }
        public string delivery_longitude { get; set; }
        public string purchase_notes { get; set; }
        public string start_delivery_date { get; set; }
        public string pay_coupon_id { get; set; }
        public string amount_due { get; set; }
        public string amount_discount { get; set; }
        public string amount_paid { get; set; }
        public string info_is_Addon { get; set; }
        public string cc_num { get; set; }
        public string cc_exp_date { get; set; }
        public string cc_cvv { get; set; }
        public string cc_zip { get; set; }
        public string charge_id { get; set; }
        public string payment_type { get; set; }
    }

    public class PurchaseResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public string sql { get; set; }
    }

    public partial class CheckoutPage : ContentPage
    {
        public PurchaseDataObject purchaseObject;
        public static ObservableCollection<ItemObject> cartItems = new ObservableCollection<ItemObject>();
        public double subtotal;
        public double discount;
        public double delivery_fee;
        public double service_fee;
        public double taxes;
        public double total;
        public static int total_qty = 0;
        private bool isAddressValidated;
        private string latitude = "0";
        private string longitude = "0";


        public CheckoutPage(IDictionary<string, ItemPurchased> order = null, string day = "")
        {
            InitializeComponent();
            InitializeMap();
            if (order != null)
            {
                cartItems.Clear();
                foreach (string key in order.Keys)
                {
                    cartItems.Add(new ItemObject()
                    {
                        qty = order[key].item_quantity,
                        name = order[key].item_name,
                        price = order[key].item_price,
                        item_uid = order[key].item_uid,
                        business_uid = order[key].pur_business_uid
                    });
                }
            }

            purchaseObject = new PurchaseDataObject()
            {
                pur_customer_uid = Application.Current.Properties.ContainsKey("user_id") ? (string)Application.Current.Properties["user_id"] : "",
                pur_business_uid = "",
                items = GetOrder(cartItems),
                order_instructions = "",
                
                delivery_instructions = Application.Current.Properties.ContainsKey("user_delivery_instructions") ? (string)Application.Current.Properties["user_delivery_instructions"] : "",
                
                order_type = "meal",
                delivery_first_name = (string)Application.Current.Properties["user_first_name"],
                delivery_last_name = (string)Application.Current.Properties["user_last_name"],
                delivery_phone_num = (string)Application.Current.Properties["user_phone_num"],
                delivery_email = (string)Application.Current.Properties["user_email"],
                delivery_address = (string)Application.Current.Properties["user_address"],
                delivery_unit = (string)Application.Current.Properties["user_unit"],
                delivery_city = (string)Application.Current.Properties["user_city"],
                delivery_state = (string)Application.Current.Properties["user_state"],
                delivery_zip = (string)Application.Current.Properties["user_zip_code"],
                delivery_latitude = (string)Application.Current.Properties["user_latitude"],
                delivery_longitude = (string)Application.Current.Properties["user_longitude"],
                purchase_notes = "purchase_notes"

            };

            // Delivery Info Data from Purchase Object
            DeliveryAddress1.Text = purchaseObject.delivery_address;
            DeliveryAddress2.Text = purchaseObject.delivery_city + ", " + purchaseObject.delivery_state + ", " + purchaseObject.delivery_zip;

            // Contact Information
            FullName.Text = purchaseObject.delivery_first_name + " " + purchaseObject.delivery_last_name;
            PhoneNumber.Text = purchaseObject.delivery_phone_num;
            EmailAddress.Text = purchaseObject.delivery_email;

            // Expected Delivery Date
            deliveryDate.Text = Application.Current.Properties.ContainsKey("delivery_date") ? (string)Application.Current.Properties["delivery_date"] : "";
            deliveryTime.Text = Application.Current.Properties.ContainsKey("delivery_time") ? (string)Application.Current.Properties["delivery_time"] : "";

            // Cart info
            CartItems.ItemsSource = cartItems;
            CartItems.HeightRequest = 56 * cartItems.Count;

            // Fees
            delivery_fee = Constant.deliveryFee;
            service_fee = Constant.serviceFee;
            ServiceFee.Text = "$ " + service_fee.ToString("N2");
            updateTotals();
        }

        public void InitializeMap()
        {
            map.MapType = MapType.Street;
            Position point = new Position(37.334789, -121.888138);
            var mapSpan = new MapSpan(point, 5, 5);
            map.MoveToRegion(mapSpan);
        }

        public void updateTotals()
        {
            subtotal = 0.0;
            total_qty = 0;
            
            foreach (ItemObject item in cartItems)
            {
                total_qty += item.qty;
                subtotal += (item.qty * item.price);
            }
            SubTotal.Text = "$ " + subtotal.ToString("N2");
            discount = subtotal * .1;
            Discount.Text = "-$ " + discount.ToString("N2");
            DeliveryFee.Text = "$ " + delivery_fee.ToString("N2");
            taxes = subtotal * 0.085;
            Taxes.Text = "$ " + taxes.ToString("N2");
            if(DriverTip.Text == null)
            {
                total = subtotal - discount + delivery_fee + taxes + service_fee; 
            }
            else
            {
                total = subtotal - discount + delivery_fee + taxes + service_fee + Double.Parse(DriverTip.Text);
            }

            GrandTotal.Text = "$ " + total.ToString("N2") ;

            CartTotal.Text = total_qty.ToString();
        }

        public void TestDateFormat(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            Console.WriteLine(now);
            Console.WriteLine(now.Year.ToString("D4") + "-" +
                              now.Month.ToString("D2") + "-" +
                              now.Day.ToString("D2") + " " +
                              now.Hour.ToString("D2") + ":" +
                              now.Minute.ToString("D2") + ":" +
                              now.Second.ToString("D2"));
        }

        public void checkoutAsync(object sender, EventArgs e)
        {

            cardframe.Height = this.Height / 2;

            string dateTime = DateTime.Parse((string)Application.Current.Properties["delivery_date"]).ToString("yyyy-MM-dd");

            purchaseObject.cc_num = "";
            purchaseObject.cc_exp_date = "";
            purchaseObject.cc_cvv = "";
            purchaseObject.cc_zip = "";
            purchaseObject.charge_id = "";
            purchaseObject.payment_type = ((Button)sender).Text == "Checkout with Paypal" ? "PAYPAL" : "STRIPE";
            purchaseObject.items = GetOrder(cartItems);
            purchaseObject.start_delivery_date = dateTime + " " + (string)Application.Current.Properties["delivery_start_time"];
            purchaseObject.pay_coupon_id = "";
            purchaseObject.amount_due = total.ToString("N2");
            purchaseObject.amount_discount = discount.ToString("N2");
            purchaseObject.amount_paid = total.ToString("N2");
            purchaseObject.info_is_Addon = "FALSE";

            var purchaseString = JsonConvert.SerializeObject(purchaseObject);
            System.Diagnostics.Debug.WriteLine(purchaseString);
        }

        public ObservableCollection<PurchasedItem> GetOrder(ObservableCollection<ItemObject> list)
        {
           ObservableCollection<PurchasedItem> purchasedOrder = new ObservableCollection<PurchasedItem>();
            foreach(ItemObject i in list)
            {
                purchasedOrder.Add(new PurchasedItem
                {
                    item_uid = i.item_uid,
                    qty = i.qty.ToString(),
                    name = i.name,
                    price = i.price.ToString(),
                    itm_business_uid = i.business_uid
                }) ;
            }
            return purchasedOrder;
        }
        
        void CompletePaymentClick(System.Object sender, System.EventArgs e)
        {
            cardframe.Height = 0;
            PayViaStripe();
        }

        async public void PayViaStripe()
        {
            try
            {
                StripeConfiguration.ApiKey = Constant.StipeKey;

                string CardNo = cardHolderNumber.Text.Trim();
                string expMonth = cardExpMonth.Text.Trim();
                string expYear = cardExpYear.Text.Trim();
                string cardCvv = cardCVV.Text.Trim();

                // Step 1: Create Card
                TokenCardOptions stripeOption = new TokenCardOptions();
                stripeOption.Number = CardNo;
                stripeOption.ExpMonth = Convert.ToInt64(expMonth);
                stripeOption.ExpYear = Convert.ToInt64(expYear);
                stripeOption.Cvc = cardCvv;

                // Step 2: Assign card to token object
                TokenCreateOptions stripeCard = new TokenCreateOptions();
                stripeCard.Card = stripeOption;

                TokenService service = new TokenService();
                Token newToken = service.Create(stripeCard);

                // Step 3: Assign the token to the soruce 
                var option = new SourceCreateOptions();
                option.Type = SourceType.Card;
                option.Currency = "usd";
                option.Token = newToken.Id;

                var sourceService = new SourceService();
                Source source = sourceService.Create(option);

                // Step 4: Create customer
                CustomerCreateOptions customer = new CustomerCreateOptions();
                customer.Name = cardHolderName.Text.Trim();
                customer.Email = cardHolderEmail.Text.ToLower().Trim();
                customer.Description = cardDescription.Text.Trim();
                if(cardHolderUnit.Text == null)
                {
                    cardHolderUnit.Text = "";
                }
                customer.Address = new AddressOptions { City = cardCity.Text.Trim(), Country = Constant.Contry, Line1 = cardHolderAddress.Text.Trim(), Line2 = cardHolderUnit.Text.Trim(), PostalCode = cardZip.Text.Trim(), State = cardState.Text.Trim() };

                var customerService = new CustomerService();
                var cust = customerService.Create(customer);

                // Step 5: Charge option
                var chargeOption = new ChargeCreateOptions();
                chargeOption.Amount =(long)RemoveDecimalFromTotalAmount(total.ToString("N2"));
                chargeOption.Currency = "usd";
                chargeOption.ReceiptEmail = cardHolderEmail.Text.ToLower().Trim();
                chargeOption.Customer = cust.Id;
                chargeOption.Source = source.Id;
                chargeOption.Description = cardDescription.Text.Trim();

                // Step 6: charge the customer
                var chargeService = new ChargeService();
                Charge charge = chargeService.Create(chargeOption);
                if (charge.Status == "succeeded")
                {
                    // Successful Payment
                    await DisplayAlert("Congratulations", "Payment was succesfull. We appreciate your business", "OK");
                    ClearCardInfo();

                    var purchaseString = JsonConvert.SerializeObject(purchaseObject);
                    System.Diagnostics.Debug.WriteLine(purchaseString);
                    var purchaseMessage = new StringContent(purchaseString, Encoding.UTF8, "application/json");
                    var client = new HttpClient();

                    var RDSResponse = await client.PostAsync(Constant.PurchaseUrl, purchaseMessage);
                    System.Diagnostics.Debug.WriteLine(RDSResponse.IsSuccessStatusCode);
                    if (RDSResponse.IsSuccessStatusCode)
                    {
                        var RDSResponseContent = await RDSResponse.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine(RDSResponseContent);

                        cartItems.Clear();
                        total = 00.00;
                        total_qty = 0;
                    }
                }
                else
                {
                    // Fail
                    await DisplayAlert("Ooops", "Payment was not succesfull. Please try again", "OK");
                }
            }catch(Exception ex)
            {
                 await DisplayAlert("Alert!", ex.Message, "OK");
            }
        }

        void ClearCardInfo()
        {
            cardHolderNumber.Text = "";
            cardExpMonth.Text = "";
            cardExpYear.Text = "";
            cardCVV.Text = "";

            cardCity.Text = "";
            cardState.Text = "";
            cardZip.Text = "";
            cardHolderAddress.Text = "";
            cardHolderUnit.Text = "";
            cardHolderEmail.Text = "";
            cardDescription.Text = "";
            cardHolderName.Text = "";
        }

        public int RemoveDecimalFromTotalAmount(string amount)
        {
            var stringAmount = "";
            var arrayAmount = amount.ToCharArray();
            for(int i = 0; i < arrayAmount.Length; i++)
            {
                if((int)arrayAmount[i] != (int)'.')
                {
                    stringAmount += arrayAmount[i];
                }
            }
            System.Diagnostics.Debug.WriteLine(stringAmount);
            return Int32.Parse(stringAmount);
        }

        void CancelPaymentClick(System.Object sender, System.EventArgs e)
        {
            cardframe.Height = 0;
        }

        public void increase_qty(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            TapGestureRecognizer tgr = (TapGestureRecognizer)l.GestureRecognizers[0];
            ItemObject item = (ItemObject)tgr.CommandParameter;
            if (item != null) item.increase_qty();

            updateTotals();
        }

        public void decrease_qty(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            TapGestureRecognizer tgr = (TapGestureRecognizer)l.GestureRecognizers[0];
            ItemObject item = (ItemObject)tgr.CommandParameter;
            if (item != null) item.decrease_qty();

            updateTotals();
        }

        public void openHistory(object sender, EventArgs e)
        {
            Application.Current.MainPage = new HistoryPage();
        }

        public void ChangeAddressClick(System.Object sender, System.EventArgs e)
        {
            addresframe.Height = this.Height / 2;
        }

        void DriverTip_Completed(System.Object sender, System.EventArgs e)
        {
            DriverTip.Focus();
        }

        void UpdateTotalAmount(System.Object sender, System.EventArgs e)
        {
            updateTotals();
        }

        public void openRefund(object sender, EventArgs e)
        {
            Application.Current.MainPage = new RefundPage();
        }

        void DeliveryDaysClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new SelectionPage();
        }

        void OrdersClick(System.Object sender, System.EventArgs e)
        {
            // Already on orders page
        }

        void InfoClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new InfoPage();
        }

        void ProfileClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new ProfilePage();
        }

        public async void ValidateAddressClick(System.Object sender, System.EventArgs e)
        {
            if (newUserAddress.Text == null)
            {
                await DisplayAlert("Alert!", "Please enter an address", "OK");
            }

            if (newUserUnitNumber.Text == null)
            {
                newUserUnitNumber.Text = "";
            }

            if(newUserCity.Text == null)
            {
                await DisplayAlert("Alert!", "Please enter a city", "OK");
            }

            if(newUserState.Text == null)
            {
                await DisplayAlert("Alert!", "Please enter a state", "OK");
            }

            if(newUserZipcode.Text == null)
            {
                await DisplayAlert("Alert!", "Please enter a zipcode", "OK");
            }

            // Setting request for USPS API
            XDocument requestDoc = new XDocument(
                new XElement("AddressValidateRequest",
                new XAttribute("USERID", "400INFIN1745"),
                new XElement("Revision", "1"),
                new XElement("Address",
                new XAttribute("ID", "0"),
                new XElement("Address1", newUserAddress.Text.Trim()),
                new XElement("Address2", newUserUnitNumber.Text.Trim()),
                new XElement("City", newUserCity.Text.Trim()),
                new XElement("State", newUserState.Text.Trim()),
                new XElement("Zip5", newUserZipcode.Text.Trim()),
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

            foreach (XElement element in xdoc.Descendants("Address"))
            {
                if (GetXMLElement(element, "Error").Equals(""))
                {
                    if (GetXMLElement(element, "DPVConfirmation").Equals("Y") && GetXMLElement(element, "Zip5").Equals(newUserZipcode.Text.Trim()) && GetXMLElement(element, "City").Equals(newUserCity.Text.Trim().ToUpper())) // Best case
                    {
                        // Get longitude and latitide because we can make a deliver here. Move on to next page.
                        // Console.WriteLine("The address you entered is valid and deliverable by USPS. We are going to get its latitude & longitude");
                        //GetAddressLatitudeLongitude();
                        Geocoder geoCoder = new Geocoder();

                        IEnumerable<Position> approximateLocations = await geoCoder.GetPositionsForAddressAsync(newUserAddress.Text.Trim() + "," + newUserCity.Text.Trim() + "," + newUserState.Text.Trim());
                        Position position = approximateLocations.FirstOrDefault();

                        latitude = $"{position.Latitude}";
                        longitude = $"{position.Longitude}";

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
                addressButton.Text = "Address Verified";
                addressButton.BackgroundColor = Color.FromHex("#136D74");
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

        async void SaveAddressClick(System.Object sender, System.EventArgs e)
        {
            addresframe.Height = 0;
            if (isAddressValidated)
            {
                Application.Current.Properties["user_address"] = newUserAddress.Text;
                Application.Current.Properties["user_unit"] = newUserUnitNumber.Text;
                Application.Current.Properties["user_city"] = newUserCity.Text;
                Application.Current.Properties["user_state"] = newUserState.Text;
                Application.Current.Properties["user_zip_code"] = newUserZipcode.Text;
                Application.Current.Properties["user_latitude"] = latitude;
                Application.Current.Properties["user_longitude"] = longitude;

                string address = (string)Application.Current.Properties["user_address"];
                string city = (string)Application.Current.Properties["user_city"];
                string state = (string)Application.Current.Properties["user_state"];
                string zipcode = (string)Application.Current.Properties["user_zip_code"];

                DeliveryAddress1.Text = address;
                DeliveryAddress2.Text = city + ", " + state + ", " + zipcode;

                ResetChangeAddress();
            }
            else
            {
                await DisplayAlert("Oops!", "We weren't able to save your changes","OK");
            }
        }

        public void ResetChangeAddress()
        {
            newUserAddress.Text = "";
            newUserUnitNumber.Text = "";
            newUserCity.Text = "";
            newUserState.Text = "";
            newUserZipcode.Text = "";
            latitude = "0";
            longitude = "0";
        }

        public void ChangeContactInfoClick(System.Object sender, System.EventArgs e)
        {
            contactframe.Height = this.Height / 2;
        }

        void ChangeContactInfoCancelClick(System.Object sender, System.EventArgs e)
        {
            contactframe.Height = 0;
        }

        void ChangeAddressCancelClick(System.Object sender, System.EventArgs e)
        {
            addresframe.Height = 0;
        }

        void SaveChangesClick(System.Object sender, System.EventArgs e)
        {
            contactframe.Height = 0;


            if (newUserFirstName.Text != null)
            {
                Application.Current.Properties["user_first_name"] = newUserFirstName.Text.Trim();
            }

            if (newUserLastName.Text != null)
            {
                Application.Current.Properties["user_last_name"] = newUserLastName.Text.Trim();
            }

            if (newUserPhoneNum.Text != null)
            {
                Application.Current.Properties["user_phone_num"] = newUserPhoneNum.Text.Trim();
            }

            if (newUserEmailAddress.Text != null)
            {
                Application.Current.Properties["user_email"] = newUserEmailAddress.Text.Trim();
            }

            string firstName = (string)Application.Current.Properties["user_first_name"];
            string lastName = (string)Application.Current.Properties["user_last_name"];

            FullName.Text = firstName + " " + lastName;
            PhoneNumber.Text = (string)Application.Current.Properties["user_phone_num"];
            EmailAddress.Text = (string)Application.Current.Properties["user_email"];

            ResetContactInfo();
        }

        public void ResetContactInfo()
        {
            newUserFirstName.Text = "";
            newUserLastName.Text = "";
            newUserPhoneNum.Text = "";
            newUserEmailAddress.Text = "";
        }
    }
}
