using System;
using System.Collections.Generic;
using System.Diagnostics;
using ServingFresh.Models.Interfaces;
using Xamarin.Forms;

namespace ServingFresh.Views
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();
            string version = "";
            string build = "";
            version = DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
            build =  DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber();

            versionNumber.Text = "Running App version: " + version;
            buildNumber.Text = "Running App build: " + build;

            // Debug.WriteLine("Running App version #: " + version);
            // Debug.WriteLine("Running App build #: " + build);
        }
    }
}
