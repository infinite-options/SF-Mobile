using System;
using System.Collections.Generic;
using ServingFresh.Models;
using Xamarin.Forms;
using static ServingFresh.Views.PrincipalPage;
using static ServingFresh.App;
using System.Diagnostics;
using Acr.UserDialogs;

namespace ServingFresh.Views
{
    public partial class AddressPage : ContentPage
    {
        private Address addr = new Address();
        private AddressAutocomplete addressToValidate = null;
        public AddressPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("AB000000");
        }

        async void ContinueWithSignUp(System.Object sender, System.EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("We are checking if we deliver to your location...");
                var client1 = new SignUp();
                if (client1.ValidateSignUpInfo(firstName, lastName, signUpPhone, signUpAddress1Entry, signUpCityEntry, signUpStateEntry, signUpZipcodeEntry))
                {
                    //try to validate address if address doesn't return true ask to enter unit number and try again
                    var client = new AddressValidation();
                    var addressStatus = client.ValidateAddressString(signUpAddress1Entry.Text, signUpAddress2Entry.Text, signUpCityEntry.Text, signUpStateEntry.Text, signUpZipcodeEntry.Text);

                    if (addressStatus != null)
                    {
                        if (addressStatus == "Y" || addressStatus == "S")
                        {

                            var location = await client.ConvertAddressToGeoCoordiantes(signUpAddress1Entry.Text, signUpCityEntry.Text, signUpStateEntry.Text);
                            if (location != null)
                            {
                                var isAddressInZones = await client.getZoneFromLocation(location.Latitude.ToString(), location.Longitude.ToString());

                                if (isAddressInZones != "" && isAddressInZones != "OUTSIDE ZONE RANGE")
                                {
                                    user.setUserUSPSType(addressStatus);
                                    user.setUserFirstName(firstName.Text);
                                    user.setUserLastName(lastName.Text);
                                    user.setUserPhoneNumber(signUpPhone.Text);
                                    user.setUserAddress(signUpAddress1Entry.Text);
                                    user.setUserUnit(signUpAddress2Entry.Text == null ? "" : signUpAddress2Entry.Text);
                                    user.setUserCity(signUpCityEntry.Text);
                                    user.setUserState(signUpStateEntry.Text);
                                    user.setUserZipcode(signUpZipcodeEntry.Text);
                                    user.setUserLatitude(location.Latitude.ToString());
                                    user.setUserLongitude(location.Longitude.ToString());
                                    user.setUserType("GUEST");
                                    UserDialogs.Instance.HideLoading();
                                    var action = await DisplayActionSheet("Hooray!\n\nLooks like we deliver to your address. Click the button below to see the variety of fresh organic fruits and vegetables we offer.", "Cancel", null, new[] { "Explore Local Produce", "Complete Sign Up" });
                                    if (action != "Cancel")
                                    {
                                        if (action == "Explore Local Produce")
                                        {
                                            UserDialogs.Instance.HideLoading();
                                            Application.Current.MainPage = new SelectionPage();
                                        }
                                        else if (action == "Complete Sign Up")
                                        {
                                            UserDialogs.Instance.HideLoading();
                                            await Application.Current.MainPage.Navigation.PopModalAsync();
                                            await Application.Current.MainPage.Navigation.PushModalAsync(new SignUpPage(), true);
                                        }
                                    }
                                }
                                else
                                {
                                    // Need a cancel button on UI
                                    UserDialogs.Instance.HideLoading();
                                    var emailAddress = await DisplayPromptAsync("Still Growing...", "Sorry, it looks like we don’t deliver to your neighborhood yet. Enter your email address, and we will let you know as soon as we come to your area.", "OK", "Cancel");
                                    return;
                                }
                            }
                            else
                            {
                                if (messageList != null)
                                {
                                    if (messageList.ContainsKey("701-000001"))
                                    {
                                        UserDialogs.Instance.HideLoading();
                                        await DisplayAlert(messageList["701-000001"].title, messageList["701-000001"].message, messageList["701-000001"].responses);
                                    }
                                    else
                                    {
                                        UserDialogs.Instance.HideLoading();
                                        await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                                    }
                                }
                                else
                                {
                                    UserDialogs.Instance.HideLoading();
                                    await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                                }
                                UserDialogs.Instance.HideLoading();
                                return;
                            }

                        }
                        else if (addressStatus == "D")
                        {
                            if (messageList != null)
                            {
                                if (messageList.ContainsKey("701-000002"))
                                {
                                    UserDialogs.Instance.HideLoading();
                                    await DisplayAlert(messageList["701-000002"].title, messageList["701-000002"].message, messageList["701-000002"].responses);
                                }
                                else
                                {
                                    UserDialogs.Instance.HideLoading();
                                    await DisplayAlert("Oops", "Please enter your address unit number", "OK");
                                }
                            }
                            else
                            {
                                UserDialogs.Instance.HideLoading();
                                await DisplayAlert("Oops", "Please enter your address unit number", "OK");
                            }
                            UserDialogs.Instance.HideLoading();
                            return;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("addressStatus: " + "null");
                    }
                }
                else
                {
                    if (messageList != null)
                    {
                        if (messageList.ContainsKey("701-000003"))
                        {
                            UserDialogs.Instance.HideLoading();
                            await DisplayAlert(messageList["701-000003"].title, messageList["701-000003"].message, messageList["701-000003"].responses);
                        }
                        else
                        {
                            UserDialogs.Instance.HideLoading();
                            await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                        }
                    }
                    else
                    {
                        UserDialogs.Instance.HideLoading();
                        await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                    }
                    UserDialogs.Instance.HideLoading();
                    return;
                }
                UserDialogs.Instance.HideLoading();
            }
            catch(Exception errorContinueWithSignUp)
            {
                UserDialogs.Instance.HideLoading();
                var client = new Diagnostic();
                client.parseException(errorContinueWithSignUp.ToString(), user);
            }
        }

