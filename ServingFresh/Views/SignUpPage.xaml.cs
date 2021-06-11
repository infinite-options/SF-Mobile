using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using ServingFresh.Config;
using ServingFresh.LogIn.Classes;
using ServingFresh.Models;
using ServingFresh.Notifications;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Diagnostics;
using System.Threading.Tasks;
using static ServingFresh.Views.PrincipalPage;
using Xamarin.Auth;
using ServingFresh.LogIn.Apple;
using System.Windows.Input;
using static ServingFresh.App;

namespace ServingFresh.Views
{
    public partial class SignUpPage : ContentPage
    {
        //public readonly static Models.User user = new Models.User();
        public string direction = "";
        public SignUpPost directSignUp = new SignUpPost();
        public bool isAddessValidated = false;
        INotifications appleNotification = DependencyService.Get<INotifications>();
        private string deviceId = null;
        IAppleSignInService appleSignInService = null;
        public ICommand SignInWithAppleCommand { get; set; }

        public class EmailVerificationObject
        {
            public string email { get; set; }
        }


        public SignUpPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("AB000000");
            AutoFillEntries(user);
            //InitializeSignUpPost();
            //InitializeMap();

            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    deviceId = Preferences.Get("guid", null);
            //    if(deviceId != null) { Debug.WriteLine("This is the iOS GUID from Direct Sign Up: " + deviceId); }
            //}
            //else
            //{
            //    deviceId = Preferences.Get("guid", null);
            //    if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Direct Sign Up " + deviceId); }
            //}

            //if(deviceId != null)
            //{
            //    localNotificationButton.IsToggled = true;
            //}
            //else
            //{
            //    localNotificationButton.IsToggled = false;
            //}

        }




        public SignUpPage(double height)
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("AB000000");
            signUpFrame.Margin = new Thickness(0, height, 0, 0);
            AutoFillEntries(user);
            //InitializeSignUpPost();
            //InitializeMap();

            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    deviceId = Preferences.Get("guid", null);
            //    if(deviceId != null) { Debug.WriteLine("This is the iOS GUID from Direct Sign Up: " + deviceId); }
            //}
            //else
            //{
            //    deviceId = Preferences.Get("guid", null);
            //    if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Direct Sign Up " + deviceId); }
            //}

