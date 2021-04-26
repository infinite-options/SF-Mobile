﻿using System;
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
        private ObservableCollection<AddressAutocomplete> _addresses;
        public event PropertyChangedEventHandler PropertyChanged;
        private CancellationTokenSource throttleCts = new CancellationTokenSource();
        string zip;

        public async Task GetPlacesPredictionsAsync(ListView addressList, ObservableCollection<AddressAutocomplete> Addresses, string _addressText)
        {
            CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token;

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, string.Format(GooglePlacesApiAutoCompletePath, Constant.GooglePlacesApiKey, WebUtility.UrlEncode(_addressText))))
            {

                using (HttpResponseMessage message = await HttpClientInstance.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false))
                {
                    //Debug.WriteLine("GOOGLE PLACES API RESPONSE: " + message.IsSuccessStatusCode);
                    //Debug.WriteLine("GOOGLE PLACES API INPUT 1: " + _addressText);
                    if (message.IsSuccessStatusCode)
                    {
                        string json = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Debug.WriteLine("RESPONSE FROM GOOGLE ADDRESS PREDICTION: " + json);
                        PlacesLocationPredictions predictionList = await Task.Run(() => JsonConvert.DeserializeObject<PlacesLocationPredictions>(json)).ConfigureAwait(false);
                        
                        if (predictionList.Status == "OK")
                        {
                            Addresses.Clear();
                            if (predictionList.Predictions.Count > 0)
                            {
                                foreach (Prediction prediction in predictionList.Predictions)
                                {
                                    string[] predictionSplit = prediction.Description.Split(',');

                                    Console.WriteLine("Place ID: " + prediction.PlaceId);
                                    await setZipcode(prediction.PlaceId);
                                    Console.WriteLine("After setZipcode:\n" + prediction.Description.Trim() + "\n" + predictionSplit[0].Trim() + "\n" + predictionSplit[1].Trim() + "\n" + predictionSplit[2].Trim() + "\n" + zip);
                                    Addresses.Add(new AddressAutocomplete
                                    {
                                        Address = prediction.Description.Trim(),
                                        Street = predictionSplit[0].Trim(),
                                        City = predictionSplit[1].Trim(),
                                        State = predictionSplit[2].Trim(),
                                        ZipCode = zip,
                                    });
                                    addressList.ItemsSource = Addresses;
                                }
                            }
                        }
                        else
                        {
                            Addresses.Clear();
                        }
                    }
                }
            }
        }

        private async Task setZipcode(string placeId)
        {
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
                        zip = components.LongName;
                    }
                }

                Console.WriteLine("Zip code: " + zip);
            }
        }

        public void OnAddressChanged(ListView addressList, ObservableCollection<AddressAutocomplete> Addresses, string _addressText)
        {
            Interlocked.Exchange(ref this.throttleCts, new CancellationTokenSource()).Cancel();
            Task.Delay(TimeSpan.FromMilliseconds(500), this.throttleCts.Token)
                .ContinueWith(
                delegate { GetPlacesPredictionsAsync(addressList, Addresses, _addressText); },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void addressEntryFocused(ListView addressList)
        {
            addressList.IsVisible = true;
            //foreach (Grid g in grids)
            //{
            //    g.IsVisible = false;
            //}
        }

        public void addressEntryUnfocused(ListView addressList)
        {
            addressList.IsVisible = false;

            //foreach (Grid g in grids)
            //{
            //    g.IsVisible = true;
            //}
        }

        public void addressEntryFocused(ListView addressList, Frame frame)
        {
            addressList.IsVisible = true;
            frame.IsVisible = true;
            //foreach (Grid g in grids)
            //{
            //    g.IsVisible = false;
            //}
        }

        public void addressEntryUnfocused(ListView addressList, Frame frame)
        {
            addressList.IsVisible = false;
            frame.IsVisible = false;
            //foreach (Grid g in grids)
            //{
            //    g.IsVisible = true;
            //}
        }

        public AddressAutocomplete addressSelected(ListView addressList, Entry entry, Frame frame)
        {
            AddressAutocomplete selectedAddress = new AddressAutocomplete();
            addressList.IsVisible = false;
            frame.IsVisible = false; 
            entry.Text = ((AddressAutocomplete)addressList.SelectedItem).Street +", " + ((AddressAutocomplete)addressList.SelectedItem).City + ", " + ((AddressAutocomplete)addressList.SelectedItem).State + ", " + ((AddressAutocomplete)addressList.SelectedItem).ZipCode;
            selectedAddress.Street = ((AddressAutocomplete)addressList.SelectedItem).Street;
            selectedAddress.City = ((AddressAutocomplete)addressList.SelectedItem).City;
            selectedAddress.State = ((AddressAutocomplete)addressList.SelectedItem).State;
            selectedAddress.ZipCode = ((AddressAutocomplete)addressList.SelectedItem).ZipCode;
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

            city.Text = ((AddressAutocomplete)addressList.SelectedItem).City;
            state.Text = ((AddressAutocomplete)addressList.SelectedItem).State;
            zipcode.Text = ((AddressAutocomplete)addressList.SelectedItem).ZipCode;

            return selectedAddress;
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
