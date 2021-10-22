using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServingFresh.LogIn.Classes;
using ServingFresh.Config;
using Xamarin.Forms;
using Xamarin.Auth;
using System.Windows.Input;
using Xamarin.Essentials;
using ServingFresh.LogIn.Apple;
using ServingFresh.Notifications;
using System.Diagnostics;
using ServingFresh.Models;
using Acr.UserDialogs;
using static ServingFresh.Views.PrincipalPage;
using static ServingFresh.App;
namespace ServingFresh.Views
{
    public partial class LogInPage : ContentPage
    {
        public string direction = "";
        private string deviceId;

        public LogInPage()
        {
            InitializeComponent();
            SetAppleContinueButtonBaseOnPlatform();
        }

        public LogInPage(double height, string direction)
        {
            InitializeComponent();
            SetAppleContinueButtonBaseOnPlatform();

            logInFrame.Margin = new Thickness(0, height, 0, 0);
            this.direction = direction;
        }

        async void DirectLogInClick(System.Object sender, System.EventArgs e)
        {
            logInButton.IsEnabled = false;

            try
            {

                if (String.IsNullOrEmpty(userEmailAddress.Text) || String.IsNullOrEmpty(userPassword.Text))
                {
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000032"))
                        {
                            await DisplayAlert(messageList["701-000032"].title, messageList["701-000032"].message, messageList["701-000032"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Error", "Please fill in all fields", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Please fill in all fields", "OK");
                    }
                }
                else
                {
                    var signInClient = new SignIn();

                    var accountSalt = await signInClient.RetrieveAccountSalt(userEmailAddress.Text.ToLower().Trim());

                    if (accountSalt != null)
                    {
                        if (accountSalt.password_algorithm == null && accountSalt.password_salt == null && accountSalt.message == "")
                        {
                            await DisplayAlert("Oops", accountSalt.message, "OK");
                        }
                        else if (accountSalt.password_algorithm == null && accountSalt.password_salt == null && accountSalt.message == "USER NEEDS TO SIGN UP")
                        {
                            await Application.Current.MainPage.Navigation.PopModalAsync();
                            RedirectUserBasedOnVerification(accountSalt.message, direction);
                        }
                        else if (accountSalt.password_algorithm != null && accountSalt.password_salt != null && accountSalt.message == null)
                        {
                            var status = await signInClient.VerifyUserCredentials(userEmailAddress.Text.ToLower().Trim(), userPassword.Text, accountSalt);

                            RedirectUserBasedOnVerification(status, direction);
                        }
                    }
                    else
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000034"))
                            {
                                await DisplayAlert(messageList["701-000034"].title, messageList["701-000034"].message, messageList["701-000034"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Alert!", "Our internal system was not able to retrieve your user information. We are working to solve this issue.", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Alert!", "Our internal system was not able to retrieve your user information. We are working to solve this issue.", "OK");
                        }
                    }
                }
            }catch(Exception errorDirectLogIn)
            {
                var client = new Diagnostic();
                client.parseException(errorDirectLogIn.ToString(), user);
            }

            logInButton.IsEnabled = true;
        }

        void SetAppleContinueButtonBaseOnPlatform()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                appleLogInButton.IsVisible = false;
            }
        }

        public void FacebookLogInClick(System.Object sender, System.EventArgs e)
        {
            string clientID = string.Empty;
            string redirectURL = string.Empty;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    clientID = Constant.FacebookiOSClientID;
                    redirectURL = Constant.FacebookiOSRedirectUrl;
                    break;
                case Device.Android:
                    clientID = Constant.FacebookAndroidClientID;
                    redirectURL = Constant.FacebookAndroidRedirectUrl;
                    break;
            }

            var authenticator = new OAuth2Authenticator(clientID, Constant.FacebookScope, new Uri(Constant.FacebookAuthorizeUrl), new Uri(redirectURL), null, false);
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();

            authenticator.Completed += FacebookAuthenticatorCompleted;
            authenticator.Error += FacebookAutheticatorError;

