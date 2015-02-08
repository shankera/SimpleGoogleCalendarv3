using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace SimpleGoogleCalendarv3
{
    public class SimpleCalendarApi : ISimpleApi
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _applicationName;
        public SimpleCalendarApi(string clientId, string clientSecret, string applicationName)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _applicationName = applicationName;
        }
        public async Task<BaseClientService> CreateServiceAsync()
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret
                },
                new[] { CalendarService.Scope.Calendar },
                "user",
                CancellationToken.None,
                new FileDataStore("Calendar.Auth.Store"));

            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName
            });
        }
    }
}
