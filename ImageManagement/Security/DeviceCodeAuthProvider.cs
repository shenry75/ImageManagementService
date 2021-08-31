using Burkhart.ImageManagement.Core.Models;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Burkhart.ImageManagement.Api.Security
{
    public class DeviceCodeAuthProvider : IAuthenticationProvider
    {

        public interface IDeviceCodeAuthProvider
        {
            Task<string> GetAccessToken();
            Task AuthenticateRequestAsync(HttpRequestMessage requestMessage);
        }

        private IPublicClientApplication _msalClient;
        private string[] _scopes;
        private IAccount _userAccount;
        private readonly AzureStorageConfig _storageConfig;

        public DeviceCodeAuthProvider(string appId, string[] scopes, AzureStorageConfig storageConfig)
        {
            this._scopes = scopes;
            this._storageConfig = storageConfig;

            _msalClient = PublicClientApplicationBuilder
                .Create(_storageConfig.AppId)
                .WithAuthority(AadAuthorityAudience.AzureAdMyOrg, true)
                .WithTenantId(_storageConfig.TenantId)
                .Build();
        }

        public async Task<string> GetAccessToken()
        {
            // If there is no saved user account, the user must sign-in
            if (_userAccount == null)
            {
                try
                {
                    // Invoke device code flow so user can sign-in with a browser
                    var result = await _msalClient.AcquireTokenWithDeviceCode(_scopes, callback => {
                        Console.WriteLine(callback.Message);
                        return Task.FromResult(0);
                    }).ExecuteAsync();

                    _userAccount = result.Account;
                    return result.AccessToken;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error getting access token: {exception.Message}");
                    return null;
                }
            }
            else
            {
                // If there is an account, call AcquireTokenSilent
                // By doing this, MSAL will refresh the token automatically if
                // it is expired. Otherwise it returns the cached token.

                var result = await _msalClient
                    .AcquireTokenSilent(_scopes, _userAccount)
                    .ExecuteAsync();

                return result.AccessToken;
            }
        }

        // This is the required function to implement IAuthenticationProvider
        // The Graph SDK will call this function each time it makes a Graph
        // call.
        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("bearer", await GetAccessToken());
        }
    }
}
