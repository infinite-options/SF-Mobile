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
        public string itemPriceLeftUnit { get; set; }
        public string itemLeftUnit { get; set; }
        public bool isItemLeftVisiable { get; set; }
        public bool isItemLeftEnable { get; set; }
        // Additional
        public string item_descLeft { get; set; }
        public double item_businessPriceLeft { get; set; }
        public string itemTaxableLeft { get; set; }

        public string imageSourceRight { get; set; }
        public string item_uidRight { get; set; }
        public string itm_business_uidRight { get; set; }
        public int quantityRight { get; set; }
        public string itemNameRight { get; set; }
        public string itemPriceRight { get; set; }
        public string itemPriceRightUnit { get; set; }
        public string itemRightUnit { get; set; }
        public bool isItemRightVisiable { get; set; }
        public bool isItemRightEnable { get; set; }
        // Additional
        public string item_descRight { get; set; }
        public double item_businessPriceRight { get; set; }
        public string itemTaxableRight { get; set; }
        public string itemTypeLeft { get; set; }
        public string itemTypeRight { get; set; }
        public Xamarin.Forms.Color colorLeft { get; set; }
        public Xamarin.Forms.Color colorRight { get; set; }


        public string favoriteIconLeft { get; set; }
        public string favoriteIconRight { get; set; }

        public string favoriteIconLeftUpdate
        {
            set
            {
                favoriteIconLeft = value;
                PropertyChanged(this, new PropertyChangedEventArgs("favoriteIconLeft"));
            }
        }

        public string favoriteIconRightUpdate
        {
            set
            {
                favoriteIconRight = value;
                PropertyChanged(this, new PropertyChangedEventArgs("favoriteIconRight"));
            }
        }

        public Xamarin.Forms.Color colorLeftUpdate
        {
            set
            {
                colorLeft = value;
                PropertyChanged(this, new PropertyChangedEventArgs("colorLeft"));
            }
        }

        public Xamarin.Forms.Color colorRightUpdate
        {
            set
            {
                colorRight = value;
                PropertyChanged(this, new PropertyChangedEventArgs("colorRight"));
            }
        }

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