            //if(deviceId != null)
            //{
            //    localNotificationButton.IsToggled = true;
            //}
            //else
            //{
            //    localNotificationButton.IsToggled = false;
            //}

        }



        public void AutoFillEntries(Models.User user)
        {
            newUserFirstName.Text = user.getUserFirstName();
            newUserLastName.Text = user.getUserLastName();
        }


        async void SignUpUserDirect(System.Object sender, System.EventArgs e)
        {

            // STEP 1: CHECK THAT ALL ENTRIES ARE FILLED
            // STEP 2: CHECK THAT EMAIL1 AND EMAIL2 ARE THE SAME
            // STEP 3: CHECK THAT PASSWORD1 AND PASSWORD2 ARE THE SAME
            // STEP 4: CHECK IF EMAIL EXISTS IF SO UPDATE
            // STEP 5: ELSE SIGN THE USER DIRECT WAY
            try
            {
                var signUpClient = new SignUp();
                var signInClient = new SignIn();

                if (signUpClient.ValidateSignUpInfo(newUserFirstName, newUserLastName, newUserEmail1, newUserEmail2, newUserPassword1, newUserPassword2))
                {

                    if (signUpClient.ValidateEmail(newUserEmail1, newUserEmail2))
                    {
                        if (signUpClient.ValidatePassword(newUserPassword1, newUserPassword2))
                        {

                            user.setUserEmail(newUserEmail1.Text);
                            user.setUserFirstName(newUserFirstName.Text);
                            user.setUserLastName(newUserLastName.Text);

                            var profile = await signInClient.ValidateExistingAccountFromEmail(user.getUserEmail());

                            if (profile != null)
                            {
                                if (profile.result.Count != 0)
                                {
                                    if (profile.result[0].role == "GUEST")
                                    {
                                        user.setUserID(profile.result[0].customer_uid);
                                        var content = signUpClient.UpdateDirectUser(user, newUserPassword1.Text);
                                        var signUpStatus = await SignUp.SignUpNewUser(content);

                                        if (signUpStatus)
                                        {
                                            user.setUserPlatform("DIRECT");
                                            user.setUserType("CUSTOMER");
                                            user.printUser();
                                            if (messageList != null)
                                            {
                                                if (messageList.ContainsKey("701-000102"))
                                                {
                                                    await DisplayAlert(messageList["701-000102"].title, messageList["701-000102"].message, messageList["701-000102"].responses);
                                                }
                                                else
                                                {
                                                    await DisplayAlert("Sign Up", "Confirmation email sent. Please check and click the link included.", "Okay, continue");
                                                }
                                            }
                                            else
                                            {
                                                await DisplayAlert("Sign Up", "Confirmation email sent. Please check and click the link included.", "Okay, continue");
                                            }
                                            
                                            if (direction == "")
                                            {
                                                Application.Current.MainPage = new SelectionPage();
                                            }
                                            else if (direction != "")
                                            {
                                                Dictionary<string, Page> array = new Dictionary<string, Page>();
                                                array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                                                array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                                                array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                                                array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                                                array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                                                array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                                                array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                                                var root = Application.Current.MainPage;
                                                Debug.WriteLine("ROOT VALUE: " + root);
                                                Application.Current.MainPage = array[root.ToString()];
                                            }
                                            user.printUser();
                                        }
                                        else
                                        {
                                            if (messageList != null)
                                            {
                                                if (messageList.ContainsKey("701-000103"))
                                                {
                                                    await DisplayAlert(messageList["701-000103"].title, messageList["701-000103"].message, messageList["701-000103"].responses);
                                                }
                                                else
                                                {
                                                    await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                                }
                                            }
                                            else
                                            {
                                                await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                            }
                                           
                                        }
                                    }
                                }
                            }
                            else
                            {

                                var content = signUpClient.SetDirectUser(user, newUserPassword1.Text);
                                var signUpStatus = await SignUp.SignUpNewUser(content);

                                if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                                {
                                    user.setUserID(signUpStatus);
                                    user.setUserPlatform("DIRECT");
                                    user.setUserType("CUSTOMER");
                                    user.printUser();
                                    signUpClient.WriteDeviceID(user);
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000104"))
                                        {
                                            await DisplayAlert(messageList["701-000104"].title, messageList["701-000104"].message, messageList["701-000104"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("Sign Up", "Confirmation email sent. Please check and click the link included.", "Okay, continue");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Sign Up", "Confirmation email sent. Please check and click the link included.", "Okay, continue");
                                    }
                                    

                                    if (direction == "")
                                    {
                                        Application.Current.MainPage = new SelectionPage();
                                    }
                                    else if (direction != "")
                                    {
                                        Dictionary<string, Page> array = new Dictionary<string, Page>();
                                        array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                                        array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                                        array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                                        array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                                        array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                                        array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                                        array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                                        var root = Application.Current.MainPage;
                                        Debug.WriteLine("ROOT VALUE: " + root);
                                        Application.Current.MainPage = array[root.ToString()];
                                    }

                                }
                                else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                                {
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000105"))
                                        {
                                            await DisplayAlert(messageList["701-000105"].title, messageList["701-000105"].message, messageList["701-000105"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                    }
                                    
                                }
                            }
                        }
                        else
                        {
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000106"))
                                {
                                    await DisplayAlert(messageList["701-000106"].title, messageList["701-000106"].message, messageList["701-000106"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Oops", "Please check that your password is the same in both entries", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Oops", "Please check that your password is the same in both entries", "OK");
                            }
                           
                            return;
                        }
                    }
                    else
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000107"))
                            {
                                await DisplayAlert(messageList["701-000107"].title, messageList["701-000107"].message, messageList["701-000107"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Oops", "Please check that your email is the same in both entries", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Oops", "Please check that your email is the same in both entries", "OK");
                        }
                        
                        return;
                    }

                }
                else
                {
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000108"))
                        {
                            await DisplayAlert(messageList["701-000108"].title, messageList["701-000108"].message, messageList["701-000108"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                    }
                    
                    return;
                }
            }
            catch (Exception errorSignUpUserDirect)
            {
                var client = new Diagnostic();
                client.parseException(errorSignUpUserDirect.ToString(), user);
            }
        }

        void ContinueWithFacebook(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
            var client = new SignIn();
            var authenticator = client.GetFacebookAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            authenticator.Completed += FacebookAuthetication;
            authenticator.Error += Authenticator_Error;
            presenter.Login(authenticator);
        }

        void ContinueWithGoogle(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
            var client = new SignIn();
            var authenticator = client.GetGoogleAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            AuthenticationState.Authenticator = authenticator;
            authenticator.Completed += GoogleAuthetication;
            authenticator.Error += Authenticator_Error;
            presenter.Login(authenticator);
        }

        private async void FacebookAuthetication(object sender, Xamarin.Auth.AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthetication;
                authenticator.Error -= Authenticator_Error;
            }

            if (e.IsAuthenticated)
            {

                try
                {
                    var clientLogIn = new SignIn();
                    var clientSignUp = new SignUp();
                    var facebookUser = clientLogIn.GetFacebookUser(e.Account.Properties["access_token"]);

                    user.setUserEmail(facebookUser.email);
                    user.setUserFirstName(newUserFirstName.Text);
                    user.setUserLastName(newUserLastName.Text);
                    
                    var profile = await clientLogIn.ValidateExistingAccountFromEmail(facebookUser.email);

                    if (profile != null)
                    {
                        if (profile.result.Count != 0)
                        {
                            if (profile.result[0].role == "GUEST")
                            {
                                user.setUserID(profile.result[0].customer_uid);
                                var content = clientSignUp.UpdateSocialUser(user, e.Account.Properties["access_token"], "", facebookUser.id, "FACEBOOK");
                                var signUpStatus = await SignUp.SignUpNewUser(content);

                                if (signUpStatus)
                                {
                                    user.setUserPlatform("FACEBOOK");
                                    user.setUserType("CUSTOMER");
                                    user.printUser();
                                    if (direction == "")
                                    {
                                        Application.Current.MainPage = new SelectionPage();
                                    }
                                    else if (direction != "")
                                    {
                                        Dictionary<string, Page> array = new Dictionary<string, Page>();
                                        array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                                        array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                                        array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                                        array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                                        array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                                        array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                                        array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                                        var root = Application.Current.MainPage;
                                        Debug.WriteLine("ROOT VALUE: " + root);
                                        Application.Current.MainPage = array[root.ToString()];
                                    }
                                }
                                else
                                {
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000109"))
                                        {
                                            await DisplayAlert(messageList["701-000109"].title, messageList["701-000109"].message, messageList["701-000109"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                    }
                                    
                                }
                            }
                        }
                        else
                        {
                            
                            var content = clientSignUp.SignUpSocialUser(user, e.Account.Properties["access_token"], "", facebookUser.id, facebookUser.email, "FACEBOOK");
                            var signUpStatus = await SignUp.SignUpNewUser(content);

                            if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                            {
                                user.setUserID(signUpStatus);
                                user.setUserPlatform("FACEBOOK");
                                user.setUserType("CUSTOMER");
                                user.printUser();
                                clientSignUp.WriteDeviceID(user);
                                if (direction == "")
                                {
                                    Application.Current.MainPage = new SelectionPage();
                                }
                                else if (direction != "")
                                {
                                    Dictionary<string, Page> array = new Dictionary<string, Page>();
                                    array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                                    array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                                    array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                                    array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                                    array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                                    array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                                    array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                                    var root = Application.Current.MainPage;
                                    Debug.WriteLine("ROOT VALUE: " + root);
                                    Application.Current.MainPage = array[root.ToString()];
                                }
                            }
                            else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                            {
                                if (messageList != null)
                                {
                                    if (messageList.ContainsKey("701-000110"))
                                    {
                                        await DisplayAlert(messageList["701-000110"].title, messageList["701-000110"].message, messageList["701-000110"].responses);
                                    }
                                    else
                                    {
                                        await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        user.setUserDeviceID(Preferences.Get("guid", ""));

                        var content = clientSignUp.SignUpSocialUser(user, e.Account.Properties["access_token"], "", facebookUser.id, facebookUser.email, "FACEBOOK");
                        var signUpStatus = await SignUp.SignUpNewUser(content);

                        if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                        {
                            user.setUserID(signUpStatus);
                            user.setUserPlatform("FACEBOOK");
                            user.setUserType("CUSTOMER");
                            user.printUser();
                            clientSignUp.WriteDeviceID(user);
                            if (direction == "")
                            {
                                Application.Current.MainPage = new SelectionPage();
                            }
                            else if (direction != "")
                            {
                                Dictionary<string, Page> array = new Dictionary<string, Page>();
                                array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                                array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                                array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                                array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                                array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                                array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                                array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                                var root = Application.Current.MainPage;
                                Debug.WriteLine("ROOT VALUE: " + root);
                                Application.Current.MainPage = array[root.ToString()];
                            }
                        }
                        else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                        {
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000111"))
                                {
                                    await DisplayAlert(messageList["701-000111"].title, messageList["701-000111"].message, messageList["701-000111"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                            }
                        }
                    }
                }
                catch (Exception errorFacebookAuthetication)
                {
                    var client = new Diagnostic();
                    client.parseException(errorFacebookAuthetication.ToString(), user);
                }
            }
        }

        private async void GoogleAuthetication(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthetication;
                authenticator.Error -= Authenticator_Error;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    var clientLogIn = new SignIn();
                    var clientSignUp = new SignUp();
                    var googleUser = await clientLogIn.GetGoogleUser(e);

                    user.setUserEmail(googleUser.email);
                    user.setUserFirstName(newUserFirstName.Text);
                    user.setUserLastName(newUserLastName.Text);
                    user.setUserImage(googleUser.picture);
                    var profile = await clientLogIn.ValidateExistingAccountFromEmail(googleUser.email);

                    if (profile != null)
                    {
                        if (profile.result.Count != 0)
                        {
                            if (profile.result[0].role == "GUEST")
                            {
                                user.setUserID(profile.result[0].customer_uid);
                                var content = clientSignUp.UpdateSocialUser(user, e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], googleUser.id, "GOOGLE");
                                var signUpStatus = await SignUp.SignUpNewUser(content);

                                if (signUpStatus)
                                {
                                    user.setUserPlatform("GOOGLE");
                                    user.setUserType("CUSTOMER");
                                    user.printUser();
                                    if (direction == "")
                                    {
                                        Application.Current.MainPage = new SelectionPage();
                                    }
                                    else if (direction != "")
                                    {
                                        Dictionary<string, Page> array = new Dictionary<string, Page>();
                                        array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                                        array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                                        array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                                        array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                                        array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                                        array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                                        array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                                        var root = Application.Current.MainPage;
                                        Debug.WriteLine("ROOT VALUE: " + root);
                                        Application.Current.MainPage = array[root.ToString()];
                                    }
                                }
                                else
                                {
                                    if (messageList != null)
                                    {
                                        if (messageList.ContainsKey("701-000112"))
                                        {
                                            await DisplayAlert(messageList["701-000112"].title, messageList["701-000112"].message, messageList["701-000112"].responses);
                                        }
                                        else
                                        {
                                            await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                    }
                                }
                            }
                        }
                        else
                        {
                            user.setUserDeviceID(Preferences.Get("guid", ""));
                            var content = clientSignUp.SignUpSocialUser(user, e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], googleUser.id, googleUser.email, "GOOGLE");
                            var signUpStatus = await SignUp.SignUpNewUser(content);

                            if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                            {
                                user.setUserID(signUpStatus);
                                user.setUserPlatform("GOOGLE");
                                user.setUserType("CUSTOMER");
                                user.printUser();
                                clientSignUp.WriteDeviceID(user);
                                if (direction == "")
                                {
                                    Application.Current.MainPage = new SelectionPage();
                                }
                                else if (direction != "")
                                {
                                    Dictionary<string, Page> array = new Dictionary<string, Page>();
                                    array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                                    array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                                    array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                                    array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                                    array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                                    array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                                    array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                                    var root = Application.Current.MainPage;
                                    Debug.WriteLine("ROOT VALUE: " + root);
                                    Application.Current.MainPage = array[root.ToString()];
                                }
                            }
                            else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                            {
                                if (messageList != null)
                                {
                                    if (messageList.ContainsKey("701-000113"))
                                    {
                                        await DisplayAlert(messageList["701-000113"].title, messageList["701-000113"].message, messageList["701-000113"].responses);
                                    }
                                    else
                                    {
                                        await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                }
                            }
                        }
                    }
                    else
                    {
                        user.setUserDeviceID(Preferences.Get("guid", ""));
                        var content = clientSignUp.SignUpSocialUser(user, e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], googleUser.id, googleUser.email, "GOOGLE");
                        var signUpStatus = await SignUp.SignUpNewUser(content);

                        if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                        {
                            user.setUserID(signUpStatus);
                            user.setUserPlatform("GOOGLE");
                            user.setUserType("CUSTOMER");
                            user.printUser();
                            clientSignUp.WriteDeviceID(user);
                            if (direction == "")
                            {
                                Application.Current.MainPage = new SelectionPage();
                            }
                            else if (direction != "")
                            {
                                Dictionary<string, Page> array = new Dictionary<string, Page>();
                                array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                                array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                                array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                                array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                                array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                                array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                                array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                                var root = Application.Current.MainPage;
                                Debug.WriteLine("ROOT VALUE: " + root);
                                Application.Current.MainPage = array[root.ToString()];
                            }
                        }
                        else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                        {
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000114"))
                                {
                                    await DisplayAlert(messageList["701-000114"].title, messageList["701-000114"].message, messageList["701-000114"].responses);
                                }
                                else
                                {
                                    await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                            }
                        }
                    }

                }
                catch (Exception errorGoogleAuthetication)
                {
                    var client = new Diagnostic();
                    client.parseException(errorGoogleAuthetication.ToString(), user);
                }
            }
        }

        private async void Authenticator_Error(object sender, Xamarin.Auth.AuthenticatorErrorEventArgs e)
        {
            await DisplayAlert("An error occur when authenticating", "Please try again", "OK");
        }

        void CloseSignUpPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
        }
        public event EventHandler SignIn;
        void appleLogInButton_Clicked(System.Object sender, System.EventArgs e)
        {
            if (Device.RuntimePlatform != Device.Android)
            {
                OnAppleSignInRequest();
            }
        }

        public void InvokeSignInEvent(object sender, EventArgs e)
           => SignIn?.Invoke(sender, e);

        public async void OnAppleSignInRequest()
        {
            try
            {
                IAppleSignInService appleSignInService = DependencyService.Get<IAppleSignInService>();
                var account = await appleSignInService.SignInAsync();
                if (account != null)
                {
                    Preferences.Set(App.LoggedInKey, true);
                    await SecureStorage.SetAsync(App.AppleUserIdKey, account.UserId);

                    if (account.Token == null) { account.Token = ""; }
                    if (account.Email != null)
                    {
                        if (Application.Current.Properties.ContainsKey(account.UserId.ToString()))
                        {
                            //Application.Current.Properties[account.UserId.ToString()] = account.Email;
                            Debug.WriteLine((string)Application.Current.Properties[account.UserId.ToString()]);
                        }
                        else
                        {
                            Application.Current.Properties[account.UserId.ToString()] = account.Email;
                        }
                    }
                    if (account.Email == null) { account.Email = ""; }
                    if (account.Name == null) { account.Name = ""; }


                    var clientLogIn = new SignIn();
                    var clientSignUp = new SignUp();
                    var content = clientSignUp.SignUpSocialUser(user, account.Token, "" , account.UserId, account.Email, "APPLE");
                    var signUpStatus = await SignUp.SignUpNewUser(content);


                    if (signUpStatus != "" && signUpStatus != "USER ALREADY EXIST")
                    {
                        user.setUserID(signUpStatus);
                        user.setUserPlatform("APPLE");
                        user.setUserType("CUSTOMER");
                        user.printUser();
                        clientSignUp.WriteDeviceID(user);
                        if (direction == "")
                        {
                            Application.Current.MainPage = new SelectionPage();
                        }
                        else
                        {
                            Dictionary<string, Page> array = new Dictionary<string, Page>();
                            array.Add("ServingFresh.Views.CheckoutPage", new CheckoutPage());
                            array.Add("ServingFresh.Views.SelectionPage", new SelectionPage());
                            array.Add("ServingFresh.Views.HistoryPage", new HistoryPage());
                            array.Add("ServingFresh.Views.RefundPage", new RefundPage());
                            array.Add("ServingFresh.Views.ProfilePage", new ProfilePage());
                            array.Add("ServingFresh.Views.ConfirmationPage", new ConfirmationPage());
                            array.Add("ServingFresh.Views.InfoPage", new InfoPage());

                            var root = Application.Current.MainPage;
                            Debug.WriteLine("ROOT VALUE: " + root);
                            Application.Current.MainPage = array[root.ToString()];
                        }
                    }
                    else if (signUpStatus != "" && signUpStatus == "USER ALREADY EXIST")
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000116"))
                            {
                                await DisplayAlert(messageList["701-000116"].title, messageList["701-000116"].message, messageList["701-000116"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Oops", "This email already exist in our system. Please use another email", "OK");
                        }
                    }

                    //var client = new HttpClient();
                    //var getAppleEmail = new AppleEmail();
                    //getAppleEmail.social_id = account.UserId;

                    //var socialLogInPostSerialized = JsonConvert.SerializeObject(getAppleEmail);

                    //System.Diagnostics.Debug.WriteLine(socialLogInPostSerialized);

                    //var postContent = new StringContent(socialLogInPostSerialized, Encoding.UTF8, "application/json");
                    //var RDSResponse = await client.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/AppleEmail", postContent);
                    //var responseContent = await RDSResponse.Content.ReadAsStringAsync();

                    //System.Diagnostics.Debug.WriteLine(responseContent);
                    //if (RDSResponse.IsSuccessStatusCode)
                    //{
                    //    var data = JsonConvert.DeserializeObject<AppleUser>(responseContent);
                    //    Application.Current.Properties[account.UserId.ToString()] = data.result[0].customer_email;
                    //    account.Email = (string)Application.Current.Properties[account.UserId.ToString()];
                    //    var root = (LogInPage)Application.Current.MainPage;
                    //    root.AppleLogIn("", "", null, account, "APPLE");
                    //    //Application.Current.MainPage = new SelectionPage("", "", null, account, "APPLE");
                    //    //AppleUserProfileAsync(account.UserId, account.Token, (string)Application.Current.Properties[account.UserId.ToString()], account.Name);
                    //}
                    //else
                    //{
                    //    await Application.Current.MainPage.DisplayAlert("Ooops", "Our system is not working. We can't process your request at this moment", "OK");
                    //}

                }
                else
                {
                    //AppleError?.Invoke(this, default(EventArgs));
                }
            }
            catch (Exception errorAppleSignInRequest)
            {
                var client = new Diagnostic();
                client.parseException(errorAppleSignInRequest.ToString(), user);
            }
        }

        public void AppleLogInClick(System.Object sender, System.EventArgs e)
        {
            //SignIn?.Invoke(sender, e);
            //var c = (ImageButton)sender;
            //c.Command?.Execute(c.CommandParameter);
        }

        //public void InvokeSignInEvent(object sender, EventArgs e)
        //    => SignIn?.Invoke(sender, e);


    }
}
