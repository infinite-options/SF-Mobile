using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using ServingFresh.Config;
using ServingFresh.Models;
using Xamarin.Forms;
using static ServingFresh.Models.RateOrderDetails;
using static ServingFresh.Views.HistoryPage;
using static ServingFresh.Views.PrincipalPage;

namespace ServingFresh.Views
{
    public partial class RateOrderPage : ContentPage
    {
        public ObservableCollection<RateOrderDetails> surveySource;
        public HistoryDisplayObject historyObjectLocal;
        public RateOrderPage()
        {
            InitializeComponent();
            SetRateOrderDetails();
        }

        public RateOrderPage(HistoryDisplayObject historyObject)
        {
            historyObjectLocal = historyObject;
            InitializeComponent();
            SetRateOrderDetails();
        }

        void SetRateOrderDetails()
        {
            surveySource = new ObservableCollection<RateOrderDetails>();

            var questionArray = new[]{
                "How was your overall experience?",
                "How was the freshness of your produce?",
                "How was the delivery?"
            };

            for(int i = 0; i < questionArray.Length; i++)
            { 
                surveySource.Add(new RateOrderDetails(questionArray[i], i));
            }

            BindableLayout.SetItemsSource(myStack, surveySource);
        }

        void ClickOnStar(System.Object sender, System.EventArgs e)
        {
            var imageView = (ImageButton)sender;
            var objectSelected = (Star)imageView.CommandParameter;

            if (AllStarsAreEmpty(surveySource[objectSelected.ratingStarListIndex].ratingStarList))
            {
                for (int i = 0; i <= objectSelected.position; i++)
                {
                    surveySource[objectSelected.ratingStarListIndex].ratingStarList[i].updateRatingStar = "filledStar";
                }
                surveySource[objectSelected.ratingStarListIndex].rateValue = objectSelected.position + 1;
            }
            else
            {
                if (surveySource[objectSelected.ratingStarListIndex].ratingStarList[objectSelected.position].ratingStar == "emptyStar" && PositionOfLastStar(surveySource[objectSelected.ratingStarListIndex].ratingStarList) < objectSelected.position)
                {
                    for (int i = PositionOfLastStar(surveySource[objectSelected.ratingStarListIndex].ratingStarList); i <= objectSelected.position; i++)
                    {
                        surveySource[objectSelected.ratingStarListIndex].ratingStarList[i].updateRatingStar = "filledStar";
                    }
                    surveySource[objectSelected.ratingStarListIndex].rateValue = objectSelected.position + 1;
                }
                else if (surveySource[objectSelected.ratingStarListIndex].ratingStarList[objectSelected.position].ratingStar == "filledStar" && PositionOfLastStar(surveySource[objectSelected.ratingStarListIndex].ratingStarList) > objectSelected.position)
                {
                    for (int i = objectSelected.position + 1; i <= PositionOfLastStar(surveySource[objectSelected.ratingStarListIndex].ratingStarList); i++)
                    {
                        surveySource[objectSelected.ratingStarListIndex].ratingStarList[i].updateRatingStar = "emptyStar";
                    }
                    surveySource[objectSelected.ratingStarListIndex].rateValue = objectSelected.position + 1;
                }
                else
                {

                    if (surveySource[objectSelected.ratingStarListIndex].ratingStarList[objectSelected.position].ratingStar == "filledStar")
                    {
                        for (int i = 1; i <= objectSelected.position; i++)
                        {
                            surveySource[objectSelected.ratingStarListIndex].ratingStarList[i].updateRatingStar = "emptyStar";
                        }
                        surveySource[objectSelected.ratingStarListIndex].rateValue = 1;
                    }
                }
            }
        }
        
        bool AllStarsAreEmpty(ObservableCollection<Star> list)
        {
            bool result = true;
            foreach(Star star in list)
            {
                if(star.ratingStar != "emptyStar")
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        int PositionOfLastStar(ObservableCollection<Star> list)
        {
            var result = 0;

            foreach(Star star in list)
            {
                if(star.ratingStar == "filledStar")
                {
                    result = star.position;
                }
            }

            return result;
        }

        async void SubmitRating(System.Object sender, System.EventArgs e)
        {
            var ratingObject = new RateOrder();
            var ratingArray = new List<int>();

            foreach(RateOrderDetails q in surveySource)
            {
                ratingArray.Add(q.rateValue);
            }

            if(ratingArray[0]> 0)
            { 
                ratingObject.purchase_uid = historyObjectLocal.original_purchase_id;
                ratingObject.comment = commentsView.Text == null ? "" : commentsView.Text;

                var serializeArray = JsonConvert.SerializeObject(ratingArray);

                ratingObject.rating = serializeArray;

                var serializeContent = JsonConvert.SerializeObject(ratingObject);
                var contentString = new StringContent(serializeContent, Encoding.UTF8, "application/json");

                var client = new HttpClient();
                var endpointCall = await client.PostAsync(Constant.RateOrder, contentString);


                if (endpointCall.IsSuccessStatusCode)
                {
                    historyObjectLocal.updateIsRateOrderButtonAvaiable = false;
                    historyObjectLocal.updateIsRateIconAvailable = true;
                    historyObjectLocal.updateRatingSourceIcon = SetIcon(ratingArray[0]);

                    if (ratingArray[0] >= 4)
                    {
                        // display different UI
                        await Navigation.PushModalAsync(new RatingMessagePage(true)); ;
                    }
                    else
                    {
                        // display different UI
                        await Navigation.PushModalAsync(new RatingMessagePage(false));
                    }

                }
                else
                {
                    await DisplayAlert("Oops", "We are not able to process your request at the moment. Please, try again later.", "OK");
                    await Navigation.PopModalAsync();
                }
            }
            else
            {
                await DisplayAlert("Oops", "Please rate your overall experience or click X to exit.", "OK");
            }

        }

        string SetIcon(int ratingValue)
        {
            string result = "";

            if (ratingValue == 0)
            {
                result = "emptyStar";
            }
            else if (ratingValue == 1)
            {
                result = "oneStarRating";
            }
            else if (ratingValue == 2)
            {
                result = "twoStarRating";
            }
            else if (ratingValue == 3)
            {
                result = "threeStarRating";
            }
            else if (ratingValue == 4)
            {
                result = "fourStarRating";
            }
            else if (ratingValue == 5)
            {
                result = "fiveStarRating";
            }

            return result;
        }

        void CloseRatingPage(System.Object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync(true);
        }
    }
}
