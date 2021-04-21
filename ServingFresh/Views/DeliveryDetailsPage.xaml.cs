using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using ServingFresh.Config;
using ServingFresh.LogIn.Classes;
using Stripe;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using static ServingFresh.Views.CheckoutPage;
using static ServingFresh.Views.SelectionPage;
using static ServingFresh.Views.SignUpPage;
using Application = Xamarin.Forms.Application;

namespace ServingFresh.Views
{
    public partial class DeliveryDetailsPage : ContentPage
    {
        // Temporary variable
        string userID;

        string deliveryInstructions;
        static string mode;
        static string clientId;
        static string secret;
        string orderId;
        PurchaseDataObject purchase;
        ScheduleInfo deliveryInfo;

        public DeliveryDetailsPage(PurchaseDataObject purchase, ScheduleInfo deliveryInfo)
        {
            InitializeComponent();
            this.purchase = purchase;
            this.deliveryInfo = deliveryInfo;
            mode = "";
            clientId = "";
            secret = "";
            orderId = "";
            cartItemsNumber.Text = purchase.items.Count.ToString();
            PlaceLocationOnMap(double.Parse(purchase.delivery_latitude), double.Parse(purchase.delivery_longitude));
            GetPayPalCredentials(purchase.delivery_instructions);
        }

        void PlaceLocationOnMap(double latitude, double longitude)
        {
            Position position = new Position(latitude, longitude);
            map.MapType = MapType.Street;
            var mapSpan = new MapSpan(position, 0.001, 0.001);
            Pin address = new Pin();
            address.Label = "Delivery Address";
            address.Type = PinType.SearchResult;
            address.Position = position;
            map.MoveToRegion(mapSpan);
            map.Pins.Add(address);
        }

        void CheckoutWithStripe(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;

            if (button.BackgroundColor == Color.FromHex("#FF8500"))
            {
                button.BackgroundColor = Color.FromHex("#2B6D74");
                stripeInformationView.HeightRequest = 194;

                purchase.delivery_first_name = firstName.Text;
                purchase.delivery_last_name = lastName.Text;
                purchase.delivery_phone_num = phoneNumber.Text;
                purchase.delivery_email = emailAddress.Text.ToLower();

            }
            else
            {
                button.BackgroundColor = Color.FromHex("#FF8500");
                stripeInformationView.HeightRequest = 0;
            }
        }

        public int RemoveDecimalFromTotalAmount(string amount)
        {
            var stringAmount = "";
            var arrayAmount = amount.ToCharArray();
            for (int i = 0; i < arrayAmount.Length; i++)
            {
                if ((int)arrayAmount[i] != (int)'.')
                {
                    stringAmount += arrayAmount[i];
                }
            }
            Debug.WriteLine("STRIPE AMOUNT TO CHARGE: " + stringAmount);
            return Int32.Parse(stringAmount);
        }

        async void CompletePaymentWithStripe(System.Object sender, System.EventArgs e)
        {
            var button = (Button)sender;

            if (button.BackgroundColor == Color.FromHex("#FF8500"))
            {
                button.BackgroundColor = Color.FromHex("#2B6D74");
                purchase.payment_type = "STRIPE";

                var userID = await SignUpNewUser(GetUserFrom(purchase));
                if( userID != "") { purchase.pur_customer_uid = userID; }
                var paymentIsSuccessful = await PayViaStripe();
                await WriteFavorites(GetFavoritesList(),userID);

                if (paymentIsSuccessful)
                {
                    _ = SendPurchaseToDatabase(purchase);
                    Application.Current.MainPage = new ConfirmationPage(purchase, deliveryInfo);
                }
                else
                {
                    await DisplayAlert("Issue with payment via stripe", "", "OK");
                }
            }
            else
            {
                button.BackgroundColor = Color.FromHex("#FF8500");
            }
        }

        string ReturnStripeApikey(string mode)
        {
            var apiKey = "";
            if(mode != "SFTEST")
            {
                apiKey = Constant.LivePK;
            }
            else
            {
                apiKey = Constant.TestSK;
            }
            return apiKey;
        }

