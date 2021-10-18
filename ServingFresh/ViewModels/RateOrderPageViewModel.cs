using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ServingFresh.Models;

namespace ServingFresh.ViewModels
{
    public class RateOrderPageViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        IList<RateOrderDetails> _feedbackSurvey = null;
        string _userComments = string.Empty;

        public IList<RateOrderDetails> FeedbackSurvey { get; set; }

        public string Description { get => FeedbackSurvey[0].question; }

        public string UserCommments
        {
            get => _userComments;
            set
            {
                _userComments = value;
                OnPropertyChaged(nameof(UserCommments));
            }
        }

        public RateOrderPageViewModel()
        {
            FeedbackSurvey = new List<RateOrderDetails>();
            var questionArray = new[]{
                "How was your overall experience?",
                "How was the freshness of your produce?",
                "How was the delivery?"
            };

            foreach (string q in questionArray)
            {
                FeedbackSurvey.Add(new RateOrderDetails(q, 0));
            }
        }

        void OnPropertyChaged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
