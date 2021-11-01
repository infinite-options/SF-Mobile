using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServingFresh.Config;
using Xamarin.Forms;

namespace ServingFresh.Models
{
    public class Address
    {
        public const string GooglePlacesApiAutoCompletePath = "https://maps.googleapis.com/maps/api/place/autocomplete/json?key={0}&input={1}&components=country:us";
        public const string GooglePlacesApiDetailsPath = "https://maps.googleapis.com/maps/api/place/details/json?key={0}&place_id={1}&fields=address_components";
        private static HttpClient _httpClientInstance;
        public static HttpClient HttpClientInstance => _httpClientInstance ?? (_httpClientInstance = new HttpClient());
        public event PropertyChangedEventHandler PropertyChanged;
        string zip;

        public async Task<string> getZipcode(string placeId)
        {
            string result = null;
            CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token;
            string s = string.Format(GooglePlacesApiDetailsPath, Constant.GooglePlacesApiKey, WebUtility.UrlEncode(placeId));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, s);
            HttpResponseMessage message = await HttpClientInstance.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                string json = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

                Console.WriteLine(json);

                PlacesDetailsResult placesDetailsResult = await Task.Run(() => JsonConvert.DeserializeObject<PlacesDetailsResult>(json)).ConfigureAwait(false);

                foreach (var components in placesDetailsResult.Result.AddressComponents)
                {
                    if (components.Types[0] == "postal_code")
                    {
                        //zip = components.LongName;
                        result =  components.LongName;
                    }
                }