        public async Task<bool> PayViaStripe()
        {
            try
            {
                if (purchase.delivery_instructions != null)
                {
                        StripeConfiguration.ApiKey = ReturnStripeApikey(purchase.delivery_instructions.Trim());
                        string CardNo = cardHolderNumber.Text.Trim();
                        string expMonth = cardExpMonth.Text.Trim();
                        string expYear = cardExpYear.Text.Trim();
                        string cardCvv = cardCVV.Text.Trim();

                        // STORE INFO
                        //if (!(bool)Application.Current.Properties["guest"])
                        //{
                        //    Application.Current.Properties["CardNumber"] = CardNo;
                        //    Application.Current.Properties["CardExpMonth"] = expMonth;
                        //    Application.Current.Properties["CardExpYear"] = expYear;
                        //    Application.Current.Properties["CardCVV"] = cardCvv;
                        //}

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
                        Stripe.Token newToken = service.Create(stripeCard);

                        // Step 3: Assign the token to the soruce 
                        SourceCreateOptions option = new SourceCreateOptions();

                        option.Type = SourceType.Card; 
                        option.Currency = "usd";
                        option.Token = newToken.Id;

                        SourceService sourceService = new SourceService();
                        Source source = sourceService.Create(option);

                        // Step 4: Create customer
                        CustomerCreateOptions customer = new CustomerCreateOptions();
                        customer.Name = cardHolderName.Text.Trim();
                        //customer.Email = cardHolderEmail.Text.ToLower().Trim();
                        //customer.Description = cardDescription.Text.Trim();
                        //if (cardHolderUnit.Text == null)
                        //{
                        //    cardHolderUnit.Text = "";
                        //}
                        //customer.Address = new AddressOptions { City = cardCity.Text.Trim(), Country = Constant.Contry, Line1 = cardHolderAddress.Text.Trim(), Line2 = cardHolderUnit.Text.Trim(), PostalCode = cardZip.Text.Trim(), State = cardState.Text.Trim() };

                        CustomerService customerService = new CustomerService();
                        Customer cust = customerService.Create(customer);

                        // Step 5: Charge option
                        ChargeCreateOptions chargeOption = new ChargeCreateOptions();
                        chargeOption.Amount = (long)RemoveDecimalFromTotalAmount(purchase.amount_due);
                        chargeOption.Currency = "usd";
                        chargeOption.ReceiptEmail = emailAddress.Text.Trim();
                        chargeOption.Customer = cust.Id;
                        chargeOption.Source = source.Id;
                        chargeOption.Description = "";

                        // Step 6: charge the customer
                        ChargeService chargeService = new ChargeService();
                        Charge charge = chargeService.Create(chargeOption);

                        if (charge.Status == "succeeded")
                        {
                            return true;
                        }
                        else
                        {
                            await DisplayAlert("Oops", "Payment was not successful. Please try again", "OK");
                            return false;
                        }
                }
                return false;

            }
            catch (Exception paymentIssueWithStripe)
            {
                await DisplayAlert("Alert!", paymentIssueWithStripe.Message, "OK");
                return false;
            }
        }

        async void CheckoutWithPayPal(System.Object sender, System.EventArgs e)
        {
            paypalRow.Height = this.Height - 100;
            purchase.payment_type = "PAYPAL";
            purchase.delivery_first_name = firstName.Text;
            purchase.delivery_last_name = lastName.Text;
            purchase.delivery_phone_num = phoneNumber.Text;
            purchase.delivery_email = emailAddress.Text.ToLower();

            var response = await createOrder(purchase.amount_due);
            var content = response.Result<PayPalCheckoutSdk.Orders.Order>();
            var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

            Console.WriteLine("Status: {0}", result.Status);
            Console.WriteLine("Order Id: {0}", result.Id);
            Console.WriteLine("Intent: {0}", result.CheckoutPaymentIntent);
            Console.WriteLine("Links:");

            foreach (LinkDescription link in result.Links)
            {
                Console.WriteLine("\t{0}: {1}\tCall Type: {2}", link.Rel, link.Href, link.Method);
                if (link.Rel == "approve")
                {
                    webView.Source = link.Href;
                    //Debug.WriteLine("PAYPAL URL: "+ link.Href);
                    //webViewPage.Source = "https://servingfresh.me";
                }
            }

            webView.Navigated += WebViewPage_Navigated;
            SetPayPalOrderId(result.Id);
        }

        string GetPayPalKey(string mode)
        {
            if (mode != "SFTEST")
            {
                return Constant.LiveClientId;
            }
            else
            {
                return Constant.TestClientId;
            }
        }

        public async static Task<HttpResponse> createOrder(string amount)
        {
            HttpResponse response;
            // Construct a request object and set desired parameters
            // Here, OrdersCreateRequest() creates a POST request to /v2/checkout/orders

            var order = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>()
                    {
                        new PurchaseUnitRequest()
                        {
                            AmountWithBreakdown = new AmountWithBreakdown()
                            {
                                CurrencyCode = "USD",
                                Value = amount
                            }
                        }
                    },
                ApplicationContext = new ApplicationContext()
                {
                    ReturnUrl = "https://servingfresh.me",
                    CancelUrl = "https://servingfresh.me"
                }
            };

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(order);
            response = await client().Execute(request);
            return response;
        }

        void SetPayPalOrderId(string orderId)
        {
            this.orderId = orderId;
        }

        string GetPayPalOrderId()
        {
            return orderId;
        }

        void SetUserID(string userID)
        {
            this.userID = userID;
        }

        string GetUserID()
        {
            return userID;
        }