            presenter.Login(authenticator);
        }

        async void FacebookAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthenticatorCompleted;
                authenticator.Error -= FacebookAutheticatorError;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    var client = new SignIn();
                    UserDialogs.Instance.ShowLoading("Retrieving your SF account...");
                    var status = await client.VerifyUserCredentials(e.Account.Properties["access_token"], "", null, null, "FACEBOOK");
                    RedirectUserBasedOnVerification(status, direction);
                }
                catch (Exception errorFacebookAuthenticatorCompleted)
                {
                    var client = new Diagnostic();
                    client.parseException(errorFacebookAuthenticatorCompleted.ToString(), user);
                }
            }
        }

        private async void FacebookAutheticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthenticatorCompleted;
                authenticator.Error -= FacebookAutheticatorError;
            }
            Application.Current.MainPage = new LogInPage();
            await DisplayAlert("Authentication error: ", e.Message, "OK");
        }

        async void RedirectUserBasedOnVerification(string status, string direction)
        {
            try
            {
                if (status.Contains("SUCCESSFUL:"))
                {
                    UserDialogs.Instance.HideLoading();

                    Debug.WriteLine("DIRECTION VALUE: " + direction);
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
                        Application.Current.MainPage = array[root.ToString()];
                    }

                }
                else if (status == "USER NEEDS TO SIGN UP")
                {
                    UserDialogs.Instance.HideLoading();
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000037"))
                        {
                            await Application.Current.MainPage.DisplayAlert(messageList["701-000037"].title, messageList["701-000037"].message, messageList["701-000037"].responses);
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Oops", "It looks like you don't have an account with Serving Fresh. Please sign up!", "OK");
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Oops", "It looks like you don't have an account with Serving Fresh. Please sign up!", "OK");
                    }
                    
                    await Application.Current.MainPage.Navigation.PushModalAsync(new AddressPage(), true);
                }
                else if (status == "WRONG SOCIAL MEDIA TO SIGN IN")
                {
                    UserDialogs.Instance.HideLoading();
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000038"))
                        {
                            await Application.Current.MainPage.DisplayAlert(messageList["701-000038"].title, messageList["701-000038"].message, messageList["701-000038"].responses);
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Oops", "Our records show that you have attempted to log in with a different social media account. Please log in through the correct social media platform. Thanks!", "OK");
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Oops", "Our records show that you have attempted to log in with a different social media account. Please log in through the correct social media platform. Thanks!", "OK");
                    }
                   
                }
                else if (status == "SIGN IN DIRECTLY")
                {
                    UserDialogs.Instance.HideLoading();
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000039"))
                        {
                            await Application.Current.MainPage.DisplayAlert(messageList["701-000039"].title, messageList["701-000039"].message, messageList["701-000039"].responses);
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Oops", "Our records show that you have attempted to log in with a social media account. Please log in through our direct log in. Thanks!", "OK");
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Oops", "Our records show that you have attempted to log in with a social media account. Please log in through our direct log in. Thanks!", "OK");
                    }
                   
                }
                else if (status == "ERROR1")
                {
                    UserDialogs.Instance.HideLoading();
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000040"))
                        {
                            await Application.Current.MainPage.DisplayAlert(messageList["701-000040"].title, messageList["701-000040"].message, messageList["701-000040"].responses);
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Oops", "There was an error getting your account. Please contact customer service", "OK");
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Oops", "There was an error getting your account. Please contact customer service", "OK");
                    }
                    
                }
                else if (status == "ERROR2")
                {
                    UserDialogs.Instance.HideLoading();
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000040"))
                        {
                            await Application.Current.MainPage.DisplayAlert(messageList["701-000040"].title, messageList["701-000040"].message, messageList["701-000040"].responses);
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Oops", "There was an error getting your account. Please contact customer service", "OK");
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Oops", "There was an error getting your account. Please contact customer service", "OK");
                    }
                }
                else
                {
                    UserDialogs.Instance.HideLoading();
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000040"))
                        {
                            await Application.Current.MainPage.DisplayAlert(messageList["701-000040"].title, messageList["701-000040"].message, messageList["701-000040"].responses);
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Oops", "There was an error getting your account. Please contact customer service", "OK");
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Oops", "There was an error getting your account. Please contact customer service", "OK");
                    }
                }
            }catch(Exception errorRedirectUserBaseOnVerification)
            {
                var client = new Diagnostic();
                client.parseException(errorRedirectUserBaseOnVerification.ToString(), user);
            }
        }

        public void GoogleLogInClick(System.Object sender, System.EventArgs e)
        {
            string clientId = string.Empty;
            string redirectUri = string.Empty;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    clientId = Constant.GoogleiOSClientID;
                    redirectUri = Constant.GoogleRedirectUrliOS;
                    break;

                case Device.Android:
                    clientId = Constant.GoogleAndroidClientID;
                    redirectUri = Constant.GoogleRedirectUrlAndroid;
                    break;
            }

            var authenticator = new OAuth2Authenticator(clientId, string.Empty, Constant.GoogleScope, new Uri(Constant.GoogleAuthorizeUrl), new Uri(redirectUri), new Uri(Constant.GoogleAccessTokenUrl), null, true);
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();

            authenticator.Completed += GoogleAuthenticatorCompleted;
            authenticator.Error += GoogleAuthenticatorError;

            AuthenticationState.Authenticator = authenticator;
            presenter.Login(authenticator);
            
        }

        async void GoogleAuthenticatorCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthenticatorCompleted;
                authenticator.Error -= GoogleAuthenticatorError;
            }

            if (e.IsAuthenticated)
            {
                try
                {
                    var client = new SignIn();
                    UserDialogs.Instance.ShowLoading("Retrieving your SF account...");
                    var status = await client.VerifyUserCredentials(e.Account.Properties["access_token"], e.Account.Properties["refresh_token"], e, null, "GOOGLE");
                    RedirectUserBasedOnVerification(status, direction);
                }
                catch(Exception errorGoogleAutheticatorCompleted)
                {
                    var client = new Diagnostic();
                    client.parseException(errorGoogleAutheticatorCompleted.ToString(), user);
                }
                
            }
            else
            {
                //Application.Current.MainPage = new LogInPage();
                //await DisplayAlert("Error", "Google was not able to autheticate your account", "OK");
            }
        }

        async void GoogleAuthenticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthenticatorCompleted;
                authenticator.Error -= GoogleAuthenticatorError;
            }
            Application.Current.MainPage = new LogInPage();
            await DisplayAlert("Authentication error: ", e.Message, "OK");
        }

        public void AppleLogInClick(System.Object sender, System.EventArgs e)
        {
            if (Device.RuntimePlatform != Device.Android)
            {
                OnAppleSignInRequest();
            }
            else
            {
                appleLogInButton.IsVisible = false;
            }
        }

        async void OnAppleSignInRequest()
        {
            try
            {
                IAppleSignInService appleSignInService = DependencyService.Get<IAppleSignInService>();
                var account = await appleSignInService.SignInAsync();

                if (account != null)
                {
                    Preferences.Set(App.LoggedInKey, true);
                    await SecureStorage.SetAsync(App.AppleUserIdKey, account.UserId);
                    string email = "";
                    if (account.Email != null)
                    {
                        await SecureStorage.SetAsync(account.UserId, account.Email);
                        Application.Current.Properties[account.UserId.ToString()] = account.Email;
                    }
                    else
                    {
                        email = await SecureStorage.GetAsync(account.UserId);

                        if (email == null)
                        {
                            if (Application.Current.Properties.ContainsKey(account.UserId.ToString()))
                            {
                                email = (string)Application.Current.Properties[account.UserId.ToString()];
                            }
                            else
                            {
                                email = "";
                            }
                        }

                        account.Email = email;

                        var client = new SignIn();
                        UserDialogs.Instance.ShowLoading("Retrieving your SF account...");
                        var status = await client.VerifyUserCredentials("", "", null, account, "APPLE");
                        RedirectUserBasedOnVerification(status, direction);
                    }
                }
            }
            catch (Exception errorAppleSignInRequest)
            {
                var client = new Diagnostic();
                client.parseException(errorAppleSignInRequest.ToString(), user);
            }
        }

        void ReturnBackToPrincipalPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage = new PrincipalPage();
        }

        void ShowHidePassword(System.Object sender, System.EventArgs e)
        {
            Label label = (Label)sender;
            if (label.Text == "Show password")
            {
                userPassword.IsPassword = false;
                label.Text = "Hide password";
            }
            else
            {
                userPassword.IsPassword = true;
                label.Text = "Show password";
            }
        }

        void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
        }

        void ResetPassword(System.Object sender, System.EventArgs e)
        {
            UpdatePassword(sender, e);
        }

        async void UpdatePassword(System.Object sender, System.EventArgs e)
        {
            if (!String.IsNullOrEmpty(userEmailAddress.Text))
            {
                var client = new SignIn();
                var password = new ResetPassword();
                password.email = userEmailAddress.Text;

                var result = await client.ResetPassword(password);
                if(result != "")
                {
                    if(result.Contains("\"Need to do login via social\""))
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000043"))
                            {
                                await DisplayAlert(messageList["701-000043"].title, messageList["701-000043"].message, messageList["701-000043"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Oops", "We can't reset the password for the given email since it is associated with a media social log-in. Please log in via social media.", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Oops", "We can't reset the password for the given email since it is associated with a media social log-in. Please log in via social media.", "OK");
                        }
                        
                    }
                    else if (result.Contains("\"message\": \"A temporary password has been sent\""))
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000044"))
                            {
                                await DisplayAlert(messageList["701-000044"].title, messageList["701-000044"].message, messageList["701-000044"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Great!", "We have reset your password. A temporary password has been sent to your email.", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Great!", "We have reset your password. A temporary password has been sent to your email.", "OK");
                        }
                        
                    }
                    else if (result.Contains("\"message\": \"No such email exists\""))
                    {
                        if (messageList != null)
                        {
                            if (messageList.ContainsKey("701-000045"))
                            {
                                await DisplayAlert(messageList["701-000045"].title, messageList["701-000045"].message, messageList["701-000045"].responses);
                            }
                            else
                            {
                                await DisplayAlert("Oops", "Our records show that this email is invalid", "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Oops", "Our records show that this email is invalid", "OK");
                        }
                        
                    }
                }
                else
                {
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000046"))
                        {
                            await DisplayAlert(messageList["701-000046"].title, messageList["701-000046"].message, messageList["701-000046"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Oops", "We were not able to fulfill your request. Please try again.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "We were not able to fulfill your request. Please try again.", "OK");
                    }
                    
                }
            }
            else
            {
                if (messageList != null)
                {
                    if (messageList.ContainsKey("701-000047"))
                    {
                        await DisplayAlert(messageList["701-000047"].title, messageList["701-000047"].message, messageList["701-000047"].responses);
                    }
                    else
                    {
                        await DisplayAlert("Oops", "Plese enter the email address associated with your Serving Fresh account", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Oops", "Plese enter the email address associated with your Serving Fresh account", "OK");
                }
            }
        }
    }
}
