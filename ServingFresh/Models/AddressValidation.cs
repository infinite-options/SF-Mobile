using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using ServingFresh.Config;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;

namespace ServingFresh.Models
{
    public class AddressValidation
    {
        public AddressValidation()
        {

        }

        public async Task<Location> ValidateAddress(string address, string unit, string city, string state, string zipcode)
        {
            Location result = null;

            // Setting request for USPS API
            XDocument requestDoc = new XDocument(
                new XElement("AddressValidateRequest",
                new XAttribute("USERID", "400INFIN1745"),
                new XElement("Revision", "1"),
                new XElement("Address",
                new XAttribute("ID", "0"),
                new XElement("Address1", address),
                new XElement("Address2", unit),
                new XElement("City", city),
                new XElement("State", state),
                new XElement("Zip5", zipcode),
                new XElement("Zip4", "")
                     )
                 )
             );

            // This endpoint needs to change
            var url = "http://production.shippingapis.com/ShippingAPI.dll?API=Verify&XML=" + requestDoc;
            var client = new WebClient();
            var response = client.DownloadString(url);
            var xdoc = XDocument.Parse(response.ToString());

            foreach (XElement element in xdoc.Descendants("Address"))
            {
                if (GetXMLElement(element, "Error").Equals(""))
                {
                    if (GetXMLElement(element, "DPVConfirmation").Equals("Y") && GetXMLElement(element, "Zip5").Equals(zipcode) && GetXMLElement(element, "City").Equals(city.ToUpper()))
                    {
                        result = await ConvertAddressToGeoCoordiantes(address, city, state);
                        break;
                    }
                }
            }

            return result;
        }

        public async Task<Location> ConvertAddressToGeoCoordiantes(string address, string city, string state)
        {
            Location result = null;

            try{
                Debug.WriteLine("INPUTS TO CONVERT ADDRESS TO GEO COORDINATES: ADDRESS: {0}, CITY: {1}, STATE: {2}", address, city, state);
                Geocoder geoCoder = new Geocoder();
                IEnumerable<Position> approximateLocations = await geoCoder.GetPositionsForAddressAsync(address + "," + city + "," + state);
                Position position = approximateLocations.FirstOrDefault();
                Debug.WriteLine("OUTPUT COORDINATES: LATITUDE: {0}, LONGITUDE: {1}", position.Latitude, position.Longitude);
                result = new Location(position.Latitude, position.Longitude);
            }catch(Exception unknowAddress)
            {
                string exception = unknowAddress.Message;
            }

            return result;
        }

        public async Task<Location> ConvertAddressToGeoCoordiantes(string address)
        {
            Location result = null;

            try
            {
                
                Debug.WriteLine("INPUTS TO CONVERT ADDRESS TO GEO COORDINATES: ADDRESS: " + ParseAddressToJustCityState(address));
                Geocoder geoCoder = new Geocoder();
                IEnumerable<Position> approximateLocations = await geoCoder.GetPositionsForAddressAsync(ParseAddressToJustCityState(address));
                Position position = approximateLocations.FirstOrDefault();
                Debug.WriteLine("OUTPUT COORDINATES: LATITUDE: {0}, LONGITUDE: {1}", position.Latitude, position.Longitude);
                result = new Location(position.Latitude, position.Longitude);
            }
            catch (Exception unknowAddress)
            {
                string exception = unknowAddress.Message;
            }

            return result;
        }

        public string ParseAddressToJustCityState(string address)
        {
            string city = "";
            string state = "";
            char[] addressArray = address.ToCharArray();

            for (int i = 0; i < addressArray.Length; i++)
            {
                if(addressArray[i] != ',')
                {
                    city += addressArray[i];
                }
                else
                {
                    for(int j = i + 2; j < addressArray.Length; j++)
                    {
                        if(addressArray[j] != ' ')
                        {
                            state += addressArray[j];
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                }
            }
            return city + ", " + state;
        }

        public async Task<string> getZoneFromLocation(string latitude, string longitude)
        {
            string result = "";

            var client = new HttpClient();
            var endpointCall = await client.GetAsync(Constant.ProduceByLocation + longitude + "," + latitude);
            Debug.WriteLine("PRODUCE BY LOCATION ENDPOINT: " + Constant.ProduceByLocation + longitude + "," + latitude);
            if (endpointCall.IsSuccessStatusCode)
            {
                var contentString = await endpointCall.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ServingFreshBusiness>(contentString);
                Debug.WriteLine("PRODUCE BY LOCATION ENDPOINT CONTENT: " + contentString);
                if (data.result.Count != 0)
                {
                    if (data.business_details.Count != 0)
                    {
                        result = data.business_details[0].zone;
                    }
                }
                else
                {
                    result = "OUTSIDE ZONE RANGE";
                }
            }

            return result;
        }

        public async Task<string> isLocationInZones(string zone, string latitude, string longitude)
        {
            string result = "";

            var client = new HttpClient();
            var endpointCall = await client.GetAsync(Constant.ProduceByLocation + longitude + "," + latitude);

            if (endpointCall.IsSuccessStatusCode)
            {
                var contentString = await endpointCall.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ServingFreshBusiness>(contentString);
                Debug.WriteLine("PRODUCE BY LOCATION ENDPOINT CONTENT: " + contentString);
                if (contentString.Contains("280") && data.result.Count != 0)
                {

                    if (zone == data.business_details[0].zone)
                    {
                        result = "INSIDE CURRENT ZONE";
                    }
                    else
                    {
                        result = "INSIDE DIFFERENT ZONE";
                    }
                }
                else
                {
                    result = "OUTSIDE ZONE RANGE";
                }
            }

           return result;
        }

        public string GetXMLElement(XElement element, string name)
        {
            var el = element.Element(name);
            if (el != null)
            {
                return el.Value;
            }
            return "";
        }

        public string GetXMLAttribute(XElement element, string name)
        {
            var el = element.Attribute(name);
            if (el != null)
            {
                return el.Value;
            }
            return "";
        }

        public void SetPinOnMap(Xamarin.Forms.Maps.Map map, Location location, string address)
        {
            Position position = new Position(location.Latitude, location.Longitude);
            map.MapType = MapType.Street;
            var mapSpan = new MapSpan(position, 0.001, 0.001);

            Pin pin = new Pin();
            pin.Label = address;
            pin.Type = PinType.SearchResult;
            pin.Position = position;

            map.MoveToRegion(mapSpan);
            map.Pins.Add(pin);
        }
    }
}
