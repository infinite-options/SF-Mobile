using System;
using System.Diagnostics;

namespace ServingFresh.Models
{
    public class User
    {
        public string userType;
        public string id;
        public string firstName;
        public string lastName;
        public string address;
        public string unit;
        public string city;
        public string state;
        public string zipcode;
        public string email;
        public string phoneNumber;
        public string latitude;
        public string longitude;
        public string platform;
        public string deviceID;
        public DateTime sessionTime;
        public string uspsDVPType;
        public string socialMediaImage;


        public User()
        {
            userType = "";
            id = "";
            firstName = "";
            lastName = "";
            address = "";
            unit = "";
            city = "";
            state = "";
            zipcode = "";
            email = "";
            phoneNumber = "";
            latitude = "";
            longitude = "";
            platform = "";
            deviceID = "";
            sessionTime = new DateTime();
            uspsDVPType = "";
            socialMediaImage = "";
        }

        public void setUserFromProfile(UserProfile profile)
        {
            userType = profile.result[0].role;
            id = profile.result[0].customer_uid;
            firstName = profile.result[0].customer_first_name;
            lastName = profile.result[0].customer_last_name;
            address = profile.result[0].customer_address;
            unit = profile.result[0].customer_unit;
            city = profile.result[0].customer_city;
            state = profile.result[0].customer_state;
            zipcode = profile.result[0].customer_zip;
            email = profile.result[0].customer_email;
            phoneNumber = profile.result[0].customer_phone_num;
            latitude = profile.result[0].customer_lat;
            longitude = profile.result[0].customer_long;
            platform = profile.result[0].user_social_media;
            deviceID = "";
        }

        public string getUserUSPSType()
        {
            return uspsDVPType;
        }

        public string getUserType()
        {
            return userType;
        }

        public string getUserID()
        {
            return id;
        }

        public string getUserFirstName()
        {
            return firstName;
        }

        public string getUserLastName()
        {
            return lastName;
        }

        public string getUserPhoneNumber()
        {
            return phoneNumber;
        }

        public string getUserEmail()
        {
            return email;
        }

        public string getUserZipcode()
        {
            return zipcode;
        }

        public string getUserAddress()
        {
            return address;
        }

        public string getUserUnit()
        {
            return unit;
        }

        public string getUserCity()
        {
            return city;
        }

        public string getUserState()
        {
            return state;
        }

        public string getUserLatitude()
        {
            return latitude;
        }

        public string getUserLongitude()
        {
            return longitude;
        }

        public string getUserPlatform()
        {
            return platform;
        }

        public string getUserDeviceID()
        {
            return deviceID;
        }

        public DateTime getUserSessionTime()
        {
            return sessionTime;
        }

        public string getUserImage()
        {
            return socialMediaImage;
        }

        public void setUserType(string userType)
        {
            this.userType = userType;
        }

        public void setUserID(string id)
        {
            this.id = id;
        }

        public void setUserFirstName(string firstName)
        {
            this.firstName = firstName;
        }

        public void setUserLastName(string lastName)
        {
            this.lastName = lastName;
        }

        public void getUserPhoneNumber(string phoneNumber)
        {
            this.phoneNumber = phoneNumber;
        }

        public void getUserEmail(string email)
        {
            this.email = email;
        }

        public void getUserZipcode(string zipcode)
        {
            this.zipcode = zipcode;
        }

        public void setUserAddress(string address)
        {
            this.address = address;
        }

        public void setUserUnit(string unit)
        {
            this.unit = unit;
        }

        public void setUserCity(string city)
        {
            this.city = city;
        }

        public void setUserZipcode(string zipcode)
        {
            this.zipcode = zipcode;
        }

        public void setUserState(string state)
        {
            this.state = state;
        }

        public void setUserPhoneNumber(string phoneNumber)
        {
            this.phoneNumber = phoneNumber;
        }

        public void setUserLatitude(string latitude)
        {
            this.latitude = latitude;
        }

        public void setUserLongitude(string longitude)
        {
            this.longitude = longitude;
        }

        public void setUserPlatform(string platform)
        {
            this.platform = platform;
        }

        public void setUserDeviceID(string deviceID)
        {
            this.deviceID = deviceID;
        }

        public void setUserEmail(string email)
        {
            this.email = email;
        }

        public void setUserSessionTime(DateTime sessionTime)
        {
            this.sessionTime = sessionTime;
        }

        public void setUserUSPSType(string uspsDVPType)
        {
            this.uspsDVPType = uspsDVPType;
        }

        public void setUserImage(string socialMediaImage)
        {
            this.socialMediaImage = socialMediaImage;
        }

        public void printUser()
        {
            Debug.WriteLine("userType: " + userType);
            Debug.WriteLine("id: " + id);
            Debug.WriteLine("firstName: " + firstName);
            Debug.WriteLine("lastName: " + lastName);
            Debug.WriteLine("address: " + address);
            Debug.WriteLine("unit: " + unit);
            Debug.WriteLine("city: " + city);
            Debug.WriteLine("state: " + state);
            Debug.WriteLine("zipcode: " + zipcode);
            Debug.WriteLine("email: " + email);
            Debug.WriteLine("phoneNumber: " + phoneNumber);
            Debug.WriteLine("latitude: " + latitude);
            Debug.WriteLine("longitude: " + longitude);
            Debug.WriteLine("platform: " + platform);
            Debug.WriteLine("deviceID: " + deviceID);
            Debug.WriteLine("sessionTime: " + sessionTime);
            Debug.WriteLine("uspsDVPType: " + uspsDVPType);
            Debug.WriteLine("userImage: " + socialMediaImage);
        }
    }
}
