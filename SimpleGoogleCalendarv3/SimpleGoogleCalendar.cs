using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
﻿using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
namespace SimpleGoogleCalendarv3
{
    public class SimpleGoogleCalendar : ISimpleCalendar
    {
        public const string DefaultCalendarId = "primary";

        private static CalendarService _service;

        public SimpleGoogleCalendar(CalendarService service)
        {
            _service = service;
        }

        public async Task<IDictionary<string, string>> GetCalendarIdsAsync(string accessLevel)
        {
            var calendarIds = new Dictionary<string, string>();
            var req = await _service.CalendarList.List().ExecuteAsync();
            foreach (var calendarListEntry in req.Items.Where(x => x.AccessRole.Equals(accessLevel)))
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
            var listRequest = _service.Events.List(calendarId);
            listRequest.TimeMin = startDate;
            listRequest.TimeMax = endDate;
            listRequest.SingleEvents = singleEvents;
            var request = await listRequest.ExecuteAsync();

            return request.Items.ToList();
        }

        public async Task AddEventAsync(string calendarId, Event gEvent)
        {
            await _service.Events.Insert(gEvent, calendarId).ExecuteAsync();
        }

        public async Task DeleteEventAsync(string calendarId, string eventId)
        {
            await _service.Events.Delete(calendarId, eventId).ExecuteAsync();
        }

        public async Task<Calendar> GetCalendarAsync(string calendarId)
        {
            return await _service.Calendars.Get(calendarId).ExecuteAsync();
        }

        public async Task DeleteCalendarAsync(string calendarId)
        {
            await _service.Calendars.Delete(calendarId).ExecuteAsync();
        }
    }

}
