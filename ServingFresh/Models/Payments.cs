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
using Stripe;

namespace ServingFresh.Models
{
    public class Payments
    {
        private static string mode;
        private string transactionID;

        public Payments(string mode)
        {
            transactionID = "";
            Payments.mode = mode;
        }

        public string getTransactionID()
        {
            return transactionID;
        }

        public async Task<string> PayViaPayPal(string amount)
        {
            var checkoutLink = "";
            var response = await createOrder(amount);
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
                    checkoutLink = link.Href;
                    transactionID = result.Id;
                    break;
                }
            }
            return checkoutLink;
        }

        public async Task<bool> captureOrder(string ID)
        {
            // Construct a request object and set desired parameters
            // Replace ORDER-ID with the approved order id from create order
            // UserDialogs.Instance.Loading("Processing Payment...");
            Debug.WriteLine("id: " + ID);
            var request = new OrdersCaptureRequest(ID);
            request.RequestBody(new OrderActionRequest());

            HttpResponse response = await client().Execute(request);
            var statusCode = response.StatusCode;
            string code = statusCode.ToString();
            Debug.WriteLine("StatusCode: " + code);
            PayPalCheckoutSdk.Orders.Order result = response.Result<PayPalCheckoutSdk.Orders.Order>();
            Debug.WriteLine("Status: " + result.Status);
            Debug.WriteLine("Capture Id: " + result.Id);
            Debug.WriteLine("id: " + ID);

            if (result.Status == "COMPLETED")
            {
                return true;
            }

            return false;
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

        public static PayPalHttp.HttpClient client()
        {
            PayPalEnvironment environment = null;
            if (mode == "TEST")
            {
                Debug.WriteLine("PAYPAL TEST ENVIROMENT");
                environment = new SandboxEnvironment(Constant.TestClientId, Constant.TestSecret);
            }
            else if (mode == "LIVE")
            {
                Debug.WriteLine("PAYPAL LIVE ENVIROMENT");
                environment = new LiveEnvironment(Constant.TestClientId, Constant.TestSecret);
            }
            return new PayPalHttpClient(environment); ;
        }


        // STRIPE PAY METHOD
        private string ReturnStripeApikey(string mode)
        {
            string apiKey = "";
            if (mode == "TEST")
            {
                apiKey = Constant.TestSK;
            }
            else
            {
                apiKey = Constant.LivePK;
            }
            return apiKey;
        }

        public bool PayViaStripe(string customerEmail, string customerName, string cardNumber, string cardCVV, string ExpMonth, string ExpYear, string amount)
        {

            StripeConfiguration.ApiKey = ReturnStripeApikey(mode);

            // Step 1: Create Card 
            TokenCardOptions stripeOption = new TokenCardOptions();
            stripeOption.Number = cardNumber;
            stripeOption.ExpMonth = Convert.ToInt64(ExpMonth);
            stripeOption.ExpYear = Convert.ToInt64(ExpYear);
            stripeOption.Cvc = cardCVV;

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
            customer.Name = customerName;
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
            chargeOption.Amount = (long) RemoveDecimalFromTotalAmount(amount);
            chargeOption.Currency = "usd";
            chargeOption.ReceiptEmail = customerEmail;
            chargeOption.Customer = cust.Id;
            chargeOption.Source = source.Id;
            chargeOption.Description = "";

            // Step 6: charge the customer
            ChargeService chargeService = new ChargeService();
            Charge charge = chargeService.Create(chargeOption);

            if (charge.Status == "succeeded")
            {
                transactionID = charge.Id;
                return true;
            }
            else
            {
                return false;
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

        public async Task<bool> SendPurchaseToDatabase(Purchase purchase)
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
    }
}
