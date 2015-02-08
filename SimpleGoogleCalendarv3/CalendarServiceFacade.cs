﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace SimpleGoogleCalendarv3
{
    public class CalendarServiceFacade : ICalendarServiceFacade
    {
        private readonly CalendarService _service;

        public CalendarServiceFacade(CalendarService service)
        {
            _service = service;
        }

        public async Task<IList<CalendarListEntry>> GetCalendarListItemsExecuteAsyncItems()
        {
            var request = await _service.CalendarList.List().ExecuteAsync();
            return request.Items;
        }

        public EventsResource.ListRequest GetEventsList(string calendarId)
        {
            return _service.Events.List(calendarId);
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