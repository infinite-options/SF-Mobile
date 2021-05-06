using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServingFresh.Config;
using ServingFresh.LogIn.Classes;
using ServingFresh.Models.Interfaces;
using ServingFresh.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ServingFresh.Models
{
    public class SignUp
    {
        public SignUp()
        {

        }


        // NON EXISTING ACCOUNT...
        public SignUpPost SetDirectUser(User user, string password)
        {
            // takes a user object and sign it as customer

            var userContent = new SignUpPost()
            {
                email = user.getUserEmail(),
                first_name = user.getUserFirstName(),
                last_name = user.getUserLastName(),
                phone_number = user.getUserPhoneNumber(),
                address = user.getUserAddress(),
                unit = user.getUserUnit(),
                city = user.getUserCity(),
                state = user.getUserState(),
                zip_code = user.getUserZipcode(),
                latitude = user.getUserLatitude(),
                longitude = user.getUserLongitude(),
                referral_source = GetDeviceInformation() + GetAppVersion(),
                role = "CUSTOMER",
                mobile_access_token = "FALSE",
                mobile_refresh_token = "FALSE",
                user_access_token = "FALSE",
                user_refresh_token = "FALSE",
                social = "FALSE",
                password = password,
                social_id = "NULL",
            };

            return userContent;
        }

        // EXISTING ACCOUNT...
        public UpdateProfile UpdateDirectUser(User user, string password)
        {
            // takes a user object and sign it as customer

            var userContent = new UpdateProfile()
            {
                customer_uid = user.getUserID(),
                email = user.getUserEmail(),
                first_name = user.getUserFirstName(),
                last_name = user.getUserLastName(),
                phone_number = user.getUserPhoneNumber(),
                address = user.getUserAddress(),
                unit = user.getUserUnit(),
                city = user.getUserCity(),
                state = user.getUserState(),
                zip_code = user.getUserZipcode(),
                latitude = user.getUserLatitude(),
                longitude = user.getUserLongitude(),
                referral_source = GetDeviceInformation() + GetAppVersion(),
                role = "CUSTOMER",
                mobile_access_token = "FALSE",
                mobile_refresh_token = "FALSE",
                user_access_token = "FALSE",
                user_refresh_token = "FALSE",
                social = "FALSE",
                password = password,
                social_id = "NULL",
            };

            return userContent;
        }

        // NON EXISTING ACCOUNT...
        public SignUpPost SignUpSocialUser(User user, string accessToken, string refreshToken, string socialID, string email, string platform)
        {
            // takes a user object and sign it as customer

            var userContent = new SignUpPost()
            {
                email = email,
                first_name = user.getUserFirstName(),
                last_name = user.getUserLastName(),
                phone_number = user.getUserPhoneNumber(),
                address = user.getUserAddress(),
                unit = user.getUserUnit(),
                city = user.getUserCity(),
                state = user.getUserState(),
                zip_code = user.getUserZipcode(),
                latitude = user.getUserLatitude(),
                longitude = user.getUserLongitude(),
                referral_source = GetDeviceInformation() + GetAppVersion(),
                role = "CUSTOMER",
                mobile_access_token = accessToken,
                mobile_refresh_token = refreshToken,
                user_access_token = "FALSE",
                user_refresh_token = "FALSE",
                social = platform,
                password = "",
                social_id = socialID,
            };

            return userContent;
        }

        // EXISTING ACCOUNT...
        public UpdateProfile UpdateSocialUser(User user, string accessToken, string refreshToken, string socialID, string platform)
        {
            // takes a user object and sign it as customer

            var userContent = new UpdateProfile()
            {
                customer_uid = user.getUserID(),
                email = user.getUserEmail(),
                first_name = user.getUserFirstName(),
                last_name = user.getUserLastName(),
                phone_number = user.getUserPhoneNumber(),
                address = user.getUserAddress(),
                unit = user.getUserUnit(),
                city = user.getUserCity(),
                state = user.getUserState(),
                zip_code = user.getUserZipcode(),
                latitude = user.getUserLatitude(),
                longitude = user.getUserLongitude(),
                referral_source = GetDeviceInformation() + GetAppVersion(),
                role = "CUSTOMER",
                mobile_access_token = accessToken,
                mobile_refresh_token = refreshToken,
                user_access_token = "FALSE",
                user_refresh_token = "FALSE",
                social = platform,
                password = "",
                social_id = socialID,
            };

            return userContent;
        }

        public static string GetAppVersion()
        {
            string versionStr = "";
            string buildStr = "";

            versionStr = DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
            buildStr = DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber();

            return versionStr + ", " + buildStr;
        }

        public static string GetDeviceInformation()
        {
            var device = "";
            if (Device.RuntimePlatform == Device.Android)
            {
                device = "MOBILE: ANDROID, ";
            }
            else
            {
                device = "MOBILE: IOS, ";
            }
            return device;
        }

        public void SendUserToSelectionPage()
        {
            Application.Current.MainPage = new SelectionPage();
        }

        public void SendUserToCheckoutPage()
        {
            Application.Current.MainPage = new SelectionPage();
        }

        public static async Task<string> SignUpNewUser(SignUpPost newUser)
        {
            //var handler = new HttpClientHandler();
            //handler.AllowAutoRedirect = true;
            var userID = "";
            var client = new HttpClient();
            var serializedObject = JsonConvert.SerializeObject(newUser);
            var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            var endpointCall = await client.PostAsync(Constant.SignUpUrl, content);

            Debug.WriteLine("USER ROLE: " + newUser.role);
            Debug.WriteLine("JSON TO SEND VIA SIGN UP ENDPOINT: " + serializedObject);

            if (endpointCall.IsSuccessStatusCode)
            {
                var endpointContentString = await endpointCall.Content.ReadAsStringAsync();
                var parsedData = JsonConvert.DeserializeObject<SignUpResponse>(endpointContentString);
                if (parsedData.code != Constant.EmailAlreadyExist)
                {
                    userID = parsedData.result.customer_uid;
                }
                else
                {
                    userID = "USER ALREADY EXIST";
                }
            }

            return userID;
        }

        public bool ValidateSignUpInfo(Entry phoneNumber, Entry address1, Entry city, Entry state, Entry zipcode)
        {
            bool result = false;
            if (!( String.IsNullOrEmpty(phoneNumber.Text)
                || String.IsNullOrEmpty(address1.Text)
                || String.IsNullOrEmpty(city.Text)
                || String.IsNullOrEmpty(state.Text)
                || String.IsNullOrEmpty(zipcode.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool ValidateSignUpInfo(Entry firstName, Entry lastName, Entry email1, Entry email2, Entry password1, Entry password2)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(firstName.Text)
                || String.IsNullOrEmpty(lastName.Text)
                || String.IsNullOrEmpty(email1.Text)
                || String.IsNullOrEmpty(email2.Text)
                || String.IsNullOrEmpty(password1.Text)
                || String.IsNullOrEmpty(password2.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool ValidateSignUpInfo(Entry firstName, Entry lastName, Entry email, Entry phoneNumber, Entry address1, Entry city, Entry state, Entry zipcode)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(firstName.Text)
                || String.IsNullOrEmpty(lastName.Text)
                || String.IsNullOrEmpty(phoneNumber.Text)
                || String.IsNullOrEmpty(email.Text)
                || String.IsNullOrEmpty(address1.Text)
                || String.IsNullOrEmpty(city.Text)
                || String.IsNullOrEmpty(state.Text)
                || String.IsNullOrEmpty(zipcode.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool ValidateSignUpInfo(Entry firstName, Entry lastName, Entry phoneNumber, Entry address1, Entry city, Entry state, Entry zipcode)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(firstName.Text)
                || String.IsNullOrEmpty(lastName.Text)
                || String.IsNullOrEmpty(phoneNumber.Text)
                || String.IsNullOrEmpty(address1.Text)
                || String.IsNullOrEmpty(city.Text)
                || String.IsNullOrEmpty(state.Text)
                || String.IsNullOrEmpty(zipcode.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool GuestCheckAllRequiredEntries(Entry firstName, Entry lastName, Entry email, Entry phoneNumber, Entry cardHolderName, Entry cardNumber, Entry cardCvv, Entry cardMonth, Entry cardYear, Entry cardZipcode)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(firstName.Text)
                || String.IsNullOrEmpty(lastName.Text)
                || String.IsNullOrEmpty(phoneNumber.Text)
                || String.IsNullOrEmpty(email.Text)
                || String.IsNullOrEmpty(cardHolderName.Text)
                || String.IsNullOrEmpty(cardNumber.Text)
                || String.IsNullOrEmpty(cardCvv.Text)
                || String.IsNullOrEmpty(cardMonth.Text)
                || String.IsNullOrEmpty(cardYear.Text)
                || String.IsNullOrEmpty(cardZipcode.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool GuestCheckAllStripeRequiredEntries( Entry cardHolderName, Entry cardNumber, Entry cardCvv, Entry cardMonth, Entry cardYear, Entry cardZipcode)
        {
            bool result = false;
            if (!( String.IsNullOrEmpty(cardHolderName.Text)
                || String.IsNullOrEmpty(cardNumber.Text)
                || String.IsNullOrEmpty(cardCvv.Text)
                || String.IsNullOrEmpty(cardMonth.Text)
                || String.IsNullOrEmpty(cardYear.Text)
                || String.IsNullOrEmpty(cardZipcode.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool GuestCheckAllRequiredEntries(Entry firstName, Entry lastName, Entry email, Entry phoneNumber)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(firstName.Text)
                || String.IsNullOrEmpty(lastName.Text)
                || String.IsNullOrEmpty(phoneNumber.Text)
                || String.IsNullOrEmpty(email.Text)
                ))
            {
                result = true;
            }
            return result;
        }

        public bool ValidateDirectSignInCredentials(Entry email, Entry password)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(email.Text)
                || String.IsNullOrEmpty(password.Text)
                ))
            {
                result = true;
            }
            return result;
        }


        public bool ValidatePassword(Entry password1, Entry password2)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(password1.Text) || String.IsNullOrEmpty(password2.Text)))
            {
                if (password1.Text == password2.Text)
                {
                    result = true;
                }
            }
            return result;
        }


        public bool ValidateEmail(Entry email1, Entry email2)
        {
            bool result = false;
            if (!(String.IsNullOrEmpty(email1.Text) || String.IsNullOrEmpty(email2.Text)))
            {
                if (email1.Text == email2.Text)
                {
                    result = true;
                }
            }
            return result;
        }

        public static async Task<bool> SignUpNewUser(UpdateProfile existingUser)
        {
            bool result = false;
            var client = new HttpClient();
            var serializedObject = JsonConvert.SerializeObject(existingUser);
            var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            var endpointCall = await client.PostAsync("https://tsx3rnuidi.execute-api.us-west-1.amazonaws.com/dev/api/v2/createAccountGuestToCustomer", content);

            Debug.WriteLine("USER ROLE: " + existingUser.role);
            Debug.WriteLine("JSON TO SEND VIA SIGN UP ENDPOINT: " + serializedObject);

            if (endpointCall.IsSuccessStatusCode)
            {
                var endpointContentString = await endpointCall.Content.ReadAsStringAsync();
                Debug.WriteLine("UPDATED PROFILE: " + endpointContentString);
                result = true;
            }

            return result;
        }

        public async void WriteDeviceID(User user)
        {
            var deviceId = "";
            var client = new HttpClient();
            if (Device.RuntimePlatform == Device.iOS)
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the iOS GUID from Log in: " + deviceId); }
            }
            else
            {
                deviceId = Preferences.Get("guid", null);
                if (deviceId != null) { Debug.WriteLine("This is the Android GUID from Log in " + deviceId); }
            }

            if (deviceId != null)
            {
                NotificationPost notificationPost = new NotificationPost();

                notificationPost.uid = user.getUserID();
                notificationPost.guid = deviceId.Substring(5);
                user.setUserDeviceID(deviceId.Substring(5));
                notificationPost.notification = "TRUE";

                var notificationSerializedObject = JsonConvert.SerializeObject(notificationPost);
                Debug.WriteLine("Notification JSON Object to send: " + notificationSerializedObject);

                var notificationContent = new StringContent(notificationSerializedObject, Encoding.UTF8, "application/json");

                var clientResponse = await client.PostAsync(Constant.NotificationsUrl, notificationContent);

                Debug.WriteLine("Status code: " + clientResponse.IsSuccessStatusCode);

                if (clientResponse.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("We have post the guid to the database");
                }
                else
                {
                    //await DisplayAlert("Ooops!", "Something went wrong. We are not able to send you notification at this moment", "OK");
                }
            }
        }

        public static SignUpPost GetUserFrom(Purchase purchase)
        {
            return new SignUpPost()
            {
                email = purchase.getPurchaseEmail(),
                first_name = purchase.getPurchaseFirstName(),
                last_name = purchase.getPurchaseLastName(),
                phone_number = purchase.getPurchasePhoneNumber(),
                address = purchase.getPurchaseAddress(),
                unit = purchase.getPurchaseUnit(),
                city = purchase.getPurchaseCity(),
                state = purchase.getPurchaseState(),
                zip_code = purchase.getPurchaseZipcode(),
                latitude = purchase.getPurchaseLatitude(),
                longitude = purchase.getPurchaseLongitude(),
                referral_source = GetDeviceInformation() + GetAppVersion(),
                role = "GUEST",
                mobile_access_token = "FALSE",
                mobile_refresh_token = "FALSE",
                user_access_token = "FALSE",
                user_refresh_token = "FALSE",
                social = "FALSE",
                password = GetAutoGeneratedPasswordFrom(purchase.getPurchaseFirstName(), purchase.getPurchaseAddress()),
                social_id = "NULL",
            };
        }

        public static string GetAutoGeneratedPasswordFrom(string firstName, string address)
        {
            var part1 = firstName;
            var part2 = "";

            foreach (char element in address.ToCharArray())
            {
                if (element != ' ')
                {
                    part2 += element;
                }
                else
                {
                    break;
                }
            }

            return part1 + part2;
        }
    }
}
