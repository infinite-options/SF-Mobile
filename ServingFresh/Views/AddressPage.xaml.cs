using System;
using System.Collections.Generic;
using ServingFresh.Models;
using Xamarin.Forms;
using static ServingFresh.Views.PrincipalPage;
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
            var client1 = new SignUp();
            if (client1.ValidateSignUpInfo(signUpPhone, signUpAddress1Entry, signUpCityEntry, signUpStateEntry, signUpZipcodeEntry))
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

                                user.setUserPhoneNumber(signUpPhone.Text);
                                user.setUserAddress(signUpAddress1Entry.Text);
                                user.setUserUnit(signUpAddress2Entry.Text);
                                user.setUserCity(signUpCityEntry.Text);
                                user.setUserState(signUpStateEntry.Text);
                                user.setUserZipcode(signUpZipcodeEntry.Text);
                                user.setUserLatitude(location.Latitude.ToString());
                                user.setUserLongitude(location.Longitude.ToString());
                                user.setUserType("GUEST");
                                var action = await DisplayActionSheet("Hooray!\n\nLooks like we deliver to your address. Click the button below to see the variety of fresh organic fruits and vegetables we offer.", "Cancel", null, new[] { "Explore Local Produce", "Sign Up" });
                                if(action != "Cancel")
                                {
                                    if(action == "Explore Local Produce")
                                    {
                                        Application.Current.MainPage = new SelectionPage();
                                    }
                                    else if (action == "Sign Up")
                                    {
                                        await Application.Current.MainPage.Navigation.PopModalAsync();
                                        await Application.Current.MainPage.Navigation.PushModalAsync(new SignUpPage(),true);
                                    }
                                }
                            }
                            else
                            {
                                // Need a cancel button on UI
                                var emailAddress = await DisplayPromptAsync("Still Growing...", "Sorry, it looks like we don’t deliver to your neighborhood yet. Enter your email address and we will let you know as soon as we come to your neighborhood.", "OK", "Cancel");
                                return;
                            }
                        }
                        else
                        {
                            await DisplayAlert("We were not able to find your location in our system.", "Try again", "OK");
                            return;
                        }

                    }
                    else if (addressStatus == "D")
                    {
                        await DisplayAlert("Oops", "Please enter your address unit number", "OK");
                        return;
                    }
                }
            }
            else
            {
                await DisplayAlert("Oops", "Please enter all the required information. Thanks!", "OK");
                return;
            }
        }

        async void signUpAddress1Entry_TextChanged(System.Object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            SignUpAddressList.ItemsSource = await addr.GetPlacesPredictionsAsync(signUpAddress1Entry.Text);
        }

        void signUpAddress1Entry_Focused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            if (!String.IsNullOrEmpty(signUpAddress1Entry.Text))
            {
                addr.addressEntryFocused(SignUpAddressList, signUpAddressFrame);
            }
        }

        void signUpAddress1Entry_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            addr.addressEntryUnfocused(SignUpAddressList, signUpAddressFrame);
        }

        async void SignUpAddressList_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            addressToValidate = addr.addressSelected(SignUpAddressList, signUpAddress1Entry, signUpAddressFrame);
            string zipcode = await addr.getZipcode(addressToValidate.PredictionID);
            if (zipcode != null)
            {
                addressToValidate.ZipCode = zipcode;
            }
            addr.addressSelectedFillEntries(addressToValidate, signUpAddress1Entry, signUpAddress2Entry, signUpCityEntry, signUpStateEntry, signUpZipcodeEntry);
            addr.addressEntryUnfocused(SignUpAddressList, signUpAddressFrame);
        }

        void CloseSignUpPage(System.Object sender, System.EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync();
        }
    }
}
