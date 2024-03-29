﻿using System;
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
using static ServingFresh.Views.CheckoutPage;
using static ServingFresh.Views.PrincipalPage;

namespace ServingFresh.Models
{
    public class Payments
    {
        private static string mode;
        private string transactionID;
        private string last4;

        public Payments()
        {

        }

        public Payments(string mode)
        {
            transactionID = "";
            Payments.mode = mode;
        }

        public string getTransactionID()
        {
            return transactionID;
        }

        public string getLastFour()
        {
            return last4;
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


        public async Task<string> getMode(string mode, string paymentType)
        {
            string result = "";
            string url = "";
            var credentials = new Credentials();

            if (paymentType == "STRIPE")
            {
                if (mode == "SFTEST")
                {
                    credentials.key = Constant.TestPK;
                }
                else
                {
                    credentials.key = Constant.LivePK;
                }
                url = Constant.StripeMode;
            }
            else if (paymentType == "PAYPAL")
            {
                if (mode == "SFTEST")
                {
                    credentials.key = Constant.TestClientId;
                }
                else
                {
                    credentials.key = Constant.LiveClientId;
                }
                url = Constant.PayPalMode;
            }
            try
            {
                var client = new System.Net.Http.HttpClient();
                var serializeObject = JsonConvert.SerializeObject(credentials);
                Debug.WriteLine("INPUT:" + serializeObject);
                var content = new StringContent(serializeObject, Encoding.UTF8, "application/json");
                var endpointCall = await client.PostAsync(url, content);

                var r = await endpointCall.Content.ReadAsStringAsync();
                Debug.WriteLine("PAYMENT MODE:" + r);

                if (endpointCall.IsSuccessStatusCode)
                {
                    var endpointContent = await endpointCall.Content.ReadAsStringAsync();
                    if (endpointContent.Contains("Test"))
                    {
                        result = "TEST";
                    }
                    else if (endpointContent.Contains("Live"))
                    {
                        result = "LIVE";
                    }
                }
                else
                {
                    result = "LIVE";
                }
            }
            catch(Exception errorPaymentsEndpoint)
            {
                var client2 = new Diagnostic();
                client2.parseException(errorPaymentsEndpoint.ToString(), user);
                result = "LIVE";
            }

            return result;
        }

        public async static Task<HttpResponse> createOrder(string amount)
        {
            HttpResponse response;

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
            try
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
                    environment = new LiveEnvironment(Constant.LiveClientId, Constant.LiveSecret);
                }
                return new PayPalHttpClient(environment);
            }
            catch (Exception wrongMode)
            {
                Debug.WriteLine("USING PAYMENTS IN THE WRONG MODE: " + wrongMode);
                return null;
            }
        }


        // STRIPE PAY METHOD
        private string ReturnStripeApikey(string mode)
        {
            string apiKey = "";
            if (mode == "TEST")
            {
                Debug.WriteLine("STRIPE TEST ENVIROMENT");
                apiKey = Constant.TestSK;
            }
            else
            {
                Debug.WriteLine("STRIPE LIVE ENVIROMENT");
                apiKey = Constant.LiveSK;
            }
            return apiKey;
        }

        public bool PayViaStripe(string customerEmail, string customerName, string cardNumber, string cardCVV, string ExpMonth, string ExpYear, string amount)
        {
            try
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
                chargeOption.Amount = (long)RemoveDecimalFromTotalAmount(amount);
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
            catch(Exception changeMode)
            {
                Debug.WriteLine("USING PAYMENTS IN WRONG MODE: " + changeMode.Message);
                return false;
            }
        }

        public async Task<bool> PayViaStripeCard(string userID, string businessCode, string cardNumber, string cardCVV, string ExpMonth, string ExpYear, string amount)
        {
           
            try
            {
                StripeConfiguration.ApiKey = ReturnStripeApikey(mode);

                var options = new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardOptions
                    {
                        Number = cardNumber.Trim(),
                        ExpMonth = Convert.ToInt64(ExpMonth),
                        ExpYear = Convert.ToInt64(ExpYear),
                        Cvc = cardCVV.Trim(),
                    },

                };

                Debug.WriteLine("reached after options");
                var payMethodService = new PaymentMethodService();
                var tempPayMethod = payMethodService.Create(options);
                var method = tempPayMethod;

                var credentials = await ProcessPaymentIntent(userID, businessCode, amount);
                if (credentials != null)
                {

                    //Debug.WriteLine("CREDENTIALS: " + credentials);
                    Debug.WriteLine("CLIENT SECRET: " + credentials.secrent);
                    Debug.WriteLine("PAYMENT INTENT: " + credentials.intent);
                    Debug.WriteLine("API KEY: " + GetStripeClient(mode));
                    //Debug.WriteLine("API KEY: " + ReturnStripeApikey(mode));

                    var paymentIntentService = new PaymentIntentService(new StripeClient(GetStripeClient(mode)));
                    //var paymentIntentService = new PaymentIntentService(new StripeClient(ReturnStripeApikey(mode)));
                    //Debug.WriteLine("reached after paymentIntentService");
                    // PaymentMethod = method.Id,
                    var paymentConfirmOptions = new PaymentIntentConfirmOptions
                    {
                        ClientSecret = credentials.secrent,
                        Expand = new List<string> { "payment_method" },
                        PaymentMethod = method.Id,
                        UseStripeSdk = true,
                        ReturnUrl = "payments-example://stripe-redirect",
                        SetupFutureUsage = "off_session",
                        
                    };
                    
                    var result = await paymentIntentService.ConfirmAsync(credentials.intent, paymentConfirmOptions);

                    switch (result.NextAction?.Type)
                    {
                        case null:
                            Debug.WriteLine("Success", "Your purchase was successful!");
                            //transactionID = credentials.intent;
                            try
                            {
                                transactionID = credentials.intent;
                                last4 = result.PaymentMethod.Card.Last4;
                                Debug.WriteLine("transactionID: " + transactionID);
                                Debug.WriteLine("LAST 4: " + last4);
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                            
                        //break; // All good

                        case "stripe_3ds2_fingerprint":
                        default:
                            return false;
                            //throw new NotImplementedException($"Not implemented: Can't handle {result.NextAction.Type}");
                    }

                }
                else
                {
                    return false;
                }                
            }
            catch (Exception changeMode)
            {
                Debug.WriteLine("Error: " + changeMode.Message);
                return false;
            }
        }