                Console.WriteLine("Zip code: " + result);
            }
            return result;
        }

        public async Task<ObservableCollection<AddressAutocomplete>> GetPlacesPredictionsAsync(string _addressText)
        {
            try
            {
                ObservableCollection<AddressAutocomplete> list = new ObservableCollection<AddressAutocomplete>();
                CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token;

                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, string.Format(GooglePlacesApiAutoCompletePath, Constant.GooglePlacesApiKey, WebUtility.UrlEncode(_addressText))))
                {

                    using (HttpResponseMessage message = await HttpClientInstance.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false))
                    {

                        if (message.IsSuccessStatusCode)
                        {
                            string json = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
                            Debug.WriteLine("RESPONSE FROM GOOGLE ADDRESS PREDICTION: " + json);
                            PlacesLocationPredictions predictionList = await Task.Run(() => JsonConvert.DeserializeObject<PlacesLocationPredictions>(json)).ConfigureAwait(false);

                            if (predictionList.Status == "OK")
                            {
                                if (predictionList.Predictions.Count > 0)
                                {
                                    foreach (Prediction prediction in predictionList.Predictions)
                                    {
                                        string[] predictionSplit = prediction.Description.Split(',');

                                        Console.WriteLine("Place ID: " + prediction.PlaceId);
                                        // comment zipcode 
                                        //await setZipcode(prediction.PlaceId);
                                        Console.WriteLine("After setZipcode:\n" + prediction.Description.Trim() + "\n" + predictionSplit[0].Trim() + "\n" + predictionSplit[1].Trim() + "\n" + predictionSplit[2].Trim() + "\n" + zip);
                                        list.Add(new AddressAutocomplete
                                        {
                                            Address = prediction.Description.Trim(),
                                            Street = predictionSplit[0].Trim(),
                                            City = predictionSplit[1].Trim(),
                                            State = predictionSplit[2].Trim(),
                                            ZipCode = "",
                                            PredictionID = prediction.PlaceId,
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                return list;
                
            }
            catch (Exception prediction)
            {
                Debug.WriteLine("EXCEPTION ON GET PLACE PREDICTION: " + prediction.Message);
                return null;
            }
        }

        public void addressEntryFocused(ListView addressList)
        {
            addressList.IsVisible = true;
        }

        public void addressEntryUnfocused(ListView addressList)
        {
            addressList.IsVisible = false;
        }

        public void addressEntryFocused(ListView addressList, Frame frame)
        {
            addressList.IsVisible = true;
            frame.IsVisible = true;
        }

        public void addressEntryUnfocused(ListView addressList, Frame frame)
        {
            addressList.IsVisible = false;
            frame.IsVisible = false;
        }

        public void addressSelectedFillEntries(AddressAutocomplete selectedAddress, Entry address1, Entry address2, Entry city, Entry state, Entry zipcode)
        {
            address1.Text = selectedAddress.Street;
            address2.Text = selectedAddress.Unit;
            city.Text = selectedAddress.City;
            state.Text = selectedAddress.State;
            zipcode.Text = selectedAddress.ZipCode;
        }

        public AddressAutocomplete addressSelected(ListView addressList, Entry entry, Frame frame)
        {
            AddressAutocomplete selectedAddress = new AddressAutocomplete();
            addressList.IsVisible = false;
            frame.IsVisible = false;
            entry.Text = ((AddressAutocomplete)addressList.SelectedItem).Street + ", " + ((AddressAutocomplete)addressList.SelectedItem).City + ", " + ((AddressAutocomplete)addressList.SelectedItem).State + ", " + ((AddressAutocomplete)addressList.SelectedItem).ZipCode;
            selectedAddress.Street = ((AddressAutocomplete)addressList.SelectedItem).Street;
            selectedAddress.City = ((AddressAutocomplete)addressList.SelectedItem).City;
            selectedAddress.State = ((AddressAutocomplete)addressList.SelectedItem).State;
            selectedAddress.ZipCode = ((AddressAutocomplete)addressList.SelectedItem).ZipCode;
            selectedAddress.PredictionID = ((AddressAutocomplete)addressList.SelectedItem).PredictionID;
            return selectedAddress;
        }

        public AddressAutocomplete addressSelected(ListView addressList, Frame frame)
        {
            AddressAutocomplete selectedAddress = new AddressAutocomplete();
            addressList.IsVisible = false;
            frame.IsVisible = false;
            selectedAddress.Street = ((AddressAutocomplete)addressList.SelectedItem).Street;
            selectedAddress.City = ((AddressAutocomplete)addressList.SelectedItem).City;
            selectedAddress.State = ((AddressAutocomplete)addressList.SelectedItem).State;
            selectedAddress.ZipCode = ((AddressAutocomplete)addressList.SelectedItem).ZipCode;
            selectedAddress.PredictionID = ((AddressAutocomplete)addressList.SelectedItem).PredictionID;
            return selectedAddress;
        }

        public AddressAutocomplete addressSelected(ListView addressList, Entry address, Frame frame, Entry unit, Grid grid, Entry city, Entry state, Entry zipcode)
        {
            AddressAutocomplete selectedAddress = new AddressAutocomplete();

            addressList.IsVisible = false;
            frame.IsVisible = false;
            unit.IsVisible = true;
            grid.IsVisible = true;

            address.Text = ((AddressAutocomplete)addressList.SelectedItem).Street;
            selectedAddress.Address = ((AddressAutocomplete)addressList.SelectedItem).Address;
            selectedAddress.Street = ((AddressAutocomplete)addressList.SelectedItem).Street;
            selectedAddress.City = ((AddressAutocomplete)addressList.SelectedItem).City;
            selectedAddress.State = ((AddressAutocomplete)addressList.SelectedItem).State;
            selectedAddress.ZipCode = ((AddressAutocomplete)addressList.SelectedItem).ZipCode;
            selectedAddress.PredictionID = ((AddressAutocomplete)addressList.SelectedItem).PredictionID;

            city.Text = ((AddressAutocomplete)addressList.SelectedItem).City;
            state.Text = ((AddressAutocomplete)addressList.SelectedItem).State;
            zipcode.Text = ((AddressAutocomplete)addressList.SelectedItem).ZipCode;

            return selectedAddress;
        }

        public void resetAddressEntries(Entry unit, Entry city, Entry state, Entry zipcode)
        {
            unit.Text = null;
            city.Text = null;
            state.Text = null;
            zipcode.Text = null;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
