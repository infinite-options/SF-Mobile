using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ServingFresh.Models
{
    public class RateOrderDetails
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string question { get; set; }
        public int rateValue { get; set; }
        public ObservableCollection<Star> ratingStarList { get; set; }
        public string comments { get; set; }

        public RateOrderDetails(string question, int j)
        {
            this.question = question == null ? "" : question;
            rateValue = 0;
            comments = "";
            ratingStarList = new ObservableCollection<Star>();

            for (int i = 0; i < 5; i++)
            {
                ratingStarList.Add(new Star { ratingStar = "emptyStar", position = i, ratingStarListIndex = j});
            }
        }
    }
}