        async void signUpAddress1Entry_TextChanged(System.Object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrEmpty(signUpAddress1Entry.Text))
            {
                

                if (addressToValidate != null)
                {
                    if (addressToValidate.Street != signUpAddress1Entry.Text)
                    {
                        SignUpAddressList.ItemsSource = await addr.GetPlacesPredictionsAsync(signUpAddress1Entry.Text);
                        signUpAddress1Entry_Focused(sender, eventArgs);
                    }
                }
                else
                {
                    SignUpAddressList.ItemsSource = await addr.GetPlacesPredictionsAsync(signUpAddress1Entry.Text);
                    signUpAddress1Entry_Focused(sender, eventArgs);
                }
            }
            else
            {
                signUpAddress1Entry_Unfocused(sender, eventArgs);
                addressToValidate = null;
            }
        }

        void signUpAddress1Entry_Focused(System.Object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrEmpty(signUpAddress1Entry.Text)) {

                if(Device.RuntimePlatform == Device.iOS)
                {
                    var currentScrollYPosition = addressScrollView.ScrollX;
                    addressScrollView.ScrollToAsync(0, currentScrollYPosition + 100, true);
                }else if (Device.RuntimePlatform == Device.Android)
                {
                    var currentScrollYPosition = addressScrollView.ScrollX;
                    addressScrollView.ScrollToAsync(0, currentScrollYPosition + 150, true);
                }
                
                addr.addressEntryFocused(SignUpAddressList, signUpAddressFrame);
            }
        }

        void signUpAddress1Entry_Unfocused(System.Object sender, EventArgs eventArgs)
        {
            addr.addressEntryUnfocused(SignUpAddressList, signUpAddressFrame);
        }

        async void SignUpAddressList_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            signUpAddress1Entry.TextChanged -= signUpAddress1Entry_TextChanged;
            addressToValidate = addr.addressSelected(SignUpAddressList, signUpAddress1Entry, signUpAddressFrame);
            string zipcode = await addr.getZipcode(addressToValidate.PredictionID);
            if (zipcode != null)
            {
                addressToValidate.ZipCode = zipcode;
            }
            addr.addressSelectedFillEntries(addressToValidate, signUpAddress1Entry, signUpAddress2Entry, signUpCityEntry, signUpStateEntry, signUpZipcodeEntry);
            addr.addressEntryUnfocused(SignUpAddressList, signUpAddressFrame);
            signUpAddress1Entry.TextChanged += signUpAddress1Entry_TextChanged;

        }

        void CloseSignUpPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
        }
    }
}
