using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace SimpleGoogleCalendarv3
{
    class SimpleCalendarApi : ISimpleApi
    {
        public async Task<BaseClientService> CreateServiceAsync(string clientId, string clientSecret, string applicationName)
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                new[] { CalendarService.Scope.Calendar },
                "user",
                CancellationToken.None,
                new FileDataStore("Calendar.Auth.Store"));

            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });
        }
    }
}
