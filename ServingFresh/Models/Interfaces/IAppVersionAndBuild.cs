using System;
namespace ServingFresh.Models.Interfaces
{
    public interface IAppVersionAndBuild
    {
        string GetVersionNumber();
        string GetBuildNumber();
    }
}
