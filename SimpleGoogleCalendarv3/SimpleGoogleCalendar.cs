using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;
namespace SimpleGoogleCalendarv3
{
    public interface ISimpleCalendar
    {
        Task<IDictionary<string, string>> GetCalendarIdsAsync(CalendarAccess accessLevel);

        Task<IEnumerable<Event>> GetEventsAsync(string calendarId, DateTime startDate, DateTime endDate,
            bool singleEvents);

        Task AddEventAsync(string calendarId, Event gEvent);

        Task DeleteEventAsync(string calendarId, string eventId);

        Task<Calendar> GetCalendarAsync(string calendarId);

        Task DeleteCalendarAsync(string calendarId);
    }

    public class SimpleGoogleCalendar : ISimpleCalendar
    {
        public const string DefaultCalendarId = "primary";

        private static ICalendarServiceFacade _service;

        public SimpleGoogleCalendar(ICalendarServiceFacade csFacade)
        {
            _service = csFacade;
        }
        
        public async Task<IDictionary<string, string>> GetCalendarIdsAsync(CalendarAccess accessLevel)
        {
            var calendarIds = new Dictionary<string, string>();
            var req = await _service.GetCalendarListItemsExecuteAsyncItems();
            foreach (var calendarListEntry in req.Where(x => x.AccessRole.Equals(accessLevel.ToString().ToLower())))
            {
                var calendarId = calendarListEntry.Id;
                var calendarSummary = calendarListEntry.Summary;
                if (calendarIds.ContainsKey(calendarListEntry.Id))
                {
                    calendarId += calendarSummary;
                }
                calendarIds.Add(calendarId, calendarSummary);
            }
            return calendarIds;
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(string calendarId, DateTime startDate, DateTime endDate, bool singleEvents)
        {
            var listRequest = _service.GetEventsList(calendarId);
            listRequest.TimeMin = startDate;
            listRequest.TimeMax = endDate;
            listRequest.SingleEvents = singleEvents;
            var request = await listRequest.ExecuteAsync();

            return request.Items.ToList();
        }

        public async Task AddEventAsync(string calendarId, Event gEvent)
        {
            await _service.AddEvent(calendarId, gEvent);
        }

        public async Task DeleteEventAsync(string calendarId, string eventId)
        {
            await _service.DeleteEvent(calendarId, eventId);
        }

        public async Task<Calendar> GetCalendarAsync(string calendarId)
        {
            return await _service.GetCalendar(calendarId);
        }

        public async Task DeleteCalendarAsync(string calendarId)
        {
            await _service.DeleteCalendar(calendarId);
        }
    }

}
