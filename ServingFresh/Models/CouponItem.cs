using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;

namespace ServingFresh.Models
{
    public class CouponItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string title { get; set; }
        public string image { get; set; }
        public string couponNote { get; set; }
        public string thresholdNote { get; set; }
        public string expNote { get; set; }
        public string savingsOrSpendingNote { get; set; }
        public int index { get; set; }
        public string status { get; set; }
        public double threshold { get; set; }
        public double discount { get; set; }
        public double shipping { get; set; }
        public double totalDiscount { get; set; }
        public string couponId { get; set; }
        public string isCouponEligible { get; set; }
        public Color textColor { get; set; }

        public void update()
        {
            PropertyChanged(this, new PropertyChangedEventArgs("image"));
            PropertyChanged(this, new PropertyChangedEventArgs("textColor"));
            PropertyChanged(this, new PropertyChangedEventArgs("isCouponEligible"));
        }

        public CouponItem()
        {
            title = "";
            image = "";
            couponNote = "";
            thresholdNote = "";
            expNote = "";
            savingsOrSpendingNote = "";
            index = -1;
            status = "";
            threshold = 0;
            discount = 0;
            shipping = 0;
            totalDiscount = 0;
            couponId = "";
            isCouponEligible = "";
            textColor = Color.White;
        }

        public static List<CouponItem> GetActiveCoupons(ObservableCollection<CouponItem> source)
        {
            var listOfActiveCoupons = new List<CouponItem>();

            foreach (CouponItem coupon in source)
            {
                if (coupon.status == "ACTIVE")
                {
                    coupon.image = "eligibleCoupon.png";
                    coupon.savingsOrSpendingNote = "You saved: $" + coupon.totalDiscount.ToString("N2");
                    coupon.isCouponEligible = "Eligible";
                    coupon.textColor = Color.Black;
                    listOfActiveCoupons.Add(coupon);
                }
            }

            return listOfActiveCoupons;
        }

        public static List<CouponItem> GetNonActiveCoupons(ObservableCollection<CouponItem> source, double subtotal)
        {
            var listOfNonActiveCoupons = new List<CouponItem>();

            foreach (CouponItem coupon in source)
            {
                if (coupon.status == "NOT-ACTIVE")
                {
                    coupon.savingsOrSpendingNote = "Spend $" + (coupon.threshold - subtotal).ToString("N2") + " more to use";
                    coupon.isCouponEligible = "Non eligible";
                    coupon.textColor = Color.Gray;
                    listOfNonActiveCoupons.Add(coupon);
                }
            }

            return listOfNonActiveCoupons;
        }

        public static void SortActiveCoupons(List<CouponItem> list)
        {
            SelectionSortByTotalDiscount(list);
            var tempList = new List<CouponItem>();

            foreach (CouponItem coupon in list)
            {
                tempList.Add(coupon);
            }

            int n = tempList.Count;
            int j = 0;
            for (int i = n - 1; i >= 0; i--)
            {
                list[j++] = tempList[i];
            }
        }

        public static void SelectionSortByTotalDiscount(List<CouponItem> list)
        {
            int length = list.Count;

            for (int i = 0; i < length - 1; i++)
            {
                int minimumIndex = i;
                for (int j = i + 1; j < length; j++)
                {
                    if (list[j].totalDiscount < list[minimumIndex].totalDiscount)
                    {
                        minimumIndex = j;
                    }
                }
                var tempCoupon = list[minimumIndex];
                list[minimumIndex] = list[i];
                list[i] = tempCoupon;
            }
        }

        public static void SelectionSortByThreshold(List<CouponItem> list)
        {
            int length = list.Count;

            for (int i = 0; i < length - 1; i++)
            {
                int minimumIndex = i;
                for (int j = i + 1; j < length; j++)
                {
                    if (list[j].threshold < list[minimumIndex].threshold)
                    {
                        minimumIndex = j;
                    }
                }
                var tempCoupon = list[minimumIndex];
                list[minimumIndex] = list[i];
                list[i] = tempCoupon;
            }
        }

        public static void SortNonActiveCoupons(List<CouponItem> list)
        {
            SelectionSortByThreshold(list);
        }

        public static ObservableCollection<CouponItem> MergeActiveNonActiveCouponLists(List<CouponItem> activeList, List<CouponItem> nonActiveList)
        {
            var resultList = new ObservableCollection<CouponItem>();
            foreach (CouponItem coupon in activeList)
            {
                resultList.Add(coupon);
            }

            foreach (CouponItem coupon in nonActiveList)
            {
                resultList.Add(coupon);
            }
            return resultList;
        }
    }
}