        private async void WebViewPage_Navigated(object sender, WebNavigatedEventArgs e)
        {
            var source = webView.Source as UrlWebViewSource;
            Debug.WriteLine("WEBVIEW SOURCE: " + source.Url);
            if (source.Url.Contains("https://servingfresh.me/"))
            {
                paypalRow.Height = 0;
                Debug.WriteLine("SUCCESSFULL REDIRECT FROM PAYPAL TO SF WEB TO MOBILE APP");
                var userID = await SignUpNewUser(GetUserFrom(purchase));
                if (userID != "") { purchase.pur_customer_uid = userID; }
                var paymentIsSuccessful = await captureOrder(GetPayPalOrderId());
                await WriteFavorites(GetFavoritesList(), userID);
                if (paymentIsSuccessful)
                {
                    _ = SendPurchaseToDatabase(purchase);
                    Application.Current.MainPage = new ConfirmationPage(purchase, deliveryInfo);
                }
                else
                {
                    await DisplayAlert("Issue with payment via PayPal", "", "OK");
                }
            }
        }

        public async Task<bool> captureOrder(string id)
        {
            // Construct a request object and set desired parameters
            // Replace ORDER-ID with the approved order id from create order
            // UserDialogs.Instance.Loading("Processing Payment...");
            Debug.WriteLine("id: " + id);
            var request = new OrdersCaptureRequest(id);
            request.RequestBody(new OrderActionRequest());

            HttpResponse response = await client().Execute(request);
            var statusCode = response.StatusCode;
            string code = statusCode.ToString();
            Debug.WriteLine("StatusCode: " + code);
            PayPalCheckoutSdk.Orders.Order result = response.Result<PayPalCheckoutSdk.Orders.Order>();
            Debug.WriteLine("Status: " + result.Status);
            Debug.WriteLine("Capture Id: " + result.Id);
            Debug.WriteLine("id: " + id);

            if(result.Status == "COMPLETED")
            {
                return true;
            }

            return false;
        }

        public async Task<bool> SendPurchaseToDatabase(PurchaseDataObject purchase)
        {
            var purchaseString = JsonConvert.SerializeObject(purchase);
            var purchaseMessage = new StringContent(purchaseString, Encoding.UTF8, "application/json");
            var client = new System.Net.Http.HttpClient();
            var Response = await client.PostAsync(Constant.PurchaseUrl, purchaseMessage);

            Debug.WriteLine("JSON TO SEND VIA PURCHASE ENDPOINT: " + purchaseString);
            Debug.WriteLine("PURCHASE WAS WRITTEN TO DATABASE: " + Response.IsSuccessStatusCode);
            if (Response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async void GetPayPalCredentials(string clientMode)
        {
            var c = new System.Net.Http.HttpClient();
            var paypal = new Credentials();

            paypal.key = GetPayPalKey(clientMode);

            var stripeObj = JsonConvert.SerializeObject(paypal);
            Debug.WriteLine("key to send JSON: " + stripeObj);
            var stripeContent = new StringContent(stripeObj, Encoding.UTF8, "application/json");
            var RDSResponse = await c.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/Paypal_Payment_key_checker", stripeContent);
            var content = await RDSResponse.Content.ReadAsStringAsync();
            Debug.WriteLine("Response key from paypal :" + content);

            if (RDSResponse.IsSuccessStatusCode)
            {
                if (content.Contains("Test"))
                {
                    mode = "TEST";
                    clientId = Constant.TestClientId;
                    secret = Constant.TestSecret;
                }
                else if (content.Contains("Live"))
                {
                    mode = "LIVE";
                    clientId = Constant.LiveClientId;
                    secret = Constant.LiveSecret;
                }
                else
                {
                    Debug.WriteLine("INVALID ENTRY");
                }
                Debug.WriteLine("MODE            : " + mode);
                Debug.WriteLine("PAYPAL CLIENT ID: " + clientId);
                Debug.WriteLine("PAYPAL SECRET   : " + secret);
            }
            else
            {
                await DisplayAlert("Oops", "We can't process your request at this moment.", "OK");
            }
        }

        public static PayPalHttp.HttpClient client()
        {
            
            if (mode == "TEST")
            {
                Debug.WriteLine("PAYPAL TEST ENVIROMENT");
                PayPalEnvironment environment = new SandboxEnvironment(clientId, secret);
                PayPalHttpClient payPalClient = new PayPalHttpClient(environment);
                return payPalClient;
            }
            else if (mode == "LIVE")
            {
                Debug.WriteLine("PAYPAL LIVE ENVIROMENT");
                PayPalEnvironment environment = new LiveEnvironment(clientId, secret);
                PayPalHttpClient payPalClient = new PayPalHttpClient(environment);
                return payPalClient;
            }
            return null;
        }

        void NavigateToCartFromDeliveryDetails(System.Object sender, System.EventArgs e)
        {
            NavigateToCart(sender, e);
        }
    }
}
