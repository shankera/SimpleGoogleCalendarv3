using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace SimpleGoogleCalendarv3
{
    public interface ICalendarServiceFacade
    {
        Task<IList<CalendarListEntry>> GetCalendarListItemsExecuteAsyncItems();

        EventsResource.ListRequest GetEventsList(string calendarId);
        Task AddEvent(string calendarId, Event gEvent);

        Task DeleteEvent(string calendarId, string eventId);

        Task<Calendar> GetCalendar(string calendarId);

        Task DeleteCalendar(string calendarId);
    }
}