using System;
using System.ComponentModel;
using Xamarin.Forms;

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

        public double opacityLeft { get; set; }
        public double opacityRight { get; set; }

        public bool isItemLeftUnavailable { get; set; }
        public bool isItemRightUnavailable { get; set; }

        public bool isItemLeftUnavailableUpdate
        {
            set
            {
                isItemLeftUnavailable = value;
                PropertyChanged(this, new PropertyChangedEventArgs("isItemLeftUnavailable"));
            }
        }

        public bool isItemRightUnavailableUpdate
        {
            set
            {
                isItemRightUnavailable = value;
                PropertyChanged(this, new PropertyChangedEventArgs("isItemRightUnavailable"));
            }
        }

        public bool isItemLeftEnableUpdate
        {
            set
            {
                isItemLeftEnable = value;
                PropertyChanged(this, new PropertyChangedEventArgs("isItemLeftEnable"));
            }
        }

        public bool isItemRightEnableUpdate
        {
            set
            {
                isItemRightEnable = value;
                PropertyChanged(this, new PropertyChangedEventArgs("isItemRightEnable"));
            }
        }

        public double opacityLeftUpdate
        {
            set
            {
                opacityLeft = value;
                PropertyChanged(this, new PropertyChangedEventArgs("opacityLeft"));
            }
        }

        public double opacityRightUpdate
        {
            set
            {
                opacityRight = value;
                PropertyChanged(this, new PropertyChangedEventArgs("opacityRight"));
            }
        }

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

    public class SingleItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        // String Properties
        public string itemImage{ get; set; }
        public string itemFavoriteImage { get; set; }
        public string itemUID { get; set; }
        public string itemBusinessUID { get; set; }
        public string itemName { get; set; }
        public string itemPrice { get; set; }
        public string itemUnit { get; set; }
        public string itemPriceWithUnit { get; set; }
        public string itemDescription { get; set; }
        public string itemTaxable { get; set; }
        public string itemType { get; set; }

        // Integer Properties
        public int itemQuantity { get; set; }

        // Double Properties
        public double itemBusinessPrice { get; set; }
        public double itemOpacity { get; set; }

        // Bool Properites
        public bool isItemVisiable { get; set; }
        public bool isItemEnable { get; set; }
        public bool isItemUnavailable { get; set; }

        // Color Properties
        public Color itemBackgroundColor { get; set; }

        // Properties that can be changed
        // Propertity: 1
        public string updateItemFavoriteImage
        {
            get { return itemFavoriteImage; }
            set
            {
                itemFavoriteImage = value;
                PropertyChanged(this, new PropertyChangedEventArgs("itemFavoriteImage"));
            }
        }
        // Propertity: 2
        public int updateItemQuantity
        {
            get { return itemQuantity; }
            set
            {
                itemQuantity = value;
                PropertyChanged(this, new PropertyChangedEventArgs("itemQuantity"));
            }
        }
        // Propertity: 3
        public double updateItemOpacity
        {
            set
            {
                itemOpacity = value;
                PropertyChanged(this, new PropertyChangedEventArgs("itemOpacity"));
            }
        }
        // Propertity: 4
        public bool updateIsItemEnable
        {
            set
            {
                isItemEnable = value;
                PropertyChanged(this, new PropertyChangedEventArgs("isItemEnable"));
            }
        }
        // Propertity: 5
        public bool updateIsItemUnavailable
        {
            set
            {
                isItemUnavailable = value;
                PropertyChanged(this, new PropertyChangedEventArgs("isItemUnavailable"));
            }
        }
        // Propertity: 6
        public Color updateItemBackgroundColor
        {
            set
            {
                itemBackgroundColor = value;
                PropertyChanged(this, new PropertyChangedEventArgs("itemBackgroundColor"));
            }
        }
    }
}
