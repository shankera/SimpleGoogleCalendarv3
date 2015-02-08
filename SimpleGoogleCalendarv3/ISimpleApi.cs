using System.Threading.Tasks;
using Google.Apis.Services;

namespace SimpleGoogleCalendarv3
{
    public interface ISimpleApi
    {
        Task<BaseClientService> CreateServiceAsync();
    }
}