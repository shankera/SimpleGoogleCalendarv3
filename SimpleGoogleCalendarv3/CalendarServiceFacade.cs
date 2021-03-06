﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace SimpleGoogleCalendarv3
{
    public interface ICalendarServiceFacade
    {
        Task<IEnumerable<CalendarListEntry>> GetCalendarListItemsExecuteAsyncItems();

        Task<IEnumerable<Event>> GetEventsList(string calendarId, DateTime startTime, DateTime endTime,
            bool singleEvent);

        Task AddEvent(string calendarId, Event gEvent);

        Task DeleteEvent(string calendarId, string eventId);

        Task<Calendar> GetCalendar(string calendarId);

        Task DeleteCalendar(string calendarId);
    }

    public class CalendarServiceFacade : ICalendarServiceFacade
    {
        private readonly CalendarService _service;

        public CalendarServiceFacade(CalendarService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<CalendarListEntry>> GetCalendarListItemsExecuteAsyncItems()
        {
            var request = await _service.CalendarList.List().ExecuteAsync();
            return request.Items;
        }

        public async Task<IEnumerable<Event>> GetEventsList(string calendarId, DateTime startTime, DateTime endTime, bool singleEvents)
        {
            var listRequest = _service.Events.List(calendarId);
            listRequest.TimeMin = startTime;
            listRequest.TimeMax = endTime;
            listRequest.SingleEvents = singleEvents;
            var request = await listRequest.ExecuteAsync();

            return request.Items;
        }

        public async Task AddEvent(string calendarId, Event gEvent)
        {
            await _service.Events.Insert(gEvent, calendarId).ExecuteAsync();
        }

        public async Task DeleteEvent(string calendarId, string eventId)
        {
            await _service.Events.Delete(calendarId, eventId).ExecuteAsync();
        }

        public async Task<Calendar> GetCalendar(string calendarId)
        {
            return await _service.Calendars.Get(calendarId).ExecuteAsync();
        }

        public async Task DeleteCalendar(string calendarId)
        {
            await _service.Calendars.Delete(calendarId).ExecuteAsync();
        }
    }
}
