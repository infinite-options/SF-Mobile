using System;
namespace ServingFresh.Models
{
    public class User
    {
        private string userType;
        private string id;
        private string firstName;
        private string lastName;
        private string address;
        private string unit;
        private string city;
        private string state;
        private string zipcode;
        private string email;
        private string phoneNumber;
        private string latitude;
        private string longitude;
        private string platform;
        private string deviceID;
        private DateTime sessionTime;


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

        public void  setUserDeviceID(string deviceID)
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
    }
}
