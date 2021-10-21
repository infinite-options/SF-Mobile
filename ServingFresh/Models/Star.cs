using System;
using System.ComponentModel;

namespace ServingFresh.Models
{
    public class Star : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public int ratingStarListIndex { get; set; }
        public string ratingStar { get; set; }
        public int position { get; set; }

        public string updateRatingStar
        {
            set
            {
                ratingStar = value;
                OnPropertyChanged(nameof(ratingStar));
            }
        }

        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
