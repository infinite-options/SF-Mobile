using System;
using System.Threading.Tasks;

namespace ServingFresh.LogIn.Apple
{
    public interface IAppleSignInService
    {
        bool IsAvailable { get; }
        Task<AppleSignInCredentialState> GetCredentialStateAsync(string userId);
        Task<AppleAccount> SignInAsync();
    }
}
