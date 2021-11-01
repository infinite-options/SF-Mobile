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
        // Atributes

        public string direction = "";

        // Constructor: Default

        public LogInPage()
        {
            InitializeComponent();
            SetAppleContinueButtonBaseOnPlatform();
        }

        // Constructor: Takes height and direction.
        // This constructor is use when users try to sign in from with in the app.
        // For example, a customer could just use the app as a guest and then later on log in from any where in the app.

        public LogInPage(double height, string direction)
        {
            InitializeComponent();
            SetAppleContinueButtonBaseOnPlatform();

            logInFrame.Margin = new Thickness(0, height, 0, 0);
            this.direction = direction;
        }

        public LogInPage(string platform)
        {
            if(platform == Constant.Facebook)
            {
                FacebookLogInClick(new System.Object(), new EventArgs());
            }
            else if (platform == Constant.Google)
            {
                GoogleLogInClick(new System.Object(), new EventArgs());
            }
            else if (platform == Constant.Apple)
            {
                AppleLogInClick(new System.Object(), new EventArgs());
            }
        }

        void SetAppleContinueButtonBaseOnPlatform()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                appleLogInButton.IsVisible = false;
            }
        }

        // MAIN LOGIN EVENT HANDLERS _________________________________________________

        // This function handles the direct login request. It first retrives the salt account, then it
        // verifies the credentails to find out if user gets access or not. This function calls functions
        // in the sign in class and uses their responses to find out what action to take at this level.

        public async void DirectLogInClick(System.Object sender, System.EventArgs e)
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
                        if (accountSalt.password_algorithm == null && accountSalt.password_salt == null && accountSalt.message != "USER NEEDS TO SIGN UP")
                        {
                            await DisplayAlert("Oops", accountSalt.message, "OK");
                        }
                        else if (accountSalt.password_algorithm == null && accountSalt.password_salt == null && accountSalt.message == "USER NEEDS TO SIGN UP")
                        {
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

        // This function handles the Facebook login request. It calls the FacebookAutheticationCompleted function
        // to find out if user login successfully or not. Note that if the LoginPage is push modally into the navigation
        // stack you have to pop modally again to present the Facebook login screen. 

        public void FacebookLogInClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();

            var client = new SignIn();
            var authenticator = client.GetFacebookAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();

            authenticator.Completed += FacebookAuthenticatorCompleted;
            authenticator.Error += FacebookAutheticatorError;
            presenter.Login(authenticator);
        }

        // This function handles the Google login request. It calls the GoogleAutheticationCompleted function
        // to find out if user login successfully or not. Note that if the LoginPage is push modally into the navigation
        // stack you have to pop modally again to present the Google login screen. Also, in comparison with the Facebook or Apple
        // login mechanism Google needs to set up a the state of the autheticator to know when to redirect the user back to the app.

        public void GoogleLogInClick(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();

            var client = new SignIn();
            var authenticator = client.GetGoogleAuthetication();
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();

            AuthenticationState.Authenticator = authenticator;

            authenticator.Completed += GoogleAuthenticatorCompleted;
            authenticator.Error += GoogleAuthenticatorError;
            presenter.Login(authenticator);
        }

        // This function handles the Apple login request. It calls the OnAppleSignInRequest function
        // to find out if user login successfully or not. 

        public void AppleLogInClick(System.Object sender, System.EventArgs e)
        {
            if (Device.RuntimePlatform != Device.Android)
            {
                OnAppleSignInRequest();
            }
        }

        // AUTHETICATION HANDLERS _________________________________________________

        // This function checks if the user was able to succesfully login to their Facebook account. Once the
        // user autheticates throught Facebook, we validate their credentials in our system.

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

        // This function checks if the user was able to succesfully login to their Google account. Once the
        // user autheticates throught Google, we validate their credentials in our system.

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
                catch (Exception errorGoogleAutheticatorCompleted)
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

        // This function checks if the user was able to succesfully login to their Apple account. Once the
        // user autheticates throught Apple, we validate their credentials in our system.

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
                    }

                    var client = new SignIn();
                    UserDialogs.Instance.ShowLoading("Retrieving your SF account...");
                    var status = await client.VerifyUserCredentials("", "", null, account, "APPLE");
                    RedirectUserBasedOnVerification(status, direction);
                }
            }
            catch (Exception errorAppleSignInRequest)
            {
                var client = new Diagnostic();
                client.parseException(errorAppleSignInRequest.ToString(), user);
            }
        }

        // AUTHETICATION HANDLERS _________________________________________________

        // AUTHETICATION ERROR HANDLERS ___________________________________________

        // This function gets call if there was an error authenticating with Facebook.

        private async void FacebookAutheticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= FacebookAuthenticatorCompleted;
                authenticator.Error -= FacebookAutheticatorError;
            }
            Application.Current.MainPage = new PrincipalPage();
            await DisplayAlert("Authentication error: ", e.Message, "OK");
        }

        // This function gets call if there was an error authenticating with Google.

        async void GoogleAuthenticatorError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;

            if (authenticator != null)
            {
                authenticator.Completed -= GoogleAuthenticatorCompleted;
                authenticator.Error -= GoogleAuthenticatorError;
            }
            Application.Current.MainPage = new PrincipalPage();
            await DisplayAlert("Authentication error: ", e.Message, "OK");
        }

        // AUTHETICATION ERROR HANDLERS ___________________________________________

        // This function handles the status of each authetication and redirects user to
        // appropiate page or gives them an alert message to find out what they should do next.

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
                    await Navigation.PopModalAsync(false);
                    await Application.Current.MainPage.Navigation.PushModalAsync(new AddressPage(), true);
                }
                else if (status == "WRONG DIRECT PASSWORD")
                {
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000035"))
                        {
                            await DisplayAlert(messageList["701-000035"].title, messageList["701-000035"].message, messageList["701-000035"].responses);
                        }
                        else
                        {
                            await DisplayAlert("Error", "Wrong password was entered", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Wrong password was entered", "OK");
                    }
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
            }
            catch (Exception errorRedirectUserBaseOnVerification)
            {
                var client = new Diagnostic();
                client.parseException(errorRedirectUserBaseOnVerification.ToString(), user);
            }
        }

        // This function shows and hides password. 
        
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

        // This function pops the login page which is equivalent to closing the page.

        void CloseLoginPage (System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
        }

        // This function handles the reset password request. 

        void ResetPassword(System.Object sender, System.EventArgs e)
        {
            SendUserResetPasswordLink();
        }

        // This function sends the user their reset password link by calling
        // function from the sign in class. 

        async void SendUserResetPasswordLink()
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
