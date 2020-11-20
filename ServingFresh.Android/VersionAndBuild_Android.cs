using System;
using Android.Content.PM;
using ServingFresh.Droid;
using ServingFresh.Models.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(VersionAndBuild_Android))]
namespace ServingFresh.Droid
{
    public class VersionAndBuild_Android : IAppVersionAndBuild
    {
        PackageInfo _appInfo;
        public VersionAndBuild_Android()
        {
            var context = Android.App.Application.Context;
            _appInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
        }
        public string GetVersionNumber()
        {
            return _appInfo.VersionName;
        }

        [Obsolete]
        public string GetBuildNumber()
        {
            return _appInfo.VersionCode.ToString();
        }
    }
}
