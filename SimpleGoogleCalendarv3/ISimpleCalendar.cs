using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;

namespace SimpleGoogleCalendarv3
{
    public interface ISimpleCalendar
    {
        Task<IDictionary<string, string>> GetCalendarIdsAsync(string accessLevel);

        Task<IEnumerable<Event>> GetEventsAsync(string calendarId, DateTime startDate, DateTime endDate,
            bool singleEvents);

        Task AddEventAsync(string calendarId, Event gEvent);

        Task DeleteEventAsync(string calendarId, string eventId);

        Task<Calendar> GetCalendarAsync(string calendarId);

        Task DeleteCalendarAsync(string calendarId);
    }
}