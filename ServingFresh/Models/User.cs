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
            sessionTime = new DateTime();
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

        public void setUserState(string state)
        {
            this.state = state;
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

        public void setUserSessionTime(DateTime sessionTime)
        {
            this.sessionTime = sessionTime;
        }
    }
}