        public string GetStripeClient(string mode)
        {
            if(mode == "TEST")
            {
                return Constant.TestPK;
            }
            else
            {
                return Constant.LivePK;
            }
        }

        public async Task<IntentSecret> ProcessPaymentIntent(string userID, string businessCode, string amount)
        {
            IntentSecret result = null;

            var paymentIntent = new StripePaymentIntent()
            {
                currency = "uds",
                customer_uid = userID,
                business_code = SetBusinessCode(businessCode.Trim()),
                payment_summary = new PaymentSummary() { total = amount }
            };

            var paymentIntentStr = JsonConvert.SerializeObject(paymentIntent);
            var content = new StringContent(paymentIntentStr, Encoding.UTF8, "application/json");
            var client = new System.Net.Http.HttpClient();
            var endpointResponse = await client.PostAsync(Constant.CreatePaymentIntent, content);

            Debug.WriteLine("JSON TO SEND VIA PURCHASE ENDPOINT: " + paymentIntentStr);
            Debug.WriteLine("PURCHASE WAS WRITTEN TO DATABASE: " + endpointResponse.IsSuccessStatusCode);
            if (endpointResponse.IsSuccessStatusCode)
            {
                var data = await endpointResponse.Content.ReadAsStringAsync();
                string clientSecret = data.Substring(1);
                clientSecret = clientSecret.Substring(0, clientSecret.IndexOf("\""));
                string payIntent = data.Substring(1, data.IndexOf("secret") - 2);

                Debug.WriteLine("clientSecret: " + clientSecret);
                Debug.WriteLine("payIntent: " + payIntent);

                result = new IntentSecret()
                {
                    intent = payIntent,
                    secrent = clientSecret,
                };
            }
            return result;
        }

        public string SetBusinessCode(string code)
        {
            string result = "";

            if(code == "SFTEST")
            {
                result = "SFTEST";
            }
            else
            {
                result = "SF";
            }

            return result;
        }

        public async Task<string> ProcessPaymentIntentOffSession(string userID, string businessCode, string amount)
        {
            string result = "";
            var paymentIntent = new StripePaymentIntent()
            {
                currency = "uds",
                customer_uid = userID,
                business_code = SetBusinessCode(businessCode),
                payment_summary = new PaymentSummary() { total = amount }
            };

            var paymentIntentStr = JsonConvert.SerializeObject(paymentIntent);
            var content = new StringContent(paymentIntentStr, Encoding.UTF8, "application/json");
            var client = new System.Net.Http.HttpClient();
            var endpointResponse = await client.PostAsync(Constant.CreatePaymentIntentOffSession, content);

            Debug.WriteLine("JSON: " + paymentIntentStr);
            Debug.WriteLine("ENDPOINT RESPONSE: " + endpointResponse.IsSuccessStatusCode);
            if (endpointResponse.IsSuccessStatusCode)
            {
                string data = await endpointResponse.Content.ReadAsStringAsync();
                data = data.Replace('"', ' ').Trim();
                Debug.WriteLine("DATA: " + data);
                result = data;
            }
            return result;
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
            try
            {
                var purchaseString = JsonConvert.SerializeObject(purchase);
                var purchaseMessage = new StringContent(purchaseString, Encoding.UTF8, "application/json");
                var client = new System.Net.Http.HttpClient();
                var Response = await client.PostAsync(Constant.PurchaseUrl, purchaseMessage);

                Debug.WriteLine("JSON : " + purchaseString);
                Debug.WriteLine("RESPONSE: " + Response.IsSuccessStatusCode);
                if (Response.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
            catch (Exception issueSendingPurchase)
            {
                var client2 = new Diagnostic();
                client2.parseException(issueSendingPurchase.ToString(), user);
                return false;
            }
        }
    }
}
