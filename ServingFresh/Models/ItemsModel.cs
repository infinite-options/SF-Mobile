using System;
using System.ComponentModel;

namespace ServingFresh.Models
{
    public class ItemsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public double height { get; set; }
        public double width { get; set; }

        public string imageSourceLeft { get; set; }
        public string item_uidLeft { get; set; }
        public string itm_business_uidLeft { get; set; }
        public int quantityLeft { get; set; }
        public string itemNameLeft { get; set; }
        public string itemPriceLeft { get; set; }
        public bool isItemLeftVisiable { get; set; }
        public bool isItemLeftEnable { get; set; }

        public string imageSourceRight { get; set; }
        public string item_uidRight { get; set; }
        public string itm_business_uidRight { get; set; }
        public int quantityRight { get; set; }
        public string itemNameRight { get; set; }
        public string itemPriceRight { get; set; }
        public bool isItemRightVisiable { get; set; }
        public bool isItemRightEnable { get; set; }

        public int quantityL
        {
            get { return quantityLeft; }
            set
            {
                quantityLeft = value;
                PropertyChanged(this, new PropertyChangedEventArgs("quantityLeft"));
            }
        }

        public int quantityR
        {
            get { return quantityRight; }
            set
            {
                quantityRight = value;
                PropertyChanged(this, new PropertyChangedEventArgs("quantityRight"));
            }
        }
    }
}
