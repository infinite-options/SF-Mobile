using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ServingFresh.Effects;
using Xamarin.Forms;

namespace ServingFresh.Views
{
    public partial class TemplatePage : ContentPage
    {

        GridLength columnWidth;
        ObservableCollection<String> list1;
        ObservableCollection<Filter> filters;

        public class Filter
        {
            public string filterName { get; set; }
            public string iconSource { get; set; }
            public Xamarin.Forms.Color color {get;set;}

            public Filter(string filterName, string iconSource)
            {
                this.filterName = filterName;
                this.iconSource = iconSource;
                color = Color.FromHex("#136D74");
            }
        }

        public TemplatePage()
        {
            InitializeComponent();
            columnWidth = deliveryDatesColumn.Width;

            list1 = new ObservableCollection<string>();
            filters = new ObservableCollection<Filter>();

            list1.Add("one");
            list1.Add("two");
            list1.Add("three");

            filters.Add(new Filter("Fruit", "OrangeIcon.png"));
            filters.Add(new Filter("Vegetables", "VegIcon.png"));
            filters.Add(new Filter("Desserts", "Donut_Icon.png"));
            filters.Add(new Filter("Others", "Bread_Icon.png"));
            filters.Add(new Filter("Favorites", "heartIcon.png"));

            scheduleList.ItemsSource = list1;
            filterList.ItemsSource = filters;
        }

        void ToScheduleView(System.Object sender, System.EventArgs e)
        {
            var initialWidth = new GridLength(0);

            if (deliveryDatesColumn.Width.Equals(initialWidth))
            {
                deliveryDatesColumn.Width = columnWidth;
                filtersColumn.Width = 0;
            }
        }

        void ToFiltersView(System.Object sender, System.EventArgs e)
        {
            var initialWidth = new GridLength(0);

            if(filtersColumn.Width.Equals(initialWidth))
            {
                deliveryDatesColumn.Width = 0;
                filtersColumn.Width = columnWidth;
            }
        }
    }
}
